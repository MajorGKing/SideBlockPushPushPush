using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class GameData
{
    public int UserLevel = 1;
    public string UserName = "Player";

    public int[] Currencies = new int[Enum.GetNames(typeof(Define.ECurrencyType)).Length];

    public int Stamina = Define.MAX_STAMINA;

    public Dictionary<int, BuddySaveData> BuddySaves = new Dictionary<int, BuddySaveData>();
    public Dictionary<int, HeroSaveData> HeroSaves = new Dictionary<int, HeroSaveData>();

    public int CurrentStageTemplateId = 1;
    public Dictionary<int, StageClear> StageClears = new Dictionary<int, StageClear>();

    public bool BGMOn = true;
    public bool EffectSoundOn = true;
}

[Serializable]
public class StageClear
{
    public int TemplateId;
    public bool isEnable;
    public bool isClear;
}

[SerializeField]
public class BuddySaveData
{
    public int TemplateId;
    public List<int> SkillTemplateId;
    public bool isSelected;

    public BuddySaveData()
    {

    }

    public BuddySaveData(int templateId, List<int> skillTemplateId, bool isSelected)
    {
        TemplateId = templateId;
        SkillTemplateId = skillTemplateId;
        this.isSelected = isSelected;
    }
}

[SerializeField]
public class HeroSaveData
{
    public int TemplateId;
    public List<int> SkillTemplateId;
    public bool isSelected;
    public int nowExp;
    public int maxExp;

    public HeroSaveData() { }

    public HeroSaveData(int templateId, List<int> skillTemplateId, bool isSelected)
    {
        TemplateId = templateId;
        SkillTemplateId = skillTemplateId;
        this.nowExp = 0;
        this.maxExp = Managers.Data.HeroDataDic[templateId].LevelUpCurrency1Count;
        this.isSelected = isSelected;
    }
}

public class GameManager
{
    string _path;

    #region GameData
    private GameData _gameData = new GameData();

    public int GetCurrency(Define.ECurrencyType currencyType)
    {
        return _gameData.Currencies[(int)currencyType];
    }

    public void SetCurrency(Define.ECurrencyType currencyType, int value)
    {
        _gameData.Currencies[(int)currencyType] = value;
        SaveGame();
        OnCurrenciesChagned?.Invoke();
    }

    public void AddCurrency(Define.ECurrencyType currencyType, int value)
    {
        _gameData.Currencies[(int)currencyType] += value;
        SaveGame();
        OnCurrenciesChagned?.Invoke();
    }

    public int Stamina
    {
        get { return _gameData.Stamina; }
        set
        {
            _gameData.Stamina = value;
            SaveGame();
            OnCurrenciesChagned?.Invoke();
        }
    }
    #endregion

    #region Hero
    private int _nowHero;
    public int NowHero
    {
        get { return _nowHero; }
        set
        {
            if (value == NowHero)
                return;

            // 기존 선택 영웅 취소
            if(_gameData.HeroSaves.ContainsKey(NowHero))
            {
                _gameData.HeroSaves[NowHero].isSelected = false;
            }
            
            // 새로운 영웅 선택으로
            _nowHero = value;
            _gameData.HeroSaves[NowHero].isSelected = true;
            OnNowHeroChanged?.Invoke();
            SaveGame();
        }
    }

    public List<HeroSaveData> heroes { get; private set; }
    public HeroSaveData GetHeroSaveData(int tempalteId)
    {
        foreach(var hero in heroes)
        {
            if (hero.TemplateId == tempalteId)
                return hero;
        }

        return null;
    }

    public int RemoveHeroSaveData(int templatedId)
    {
        for (int i = 0; i < heroes.Count; i++)
        {
            if (heroes[i].TemplateId == templatedId)
            {
                heroes.RemoveAt(i);

                _gameData.HeroSaves.Remove(templatedId);
                SaveGame();
                return i;
            }
        }

        return -1;
    }

