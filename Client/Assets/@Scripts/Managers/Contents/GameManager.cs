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

    public List<HeroController> Heroes = new List<HeroController>();

    public Dictionary<int, BuddySaveData> BuddySaves = new Dictionary<int, BuddySaveData>();

    public HeroController SelectedHero;
    
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

    private HeroController _nowHero;
    public HeroController NowHero
    {
        get { return _nowHero; }
        set
        {
            if (value == NowHero)
                return;

            _nowHero = value;
            OnNowHeroChanged?.Invoke();
        }
    }

    //private BuddyController[] _selectedBuddy;
    //public BuddyController[] SelectedBuddy
    //{
    //    get { return _selectedBuddy; }       
    //}
    #endregion

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
    

    public void SelectedBuddyRemove(int templatedId)
    {
        for (int i = 0; i < _selectedBuddies.Length; i++)
        {
            if (_selectedBuddies[i] == templatedId)
            {
                _selectedBuddies[i] = 0;
                SetSelectdBuddy(templatedId, false);
                return;
            }
        }
    }

    public void SelectedBuddySet(int templatedId)
    {
        NowBuddy = templatedId;

        for (int i = 0; i < _selectedBuddies.Length; i++)
        {
            if (_selectedBuddies[i] != 0)
            {
                _selectedBuddies[i] = templatedId;
                SetSelectdBuddy(templatedId, true);
                return;
            }
        }
    }

    private void SetSelectdBuddy(int templatedId, bool isSelected)
    {
        _gameData.BuddySaves[templatedId].isSelected = false;
        SaveGame();
    }

    #region Action
    public event Action OnCurrenciesChagned;
    public event Action OnCurrentStageChanged;
    public event Action OnNowBuddyChanged;
    public event Action OnNowHeroChanged;
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

        buddies = _gameData.BuddySaves.Values.ToList();
        int i = 0;
        foreach (var buddy in buddies)
        {
            if (buddy.isSelected == true)
            {
                _selectedBuddies[i++] = buddy.TemplateId;
            }
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

        Debug.Log("Loading Sucess");
        return true;
    }
    #endregion
}