    public void AddHeroSaveData(HeroSaveData heroSaveData, int insertIndex = -1)
    {
        if (insertIndex < 0)
        {
            heroes.Add(heroSaveData);
        }
        else
        {
            heroes.Insert(insertIndex, heroSaveData);
        }


        _gameData.HeroSaves.Add(heroSaveData.TemplateId, heroSaveData);

        SaveGame();
    }
    #endregion

    #region HeroUp
    public void HeroLevelUp()
    {
        var heroData = Managers.Data.HeroDataDic[NowHero];
        // 지금 선택된 허어로가 레벨업 가능한지 체크
        {
            // 다음 레벨이 있어 레벨업 가능한지 확인
            if (heroData.NextLevelId == 0)
                return;

            // 자원 가능한지 체크
            var currencies = heroData.LevelUpCurrencies;

            foreach (var currency in currencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                if (currency.count > GetCurrency(currency.currencyType))
                    return;
            }

            // 자원가능하면 자원 빼고 저장
            foreach (var currency in currencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                AddCurrency(currency.currencyType, -currency.count);
            }
        }

        // 선택된 영웅을 레벨업
        {
            var heroSavedata = _gameData.HeroSaves[NowHero];
            // 기존 영웅 정보를 삭제
            int removeIndex = RemoveHeroSaveData(NowHero);

            // 새로운 영웅 정보를 추가
            {
                heroSavedata.TemplateId = heroData.NextLevelId;

                var nextHeroData = Managers.Data.HeroDataDic[heroSavedata.TemplateId];

                List<int> orgSkillId = new List<int>();

                foreach (int skillId in heroSavedata.SkillTemplateId)
                {
                    orgSkillId.Add(Managers.Data.HeroSkillDataDic[skillId].OriginalLevelId);
                }

                // 버디의 추가 스킬 정보를 추가
                foreach (var skillId in nextHeroData.SKillIds)
                {
                    if (orgSkillId.Contains(Managers.Data.HeroSkillDataDic[skillId].OriginalLevelId) == false)
                    {
                        heroSavedata.SkillTemplateId.Add(skillId);
                    }
                }

                AddHeroSaveData(heroSavedata, removeIndex);
            }
        }

        // 레벨업에 따른 정보 갱신
        NowHero = heroData.NextLevelId;

        // 세이브
        SaveGame();
    }

    public void HeroSkillUp(int skillTemplateId)
    {
        if (skillTemplateId == 0)
            return;

        // NowHero의 HeroSaveData에 접근 skill의 templateId를 갱신

        HeroSaveData currentData = new HeroSaveData();

        foreach (var hero in heroes)
        {
            if (hero.TemplateId == NowHero)
            {
                currentData = hero;
                break;
            }
        }

        if (currentData.TemplateId == 0)
            return;

        var skillData = Managers.Data.HeroSkillDataDic[skillTemplateId];

        if (skillData == null)
            return;

        // 업그레이드 가능한지 체크
        {
            // 다음 레벨로 진행 가능한가
            if (skillData.NextLevelId == 0)
                return;

            // 자원은 충분한가
            var currencies = skillData.LevelUpCurrencies;

            foreach (var currency in currencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                if (currency.count > GetCurrency(currency.currencyType))
                    return;
            }

            // 자원가능하면 자원 빼고 저장
            foreach (var currency in currencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                AddCurrency(currency.currencyType, -currency.count);
            }
        }

        // 선택된 스킬 레벨업
        {
            // 로컬값 수정
            var nowSkillIndex = currentData.SkillTemplateId.IndexOf(skillTemplateId);
            currentData.SkillTemplateId[nowSkillIndex] = skillData.NextLevelId;

            // 세이브 될 값 수정 - 위에서 링크로 수정되었기 때문에 gameData값도 자동 수정됨
            //var nowSKillIndexSave = _gameData.BuddySaves[NowBuddy].SkillTemplateId.IndexOf(skillTemplateId);
            //_gameData.BuddySaves[NowBuddy].SkillTemplateId[nowSKillIndexSave] = skillData.NextLevelId;

            SaveGame();
            OnNowHeroChanged?.Invoke();
        }

    }
    #endregion

    #region Buddy
    private int _nowBuddy;
    public int NowBuddy
    {
        get { return _nowBuddy; }
        set
        {
            if (value == NowBuddy)
                return;

            _nowBuddy = value;
            OnNowBuddyChanged?.Invoke();
        }
    }

    public List<BuddySaveData> buddies { get; private set; }
    private int[] _selectedBuddies = new int[4];
    
    public BuddySaveData GetBuddySaveData(int templateId)
    {
        foreach(var buddy in buddies)
        {
            if (buddy.TemplateId == templateId)
                return buddy;
        }

        return null;
    }

    public int RemoveBuddySaveData(int templatedId)
    {
        for(int i = 0; i < buddies.Count; i++)
        {
            if (buddies[i].TemplateId == templatedId)
            {
                buddies.RemoveAt(i);

                _gameData.BuddySaves.Remove(templatedId);
                SaveGame();
                return i;
            }
        }

        return -1;
    }

    public void AddBuddySaveData(BuddySaveData buddySaveData, int insertIndex = -1)
    {
        if(insertIndex < 0)
        {
            buddies.Add(buddySaveData);
        }
        else
        {
            buddies.Insert(insertIndex, buddySaveData);
        }
        

        _gameData.BuddySaves.Add(buddySaveData.TemplateId, buddySaveData);

        // 만약 셀렉트된 버디(전 레벨)가 있다면 최신 버디로 갱신 해준다
        {
            var previewIndex = Managers.Data.BuddyDataDic[buddySaveData.TemplateId].PreviewLevelId;

            var selectedIndex = Array.IndexOf(_selectedBuddies, previewIndex);

            // 만약 해당하는 내용이 있다면 갱신해준다
            if (selectedIndex >= 0)
            {
                _selectedBuddies[selectedIndex] = buddySaveData.TemplateId;
                OnSelectedBuddyChanged?.Invoke();
            }
        }


        SaveGame();
    }

    public int SelectedBuddyGet(int index)
    {
        if (index > _selectedBuddies.Length)
            return 0;

        return _selectedBuddies[index];
    }

    public bool SelectedBuddyRemove(int templatedId)
    {
        for (int i = 0; i < _selectedBuddies.Length; i++)
        {
            if (_selectedBuddies[i] == templatedId)
            {
                _selectedBuddies[i] = 0;
                SetSelectdBuddy(templatedId, false);

                int writeIndex = 0;
                for (int j = 0; j < _selectedBuddies.Length; j++)
                {
                    // 0이 아닌 요소만 writeIndex 위치에 복사
                    if (_selectedBuddies[j] != 0)
                    {
                        _selectedBuddies[writeIndex] = _selectedBuddies[j];
                        writeIndex++;
                    }
                }
                // 남은 공간을 0으로 채움
                for (int k = writeIndex; k < _selectedBuddies.Length; k++)
                {
                    _selectedBuddies[k] = 0;
                }

                OnSelectedBuddyChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    public void SelectedBuddySet(int templatedId)
    {
        NowBuddy = templatedId;

        if (_selectedBuddies.Contains(templatedId))
            return;

        for (int i = 0; i < _selectedBuddies.Length; i++)
        {
            if (_selectedBuddies[i] == 0)
            {
                _selectedBuddies[i] = templatedId;
                SetSelectdBuddy(templatedId, true);
                return;
            }
        }
    }

    private void SetSelectdBuddy(int templatedId, bool selected)
    {
        _gameData.BuddySaves[templatedId].isSelected = selected;
        OnSelectedBuddyChanged?.Invoke();
        SaveGame();
    }
    #endregion

    #region BuddyUp
    public void BuddyLevelUp()
    {
        var buddyData = Managers.Data.BuddyDataDic[NowBuddy];
        // 지금 선택된 버디가 레벨업이 가능한지 체크
        {
            // 다음 레벨이 있어 레벨업 가능한지 확인
            if (buddyData.NextLevelId == 0)
                return;

            // 자원 가능한지 체크
            var currencies = buddyData.LevelUpCurrencies;

            foreach (var currency in currencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                if (currency.count > GetCurrency(currency.currencyType))
                    return;
            }

            // 자원가능하면 자원 빼고 저장
            foreach (var currency in currencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                AddCurrency(currency.currencyType, -currency.count);
            }
        }

        // 선택된 버디를 레벨업
        {
            var buddySavedata = _gameData.BuddySaves[NowBuddy];
            // 기존 버디 정보를 삭제
            int removeIndex = RemoveBuddySaveData(NowBuddy);

            // 새로운 버디 정보를 추가
            {
                buddySavedata.TemplateId = buddyData.NextLevelId;

                var nextBuddyData = Managers.Data.BuddyDataDic[buddySavedata.TemplateId];

                List<int> orgSkillId = new List<int>();

                foreach(int skillId in buddySavedata.SkillTemplateId)
                {
                    orgSkillId.Add(Managers.Data.BuddySkillDataDic[skillId].OriginalLevelId);
                }

                // 버디의 추가 스킬 정보를 추가
                foreach(var skillId in nextBuddyData.SKillIds)
                {
                    if (orgSkillId.Contains(Managers.Data.BuddySkillDataDic[skillId].OriginalLevelId) == false)
                    {
                        buddySavedata.SkillTemplateId.Add(skillId);
                    }
                }

                AddBuddySaveData(buddySavedata, removeIndex);
            }
        }

        // 레벨업에 따른 정보 갱신
        NowBuddy = buddyData.NextLevelId;

        // 세이브
        SaveGame();
    }

    public void BuddySkillUp(int skillTemplateId)
    {
        if (skillTemplateId == 0)
            return;

        // NowBuddy의 BuddySaveData에 접근 skill의 templateId를 갱신

        BuddySaveData currentData = new BuddySaveData();

        foreach(var buddy in buddies)
        {
            if(buddy.TemplateId == NowBuddy)
            {
                currentData = buddy;
                break;
            }
        }

        if (currentData.TemplateId == 0)
            return;

        var skillData = Managers.Data.BuddySkillDataDic[skillTemplateId];

        if (skillData == null)
            return;

        // 업그레이드 가능한지 체크
        {
            // 다음 레벨로 진행 가능한가
            if (skillData.NextLevelId == 0)
                return;

            // 자원은 충분한가
            var currencies = skillData.LevelUpCurrencies;

            foreach (var currency in currencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                if (currency.count > GetCurrency(currency.currencyType))
                    return;
            }

            // 자원가능하면 자원 빼고 저장
            foreach (var currency in currencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                AddCurrency(currency.currencyType, -currency.count);
            }
        }

        // 선택된 스킬 레벨업
        {
            // 로컬값 수정
            var nowSkillIndex = currentData.SkillTemplateId.IndexOf(skillTemplateId);
            currentData.SkillTemplateId[nowSkillIndex] = skillData.NextLevelId;

            // 세이브 될 값 수정 - 위에서 링크로 수정되었기 때문에 gameData값도 자동 수정됨
            //var nowSKillIndexSave = _gameData.BuddySaves[NowBuddy].SkillTemplateId.IndexOf(skillTemplateId);
            //_gameData.BuddySaves[NowBuddy].SkillTemplateId[nowSKillIndexSave] = skillData.NextLevelId;

            SaveGame();
            OnNowBuddyChanged?.Invoke();
        }

    }
    #endregion

    #region Action
    public event Action OnCurrenciesChagned;
    public event Action OnCurrentStageChanged;
    public event Action OnNowBuddyChanged;
    public event Action OnNowHeroChanged;
    public event Action OnSelectedBuddyChanged;
    #endregion


    private GameScene _scene;
    private bool _nowGameScene = false;

    private int _stageTemplateId;
    public int stageTemplateId
    {
        get { return _stageTemplateId; }
        set 
        {
            if (value == 0)
                return;

            if (_gameData.StageClears.ContainsKey(value) == false || _gameData.StageClears[value].isEnable == false)
            {
                if (Managers.Data.StageDataDic[value].PreviewStageId == 0)
                    return;

                var prevStage = Managers.Data.StageDataDic[Managers.Data.StageDataDic[value].PreviewStageId];

                var message = $"Need to Clear {prevStage.DifficultyLevel} {prevStage.WorldNumber} - {prevStage.StageNumber}";

                Managers.UI.ShowToast(message, 1f, Define.EToastColor.Red, Define.EToastPosition.MiddleCenter);

                return;
            }

            _stageTemplateId = value;
            _gameData.CurrentStageTemplateId = value;
            OnCurrentStageChanged?.Invoke();
            SaveGame();
        }
    }

    public void Init()
    {
        _path = Application.persistentDataPath + "/SaveData.json";

        if (LoadGame())
            return;

        // 세이브 파일이 없을 때
        // 기본 동료 4개 넣어두기
        _gameData.BuddySaves.Add(1, new BuddySaveData(1, Managers.Data.BuddyDataDic[1].SKillIds, true));
        _gameData.BuddySaves.Add(3, new BuddySaveData(3, Managers.Data.BuddyDataDic[3].SKillIds, true));
        _gameData.BuddySaves.Add(5, new BuddySaveData(5, Managers.Data.BuddyDataDic[5].SKillIds, true));
        _gameData.BuddySaves.Add(7, new BuddySaveData(7, Managers.Data.BuddyDataDic[7].SKillIds, true));

        _gameData.HeroSaves.Add(100, new HeroSaveData(100, Managers.Data.HeroDataDic[100].SKillIds, true));
        _gameData.HeroSaves.Add(200, new HeroSaveData(200, Managers.Data.HeroDataDic[200].SKillIds, false));

        buddies = _gameData.BuddySaves.Values.ToList();
        int selectedIndex = 0;
        foreach (var buddy in buddies)
        {
            if (buddy.isSelected == true)
            {
                _selectedBuddies[selectedIndex++] = buddy.TemplateId;
            }
        }

        heroes = _gameData.HeroSaves.Values.ToList();
        foreach (var hero in heroes)
        {
            if(hero.isSelected == true)
            {
                NowHero = hero.TemplateId;
            }
        }

        OnSelectedBuddyChanged?.Invoke();

        var currencyTypes = Enum.GetValues(typeof(Define.ECurrencyType));

        for (int i = 1; i < currencyTypes.Length; i++)
        {
            AddCurrency((Define.ECurrencyType)i, 100);
        }

        StageClear stage = new StageClear();
        stage.TemplateId = 1;
        stage.isEnable = true;
        stage.isClear = false;
        _gameData.StageClears.Add(1, stage);


        PlayerPrefs.SetInt("ISFIRST", 0);
        //PlayerPrefs.Save();

        stageTemplateId = _gameData.CurrentStageTemplateId;
    }

    public void Update()
    {
        if (_scene == null)
            return;

        if (_nowGameScene == false)
            return;

        // 입력 처리
        UpdateInput();
    }

    public void GameSceneStart(GameScene scene)
    {
        _scene = scene;
        _nowGameScene = true;
    }

    public void GameSceneEnd()
    {
        _scene = null;
        _nowGameScene = false;
    }

    private void UpdateInput()
    {
        if (IsPointerOverUIObject(Input.mousePosition))
            return;

        if (Input.GetMouseButtonDown(0))
        {
            
        }
        else if (Input.GetMouseButtonUp(0))
        {
            //Debug.Log("Touch Position: " + Input.mousePosition);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null)
            {
                if (hit.transform.TryGetComponent<LineTouchController>(out LineTouchController lineTouch))
                {
                    var lineNum = lineTouch.LineTouched();
                    _scene.LineTouched(lineNum);
                }
            }
        }
    }

    public bool IsPointerOverUIObject(Vector2 touchPos)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = touchPos;
        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0;
    }

    #region Reward
    public List<Reward> GetRewards()
    {
        List<Reward> rewards = new List<Reward>();
        var stageData = Managers.Data.StageDataDic[stageTemplateId];

        int enumCount = Enum.GetNames(typeof(Define.ECurrencyType)).Length;
        List<int> currencyCounts = new List<int>(new int[enumCount]);

        System.Random _random = new System.Random();

        for(int i = 0; i < stageData.RewardTimes; i++)
        {
            int totalWeight = 0;
            foreach (int weight in stageData.RewardPercent)
                totalWeight += weight;

            int rand = _random.Next(0, totalWeight);
            int cumulative = 0;

            for (int j = 0; j < stageData.RewardPercent.Count; j++)
            {
                cumulative += stageData.RewardPercent[j];
                if (rand < cumulative)
                {
                    Define.ECurrencyType currencyType = stageData.RewardType[j];
                    int rewardCount = stageData.RewardCount[j];
                    currencyCounts[(int)currencyType] += rewardCount;
                    break;
                }
            }
        }

        for(int i = 0; i < currencyCounts.Count; i++)
        {
            if(currencyCounts[i] == 0)
                continue;

            rewards.Add(new Reward((Define.ECurrencyType)i, currencyCounts[i]));
            // 여기서 하는게 맞나?
            AddCurrency((Define.ECurrencyType)i, currencyCounts[i]);
        }

        if (_gameData.StageClears[stageTemplateId].isClear == false)
        {
            for(int i = 0; i < stageData.RewardFirstType.Count; i++)
            {
                rewards.Add(new Reward(stageData.RewardFirstType[i], stageData.RewardFirstCount[i], true));
                // 여기서 하는게 맞나?
                AddCurrency(stageData.RewardFirstType[i], stageData.RewardFirstCount[i]);
            }
        }

        return rewards;
    }
    #endregion

    #region StageClear
    public void ClearStage()
    {
        _gameData.StageClears[stageTemplateId].isClear = true;
        if(_gameData.StageClears.ContainsKey(Managers.Data.StageDataDic[stageTemplateId].NextaStageId) == false)
        {
            var newStage = new StageClear();
            newStage.TemplateId = Managers.Data.StageDataDic[stageTemplateId].NextaStageId;
            newStage.isClear = false;
            newStage.isEnable = true;

            _gameData.StageClears.Add(newStage.TemplateId, newStage);
            stageTemplateId = newStage.TemplateId;
        }

        _gameData.CurrentStageTemplateId = stageTemplateId;

        SaveGame();
    }
    #endregion

    #region SaveLoad
    public void SaveGame()
    {
        string jsonStr = JsonConvert.SerializeObject(_gameData);
        File.WriteAllText(_path, jsonStr);

        Debug.Log("Save Sucess");
    }

    public bool LoadGame()
    {
        if (PlayerPrefs.GetInt("ISFIRST", 1) == 1)
        {
            string path = Application.persistentDataPath + "/SaveData.json";
            if (File.Exists(path))
                File.Delete(path);
            return false;
        }

        if (File.Exists(_path) == false)
            return false;

        string fileStr = File.ReadAllText(_path);
        GameData data = JsonConvert.DeserializeObject<GameData>(fileStr);
        if (data != null)
            _gameData = data;

        //IsLoaded = true;

        stageTemplateId = _gameData.CurrentStageTemplateId;

        // 영웅, 동료 관련 처리

        // 동료 정보 가저오기
        buddies = _gameData.BuddySaves.Values.ToList();
        int i = 0;
        foreach ( var buddy in buddies )
        {
            if( buddy.isSelected == true )
            {
                _selectedBuddies[i++] = buddy.TemplateId;
            }
        }

        // 영웅 가저오기
        heroes = _gameData.HeroSaves.Values.ToList();
        foreach (var hero in heroes)
        {
            if (hero.isSelected == true)
            {
                NowHero = hero.TemplateId;
            }
        }

        OnSelectedBuddyChanged?.Invoke();

        Debug.Log("Loading Sucess");
        return true;
    }
    #endregion
}
