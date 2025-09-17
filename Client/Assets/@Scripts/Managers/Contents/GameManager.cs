using Cysharp.Threading.Tasks;
using Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using WebPacket;

public class GameManager
{
    #region WebPlayerData
    private PlayerData _playerData = new PlayerData();
    public PlayerData PlayerData => _playerData;

    public void UpdatePlayerData(PlayerData newData)
    {
        _playerData.PlayerDbId = newData.PlayerDbId;
        _playerData.UserLevel = newData.UserLevel;
        _playerData.UserName = newData.UserName;
        _playerData.Stamina = newData.Stamina;
        _playerData.BGMOn = newData.BGMOn;
        _playerData.EffectSoundOn = newData.EffectSoundOn;
        _playerData.LastMissionTime = newData.LastMissionTime;

        // TODO ILHAK UI정보 갱신하기
    }

    #endregion

    #region WebCurrency
    private int[] _currency = new int[Enum.GetValues(typeof(CurrencyType)).Length];

    public int[] Currency => _currency;
    public void UpdateCurrency(CurrencyData data)
    {
        _currency[(int)CurrencyType.Gold] = data.Gold;
        _currency[(int)CurrencyType.Dia] = data.Dia;
        _currency[(int)CurrencyType.BlueGem] = data.BlueGem;
        _currency[(int)CurrencyType.GreenGem] = data.GreenGem;
        _currency[(int)CurrencyType.YellowGem] = data.YellowGem;

        _currency[(int)CurrencyType.StoneArmor] = data.StoneArmor;
        _currency[(int)CurrencyType.StoneBelt] = data.StoneBelt;
        _currency[(int)CurrencyType.StoneBoots] = data.StoneBoots;
        _currency[(int)CurrencyType.StoneGloves] = data.StoneGloves;
        _currency[(int)CurrencyType.StoneRing] = data.StoneRing;
        _currency[(int)CurrencyType.StoneWeapon] = data.StoneWeapon;

        _currency[(int)CurrencyType.Exp] = data.Exp;

        _currency[(int)CurrencyType.ScrollArmor] = data.ScrollArmor;
        _currency[(int)CurrencyType.ScrollBelt] = data.ScrollBelt;
        _currency[(int)CurrencyType.ScrollBoots] = data.ScrollBoots;
        _currency[(int)CurrencyType.ScrollGloves] = data.ScrollGloves;
        _currency[(int)CurrencyType.ScrollRing] = data.ScrollRing;
        _currency[(int)CurrencyType.ScrollWeapon] = data.ScrollWeapon;

        OnCurrenciesChagned?.Invoke();
    }

    public int GetCurrency(CurrencyType type)
    {
        return _currency[(int)type];
    }

    public async UniTask UpdateCurrencyAsync()
    {
        var req = new CurrencyAllReq()
        {
            jwt = Managers.Web.jwt,
        };

        var res = await Managers.Web.SendPostRequestAsync<CurrencyAllRes>("api/game/currency", req);
        {
            // 4. 서버 응답을 처리하는 콜백 함수입니다.
            if (res.Success)
            {
                UpdateCurrency(res.currencyData);
            }
            else
            {
                Debug.LogError($"Get Currency Failed.");
            }
        }
    }
    #endregion

    #region WebHero
    private int _nowHero;
    public int NowHero
    {
        get => _nowHero;
    }

    public async UniTask NowHeroSetAsync(int value)
    {
        if (value == _nowHero)
        {
            return;
        }

        if (_nowHero == 0)
        {
            _nowHero = value;
            return;
        }

        //Debug.Log($"Now Hero Changed {_nowHero} to {value}");

        // 새로운 영웅 선택으로
        // Web통신으로 변경
        var req = new HeroNowChangeReq { Jwt = Managers.Web.jwt, TemplateId = value };

        HeroListRes res = await Managers.Web.SendPostRequestAsync<HeroListRes>("api/game/hero/nowHeroChange", req);

        if (res.Success)
        {
            _nowHero = value;
            OnNowHeroChanged?.Invoke();
            Debug.Log($"Now Hero Changed Finish {_nowHero}");
        }
        else
        {
            Debug.LogError($"error: {res.Message}");
        }
    }

    private List<HeroDTO> _heroData = new List<HeroDTO>();
    public List<HeroDTO> HeroData => _heroData;
    public async UniTask UpdateHeroData(List<HeroDTO> data)
    {
        if (data == null) return;

        // 기존 리스트를 지우고 새 데이터로 교체
        _heroData.Clear();

        foreach (var hero in data)
        {
            // 깊은 복사를 위해 새로운 객체 생성
            var copy = new HeroDTO
            {
                TemplateId = hero.TemplateId,
                SkillTemplateIds = new List<int>(hero.SkillTemplateIds),
                IsSelected = hero.IsSelected,
                NowExp = hero.NowExp,
                MaxExp = hero.MaxExp
            };

            _heroData.Add(copy);

            if (copy.IsSelected)
            {
                Debug.Log($"Now Hero Id {copy.TemplateId}");

                await NowHeroSetAsync(copy.TemplateId);
            }
        }

        Debug.Log($"HeroData updated. Total heroes: {_heroData.Count}");
    }

    public HeroDTO GetHeroData(int tempalteId)
    {
        foreach (var hero in HeroData)
        {
            if (hero.TemplateId == tempalteId)
                return hero;
        }

        return null;
    }



    public async UniTask HeroLevelUp()
    {
        Debug.Log("Try Hero Level Up");
        var req = new HeroLevelUpReq { Jwt = Managers.Web.jwt, TemplateId = NowHero };
        // Await the web request
        HeroListRes res = await Managers.Web.SendPostRequestAsync<HeroListRes>("api/game/hero/levelUp", req);

        if (res.Success)
        {
            Debug.Log("Success Hero Level Up");
            await UpdateHeroData(res.Heroes);
        }
        else
        {
            Debug.LogError($"error: {res.Message}");
        }

        // TODO ILHAK WebMission
        Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.HeroLevelUp, 1);
    }

    public async UniTask HeroSkillUp(int skillTemplateId)
    {
        var req = new HeroSkillLevelUpReq
        {
            Jwt = Managers.Web.jwt,
            HeroTemplateId = NowHero,
            HeroSkillTemplateId = skillTemplateId
        };

        HeroListRes res = await Managers.Web.SendPostRequestAsync<HeroListRes>("api/game/hero/skillLevelUp", req);

        if (res.Success)
        {
            Debug.Log("Skill upgrade success!");

            // Update hero data
            await UpdateHeroData(res.Heroes);

            // Trigger hero changed event
            OnNowHeroChanged?.Invoke();
        }
        else
        {
            Debug.LogError(res.Message);
        }

        // TODO ILHAK Event
        Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.HeroSkillUp, 1);
    }
    #endregion

    #region WebBuddy
    private List<BuddyDTO> _BuddyData = new List<BuddyDTO>();

    public BuddyDTO GetBuddyData(int templateId)
    {
        foreach (var buddy in _BuddyData)
        {
            if (buddy.TemplateId == templateId)
                return buddy;
        }

        return null;
    }
    public List<BuddyDTO> BuddyData => _BuddyData;
    private int[] _selectedBuddies = new int[4];

    public int SelectedBuddyGet(int index)
    {
        if (index > _selectedBuddies.Length)
            return 0;

        return _selectedBuddies[index];
    }

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

    public async UniTask UpdateBuddyData(int nowBuddy = 0)
    {
        var req = new BuddyListReq()
        {
            Jwt = Managers.Web.jwt,
        };

        BuddyListRes res = await Managers.Web.SendPostRequestAsync<BuddyListRes>("api/game/buddy", req);

        if (res.Success)
        {
            Managers.Game.NowBuddy = nowBuddy;
            await UpdateBuddyData(res.Buddies);
        }
        else
        {
            Debug.LogError($"Get Buddy Failed.");
        }
    }

    public async UniTask UpdateBuddyData(List<BuddyDTO> data)
    {
        if (data == null)
        {
            return;
        }

        // 기존 리스트를 지우고 새 데이터로 교체
        _BuddyData.Clear();

        // _selectedBuddies 초기화
        for (int i = 0; i < _selectedBuddies.Length; i++)
        {
            _selectedBuddies[i] = 0;
        }

        foreach (var buddy in data)
        {
            // 깊은 복사를 위해 새로운 객체 생성
            var copy = new BuddyDTO
            {
                TemplateId = buddy.TemplateId,
                SkillTemplateId = new List<int>(buddy.SkillTemplateId),
                SelectedNumber = buddy.SelectedNumber
            };

            Debug.Log($"Buddy Id {copy.TemplateId}, Slot: {copy.SelectedNumber}");

            _BuddyData.Add(copy);            

            if (copy.SelectedNumber >= 0)
            {
                _selectedBuddies[copy.SelectedNumber] = copy.TemplateId;
            }
        }

        OnNowBuddyChanged?.Invoke();

        Debug.Log($"BuddyData updated. Total buddies: {_BuddyData.Count}");
    }

    public async UniTask SelectedBuddyRemove(int templatedId)
    {
        var req = new BuddySelectedRemoveReq
        {
            Jwt = Managers.Web.jwt,
            TemplateId = templatedId,
        };

        var res = await Managers.Web.SendPostRequestAsync<BuddyListRes>("api/game/buddy/selectedRemove", req);

        if (res.Success)
        {
            await UpdateBuddyData(res.Buddies);
        }
        else
        {
            Debug.LogError(res.Message);
        }
    }

    public async UniTask SelectedBuddyAdd(int templatedId)
    {
        NowBuddy = templatedId;

        var req = new BuddySelectedAddReq
        {
            Jwt = Managers.Web.jwt,
            TemplateId = templatedId,
        };

        var res = await Managers.Web.SendPostRequestAsync<BuddyListRes>("api/game/buddy/selectedAdd", req);

        if (res.Success)
        {
            await UpdateBuddyData(res.Buddies);
        }
        else
        {
            Debug.LogError(res.Message);
        }
    }

    public async UniTask BuddyLevelUp()
    {
        Debug.Log("Try Buddy Level Up");
        var req = new BuddyLevelUpReq { Jwt = Managers.Web.jwt, TemplateId = NowBuddy };

        // Await the web request
        BuddyListRes res = await Managers.Web.SendPostRequestAsync<BuddyListRes>("api/game/buddy/levelUp", req);

        if (res.Success)
        {
            Debug.Log("Success Buddy Level Up");

            // Now Buddy는 로컬 처리
            NowBuddy = Managers.Data.BuddyDataDic[NowBuddy].NextLevelId;

            await UpdateBuddyData(res.Buddies);
        }
        else
        {
            Debug.LogError($"error: {res.Message}");
        }

        // TODO ILHAK Mission
        //Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.BuddyLevelUp, 1);
    }

    public async UniTask BuddySkillUp(int skillTemplateId)
    {
        Debug.Log("Try Buddy Skill Level Up");

        var req = new BuddySkillLevelUpReq { Jwt = Managers.Web.jwt, BuddyTemplateId = NowBuddy, BuddySkillTemplateId = skillTemplateId };

        // Await the web request
        BuddyListRes res = await Managers.Web.SendPostRequestAsync<BuddyListRes>("api/game/buddy/skillUp", req);
        if (res.Success)
        {
            Debug.Log("Success Buddy Skill Level Up");

            await UpdateBuddyData(res.Buddies);
        }
        else
        {
            Debug.LogError($"error: {res.Message}");
        }

        // TODO ILHAK Mission
        //Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.BuddySkillUp, 1);
    }

    #endregion

    #region WebGacha
    public async UniTask DoHeroGacha(int count)
    {
        Debug.Log("Try Hero Gacha");

        var req = new ShopHeroGachaReq { Jwt = Managers.Web.jwt, Count = count };

        // Await the web request
        var res = await Managers.Web.SendPostRequestAsync<ShopHeroGachaRes>("api/game/shop/heroGachaDo", req);
        if (res.Success)
        {
            Debug.Log("Success Hero Gacha");

            var popup = Managers.UI.ShowPopupUI<UI_RewardPopup>();

            List<Reward> rewards = new List<Reward>();

            foreach(var reward in res.Rewards)
            {
                rewards.Add(new Reward((Define.ECurrencyType)((int)reward.Type + 1), reward.Count));
            }

            popup.SetInfo(Define.ERewardType.HeroGacha, rewards);

            // TODO ILHAK Mission
            Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.DoHeroGacha, count);
        }
        else
        {
            Debug.LogError($"error: {res.Message}");
        }

        await UpdateCurrencyAsync();
    }

    public async UniTask DoBuddyGacha(int count)
    {
        Debug.Log("Start Buddy Gacha");

        var req = new ShopBuddyGachaReq { Jwt = Managers.Web.jwt, Count = count };

        // Await the web request
        var res = await Managers.Web.SendPostRequestAsync<ShopBuddyGachaRes>("api/game/shop/buddyGachaDo", req);
        if (res.Success)
        {
            Debug.Log("Success Buddy Gacha");

            var popup = Managers.UI.ShowPopupUI<UI_BuddyGachaPopup>();

            List<BuddyGacha> rewards = new List<BuddyGacha>();

            foreach (var reward in res.Rewards)
            {
                rewards.Add(new BuddyGacha(reward.BuddyName, reward.IsDuplicate));
            }

            popup.SetInfo(rewards);

            // TODO ILHAK Mission
            Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.DoHeroGacha, count);
        }
        else
        {
            Debug.LogError($"error: {res.Message}");
        }

        await UpdateBuddyData();
        await UpdateCurrencyAsync();
    }

    public async UniTask DoCurrencyGacha(int count)
    {
        Debug.Log("Start Currency Gacha");
        
        var req = new ShopCurrencyGachaReq { Jwt = Managers.Web.jwt, Count = count };
        var res = await Managers.Web.SendPostRequestAsync<ShopCurrencyGachaRes>("api/game/shop/currencyGachaDo", req);

        if (res.Success)
        {
            Debug.Log("Success Currency Gacha");

            List<Reward> rewards = new List<Reward>();
            var popup = Managers.UI.ShowPopupUI<UI_RewardPopup>();

            foreach(var reward in res.Rewards)
            {
                rewards.Add(new Reward((Define.ECurrencyType)((int)reward.Type + 1), reward.Count));
            }

            popup.SetInfo(Define.ERewardType.CurrencyGacha, rewards);

            // TODO ILHAK Mission
            Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.DoCurrencyGacha, count);
        }
        else
        {
            Debug.LogError($"error: {res.Message}");
        }

        await UpdateCurrencyAsync();
    }
    #endregion


    string _path;

    #region GameData
    private GameData _gameData = new GameData();

    public int GetCurrency(Define.ECurrencyType currencyType)
    {
        // None은 처리하지 않음
        if (currencyType == Define.ECurrencyType.None)
            return 0;

        // Define.ECurrencyType은 CurrencyType보다 1 큰 인덱스라고 가정
        CurrencyType type = (CurrencyType)((int)currencyType - 1);
        return GetCurrency(type);
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

        if (currencyType == Define.ECurrencyType.Gold && value < 0)
        {
            Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.UseGold, value);
            Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.ChangeGold, value);
        }
        else if (currencyType == Define.ECurrencyType.Gold && value > 0)
        {
            Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.GetGold, value);
            Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.ChangeGold, value);
        }
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

    #region Buddy
    public List<BuddySaveData> buddies { get; private set; }


    public BuddySaveData GetBuddySaveData(int templateId)
    {
        foreach (var buddy in buddies)
        {
            if (buddy.TemplateId == templateId)
                return buddy;
        }

        return null;
    }

    public int RemoveBuddySaveData(int templatedId)
    {
        for (int i = 0; i < buddies.Count; i++)
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
        if (insertIndex < 0)
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
    //public void BuddyLevelUp()
    //{
    //    var buddyData = Managers.Data.BuddyDataDic[NowBuddy];
    //    // 지금 선택된 버디가 레벨업이 가능한지 체크
    //    {
    //        // 다음 레벨이 있어 레벨업 가능한지 확인
    //        if (buddyData.NextLevelId == 0)
    //            return;

    //        // 자원 가능한지 체크
    //        var currencies = buddyData.LevelUpCurrencies;

    //        foreach (var currency in currencies)
    //        {
    //            if (currency.currencyType == Define.ECurrencyType.None)
    //                continue;

    //            if (currency.count > GetCurrency(currency.currencyType))
    //                return;
    //        }

    //        // 자원가능하면 자원 빼고 저장
    //        foreach (var currency in currencies)
    //        {
    //            if (currency.currencyType == Define.ECurrencyType.None)
    //                continue;

    //            AddCurrency(currency.currencyType, -currency.count);
    //        }
    //    }

    //    // 선택된 버디를 레벨업
    //    {
    //        var buddySavedata = _gameData.BuddySaves[NowBuddy];
    //        // 기존 버디 정보를 삭제
    //        int removeIndex = RemoveBuddySaveData(NowBuddy);

    //        // 새로운 버디 정보를 추가
    //        {
    //            buddySavedata.TemplateId = buddyData.NextLevelId;

    //            var nextBuddyData = Managers.Data.BuddyDataDic[buddySavedata.TemplateId];

    //            List<int> orgSkillId = new List<int>();

    //            foreach (int skillId in buddySavedata.SkillTemplateId)
    //            {
    //                orgSkillId.Add(Managers.Data.BuddySkillDataDic[skillId].OriginalLevelId);
    //            }

    //            // 버디의 추가 스킬 정보를 추가
    //            foreach (var skillId in nextBuddyData.SKillIds)
    //            {
    //                if (orgSkillId.Contains(Managers.Data.BuddySkillDataDic[skillId].OriginalLevelId) == false)
    //                {
    //                    buddySavedata.SkillTemplateId.Add(skillId);
    //                }
    //            }

    //            AddBuddySaveData(buddySavedata, removeIndex);
    //        }
    //    }

    //    // 레벨업에 따른 정보 갱신
    //    NowBuddy = buddyData.NextLevelId;

    //    // 세이브
    //    SaveGame();

    //    Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.BuddyLevelUp, 1);
    //}

    //public void BuddySkillUp(int skillTemplateId)
    //{
    //    if (skillTemplateId == 0)
    //        return;

    //    // NowBuddy의 BuddySaveData에 접근 skill의 templateId를 갱신

    //    BuddySaveData currentData = new BuddySaveData();

    //    foreach (var buddy in buddies)
    //    {
    //        if (buddy.TemplateId == NowBuddy)
    //        {
    //            currentData = buddy;
    //            break;
    //        }
    //    }

    //    if (currentData.TemplateId == 0)
    //        return;

    //    var skillData = Managers.Data.BuddySkillDataDic[skillTemplateId];

    //    if (skillData == null)
    //        return;

    //    // 업그레이드 가능한지 체크
    //    {
    //        // 다음 레벨로 진행 가능한가
    //        if (skillData.NextLevelId == 0)
    //            return;

    //        // 자원은 충분한가
    //        var currencies = skillData.LevelUpCurrencies;

    //        foreach (var currency in currencies)
    //        {
    //            if (currency.currencyType == Define.ECurrencyType.None)
    //                continue;

    //            if (currency.count > GetCurrency(currency.currencyType))
    //                return;
    //        }

    //        // 자원가능하면 자원 빼고 저장
    //        foreach (var currency in currencies)
    //        {
    //            if (currency.currencyType == Define.ECurrencyType.None)
    //                continue;

    //            AddCurrency(currency.currencyType, -currency.count);
    //        }
    //    }

    //    // 선택된 스킬 레벨업
    //    {
    //        // 로컬값 수정
    //        var nowSkillIndex = currentData.SkillTemplateId.IndexOf(skillTemplateId);
    //        currentData.SkillTemplateId[nowSkillIndex] = skillData.NextLevelId;

    //        // 세이브 될 값 수정 - 위에서 링크로 수정되었기 때문에 gameData값도 자동 수정됨
    //        //var nowSKillIndexSave = _gameData.BuddySaves[NowBuddy].SkillTemplateId.IndexOf(skillTemplateId);
    //        //_gameData.BuddySaves[NowBuddy].SkillTemplateId[nowSKillIndexSave] = skillData.NextLevelId;

    //        SaveGame();
    //        OnNowBuddyChanged?.Invoke();

    //        Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.BuddySkillUp, 1);
    //    }

    //}
    #endregion

    #region Achievement
    List<int> EventValues;
    public HashSet<int> AchievementClearList;
    private List<AchievementSaveData> _AchievementSaveDats;
    public List<AchievementSaveData> AchievementSaveDats
    {
        get
        {
            foreach (var achievement in _AchievementSaveDats)
            {
                achievement.CheckRewardAble();
            }

            return _AchievementSaveDats
            .OrderByDescending(data => data.MissionState == Define.EMissionState.Rewardable)
            //.ThenBy(data => data.TemplateId) // 필요 시 TemplateId 기준 2차 정렬
            .ToList();
        }
        set
        {
            _AchievementSaveDats = value;
        }
    }
    public AchievementSaveData GetAchievmentSaveData(int templateId)
    {
        return AchievementSaveDats.FirstOrDefault(m => m.TemplateId == templateId);
    }

    public int GetAcievemntValue(int templateId)
    {
        var missionGoal = Managers.Data.AchievementDataDic[templateId].MissionGoal;

        if (missionGoal == Define.EMissionGoal.MonsterKill)
        {
            return EventValues[(int)Define.EBroadcastEventType.KillMonster];
        }
        else if (missionGoal == Define.EMissionGoal.ConsumGold)
        {
            return EventValues[(int)Define.EBroadcastEventType.UseGold];
        }
        else if (missionGoal == Define.EMissionGoal.StageClear)
        {
            return EventValues[(int)Define.EBroadcastEventType.StageClear];
        }
        else if (missionGoal == Define.EMissionGoal.CurrencyGacha)
        {
            return EventValues[(int)Define.EBroadcastEventType.DoCurrencyGacha];
        }
        else if (missionGoal == Define.EMissionGoal.BuddySkillUp)
        {
            return EventValues[(int)Define.EBroadcastEventType.BuddySkillUp];
        }
        else if (missionGoal == Define.EMissionGoal.BuddyLevelUp)
        {
            return EventValues[(int)Define.EBroadcastEventType.BuddyLevelUp];
        }
        else if (missionGoal == Define.EMissionGoal.HeroSkillUp)
        {
            return EventValues[(int)Define.EBroadcastEventType.HeroSkillUp];
        }
        else if (missionGoal == Define.EMissionGoal.HeroLevelUp)
        {
            return EventValues[(int)Define.EBroadcastEventType.HeroLevelUp];
        }
        else if (missionGoal == Define.EMissionGoal.StageClearAt)
        {
            var stageIndex = Managers.Data.AchievementDataDic[templateId].MissionCount;

            if (IsStageClearedAt(stageIndex) == true)
                return 1;

            return 0;
        }
        else if (missionGoal == Define.EMissionGoal.HeroGacha)
        {
            return EventValues[(int)Define.EBroadcastEventType.DoHeroGacha];
        }
        else if (missionGoal == Define.EMissionGoal.BuddyGacha)
        {
            return EventValues[(int)Define.EBroadcastEventType.DoBuddyGacha];
        }

        return 0;
    }

    public void OnHandleBroadcastEventValue(Define.EBroadcastEventType eventType, int value)
    {
        EventValues[(int)eventType] += value;
    }

    public void GetAchievmentReward(int templateId)
    {
        var achievmentSaveData = GetAchievmentSaveData(templateId);
        var achievmentData = Managers.Data.AchievementDataDic[templateId];

        if (achievmentSaveData == null)
            return;

        if (achievmentSaveData.MissionState != Define.EMissionState.Rewardable)
            return;

        if (achievmentData == null)
            return;

        List<Reward> rewardList = new List<Reward>();
        rewardList.Add(new Reward(achievmentData.RewardType, achievmentData.RewardCount));

        if (rewardList.Count == 0)
            return;

        UI_RewardPopup rewardPopup = Managers.UI.ShowPopupUI<UI_RewardPopup>();
        rewardPopup.SetInfo(Define.ERewardType.Mission, rewardList);

        // 클리어한 업적에 추가
        AchievementClearList.Add(templateId);

        // 업적 다음단계로
        achievmentSaveData.SetNextAchievment();

        SaveGame();
        Managers.Event.TriggerEvent(Define.EEventType.OnMissionChanged);
    }


    #endregion

    #region Mission
    public List<MissionSaveData> MissionSaveDatas { get; private set; }
    public List<MissionData> NormalMissionList => Managers.Data.MissionDataDic.Where(mission => mission.Value.MissionType == Define.EMissionType.Normal).Select(mission => mission.Value).ToList();
    public List<MissionData> DayMissionList => Managers.Data.MissionDataDic.Where(mission => mission.Value.MissionType == Define.EMissionType.Day).Select(mission => mission.Value).ToList();
    public List<MissionData> WeekMissionList => Managers.Data.MissionDataDic.Where(mission => mission.Value.MissionType == Define.EMissionType.Week).Select(mission => mission.Value).ToList();

    public MissionSaveData GetMissionSaveData(int templateId)
    {
        return MissionSaveDatas.FirstOrDefault(m => m.TemplateId == templateId);
    }


    public void GetMissionSubItemReward(int templateId)
    {
        var missionSavewData = GetMissionSaveData(templateId);

        if (missionSavewData == null)
            return;

        if (missionSavewData.MissionState != Define.EMissionState.Rewardable)
            return;

        int point = Managers.Data.MissionDataDic[templateId].Point;

        int dayIndex = Managers.Data.MissionDataDic.Values.FirstOrDefault(m => m.MissionType == Define.EMissionType.Day).TemplateId;
        var dayMissionSaveData = GetMissionSaveData(dayIndex);
        dayMissionSaveData.StackedPoint += point;

        if (dayMissionSaveData.StackedPoint > Managers.Data.MissionDataDic[dayIndex].MaxPoint)
        {
            dayMissionSaveData.StackedPoint = Managers.Data.MissionDataDic[dayIndex].MaxPoint;
        }

        int weekIndex = Managers.Data.MissionDataDic.Values.FirstOrDefault(m => m.MissionType == Define.EMissionType.Week).TemplateId;
        var weekMissionSaveData = GetMissionSaveData(weekIndex);
        weekMissionSaveData.StackedPoint += point;

        if (weekMissionSaveData.StackedPoint > Managers.Data.MissionDataDic[weekIndex].MaxPoint)
        {
            weekMissionSaveData.StackedPoint = Managers.Data.MissionDataDic[weekIndex].MaxPoint;
        }

        missionSavewData.MissionState = Define.EMissionState.Finish;

        Managers.Event.TriggerEvent(Define.EEventType.OnMissionChanged);

        SaveGame();
    }

    public void GetMissionReward(int templateId)
    {
        var missionSavewData = GetMissionSaveData(templateId);
        var missionData = Managers.Data.MissionDataDic[templateId];

        if (missionSavewData == null)
            return;

        List<Reward> rewardList = new List<Reward>();
        for (int index = 0; index < missionSavewData.PointStepMissionState.Count; index++)
        {
            if (missionSavewData.StackedPoint >= missionData.RewardCurrencies[index].point && missionSavewData.PointStepMissionState[index] == Define.EMissionState.Progress)
            {
                missionSavewData.PointStepMissionState[index] = Define.EMissionState.Finish;
                rewardList.Add(new Reward(missionData.RewardCurrencies[index].currencyType, missionData.RewardCurrencies[index].count));
            }
        }

        if (rewardList.Count == 0)
            return;

        UI_RewardPopup rewardPopup = Managers.UI.ShowPopupUI<UI_RewardPopup>();
        rewardPopup.SetInfo(Define.ERewardType.Mission, rewardList);

        SaveGame();
        Managers.Event.TriggerEvent(Define.EEventType.OnMissionChanged);
    }

    public void SaveMission(int templateId)
    {
        SaveGame();
    }

    #endregion

    #region Time
    public void SaveMissionTime(DateTime time)
    {
        _gameData.LastMissionTime = time;
        SaveGame();
    }
    #endregion

    #region Reward
    public List<Reward> GetRewards()
    {
        List<Reward> rewards = new List<Reward>();
        var stageData = Managers.Data.StageDataDic[stageTemplateId];

        int enumCount = Enum.GetNames(typeof(Define.ECurrencyType)).Length;
        List<int> currencyCounts = new List<int>(new int[enumCount]);

        System.Random _random = new System.Random();

        for (int i = 0; i < stageData.RewardTimes; i++)
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

        for (int i = 0; i < currencyCounts.Count; i++)
        {
            if (currencyCounts[i] == 0)
                continue;

            rewards.Add(new Reward((Define.ECurrencyType)i, currencyCounts[i]));
            // 여기서 하는게 맞나?
            AddCurrency((Define.ECurrencyType)i, currencyCounts[i]);
        }

        if (_gameData.StageClears[stageTemplateId].isClear == false)
        {
            for (int i = 0; i < stageData.RewardFirstType.Count; i++)
            {
                rewards.Add(new Reward(stageData.RewardFirstType[i], stageData.RewardFirstCount[i], true));
                // 여기서 하는게 맞나?
                AddCurrency(stageData.RewardFirstType[i], stageData.RewardFirstCount[i]);
            }
        }

        return rewards;
    }
    #endregion

    #region Stage
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


    #endregion

    #region StageClear
    public void ClearStage()
    {
        _gameData.StageClears[stageTemplateId].isClear = true;
        if (_gameData.StageClears.ContainsKey(Managers.Data.StageDataDic[stageTemplateId].NextaStageId) == false)
        {
            var newStage = new StageClear();
            newStage.TemplateId = Managers.Data.StageDataDic[stageTemplateId].NextaStageId;
            newStage.isClear = false;
            newStage.isEnable = true;

            _gameData.StageClears.Add(newStage.TemplateId, newStage);
            stageTemplateId = newStage.TemplateId;
        }

        _gameData.CurrentStageTemplateId = stageTemplateId;

        Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.StageClear, 1);

        SaveGame();
    }

    public bool IsStageClearedAt(int templateId)
    {
        if (_gameData.StageClears.ContainsKey(templateId) == false)
            return false;

        return _gameData.StageClears[templateId].isClear;
    }
    #endregion

    #region Gacha
    //public void DoHeroGacha(int count)
    //{
    //    // TODO ILHAK price data
    //    var needDia = 0;

    //    if (count == 1)
    //    {
    //        needDia = 110;
    //    }
    //    else if (count == 10)
    //    {
    //        needDia = 1000;
    //    }

    //    if (needDia == 0)
    //        return;

    //    List<Reward> rewards = new List<Reward>();
    //    System.Random random = new System.Random();

    //    for (int i = 0; i < count; i++)
    //    {
    //        int randomNumber = random.Next(Managers.Data.HeroGachaDataDic.First().Value.Max);

    //        foreach (var heroGachaData in Managers.Data.HeroGachaDataDic.Values)
    //        {
    //            if (heroGachaData.Percent > randomNumber)
    //            {
    //                Debug.Log($"{heroGachaData.CurrencyType} : {heroGachaData.CurrencyCount}");
    //                rewards.Add(new Reward(heroGachaData.CurrencyType, heroGachaData.CurrencyCount));
    //                AddCurrency(heroGachaData.CurrencyType, heroGachaData.CurrencyCount);
    //                break;
    //            }
    //        }

    //        var clear = Managers.UI.ShowPopupUI<UI_RewardPopup>();

    //        clear.SetInfo(Define.ERewardType.HeroGacha, rewards);
    //    }

    //    Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.DoHeroGacha, count);
    //}

    //public void DoCurrencyGacha(int count)
    //{
    //    // TODO ILHAK price data
    //    var needGold = 0;

    //    if (count == 1)
    //    {
    //        needGold = 100;
    //    }
    //    else if (count == 10)
    //    {
    //        needGold = 1000;
    //    }
    //    else if (count == 100)
    //    {
    //        needGold = 10000;
    //    }

    //    if (needGold == 0)
    //        return;

    //    List<Reward> rewards = new List<Reward>();
    //    System.Random random = new System.Random();

    //    for (int i = 0; i < count; i++)
    //    {
    //        int randomNumber = random.Next(Managers.Data.CurrencyGachaDataDic.First().Value.Max);

    //        foreach (var currencyGachaData in Managers.Data.CurrencyGachaDataDic.Values)
    //        {
    //            if (currencyGachaData.Percent > randomNumber)
    //            {
    //                Debug.Log($"{currencyGachaData.CurrencyType} : {currencyGachaData.CurrencyCount}");
    //                rewards.Add(new Reward(currencyGachaData.CurrencyType, currencyGachaData.CurrencyCount));
    //                AddCurrency(currencyGachaData.CurrencyType, currencyGachaData.CurrencyCount);
    //                break;
    //            }
    //        }

    //        var clear = Managers.UI.ShowPopupUI<UI_RewardPopup>();

    //        clear.SetInfo(Define.ERewardType.CurrencyGacha, rewards);
    //    }

    //    Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.DoCurrencyGacha, count);
    //}

    //public void DoBuddyGacha(int count)
    //{
    //    Debug.Log("Start Buddy Gacha");
    //    // TODO ILHAK price data
    //    var needDia = 0;

    //    if (count == 1)
    //    {
    //        needDia = 110;
    //    }
    //    else if (count == 10)
    //    {
    //        needDia = 1000;
    //    }

    //    if (needDia == 0)
    //        return;

    //    List<BuddyGacha> gachaResult = new List<BuddyGacha>();
    //    List<string> buddyNames = new List<string>();
    //    System.Random random = new System.Random();

    //    for (int i = 0; i < count; i++)
    //    {
    //        int randomNumber = random.Next(Managers.Data.BuddyGachaRarityDataDic.First().Value.Max);

    //        Define.ERarityType rarity = Define.ERarityType.None;

    //        foreach (var buddyRarity in Managers.Data.BuddyGachaRarityDataDic.Values)
    //        {
    //            if (buddyRarity.Percent > randomNumber)
    //            {
    //                // 레어리티 결정됨
    //                Debug.Log($"{buddyRarity.RarityType} : {buddyRarity.Percent}");
    //                rarity = buddyRarity.RarityType;
    //                break;
    //            }
    //        }

    //        // 버디 뽑기
    //        if (rarity == Define.ERarityType.Common)
    //        {
    //            int randomBuddyPercent = random.Next(Managers.Data.commonBuddies.Count);
    //            buddyNames.Add(Managers.Data.BuddyGachaDataDic[Managers.Data.commonBuddies[randomBuddyPercent]].GachaItem);
    //        }
    //        else if (rarity == Define.ERarityType.Rare)
    //        {
    //            int randomBuddyPercent = random.Next(Managers.Data.rareBuddies.Count);
    //            buddyNames.Add(Managers.Data.BuddyGachaDataDic[Managers.Data.rareBuddies[randomBuddyPercent]].GachaItem);
    //        }
    //        else if (rarity == Define.ERarityType.Epic)
    //        {
    //            int randomBuddyPercent = random.Next(Managers.Data.epicBuddies.Count);
    //            buddyNames.Add(Managers.Data.BuddyGachaDataDic[Managers.Data.epicBuddies[randomBuddyPercent]].GachaItem);
    //        }
    //        else if (rarity == Define.ERarityType.Unique)
    //        {
    //            int randomBuddyPercent = random.Next(Managers.Data.uniqueBuddies.Count);
    //            buddyNames.Add(Managers.Data.BuddyGachaDataDic[Managers.Data.uniqueBuddies[randomBuddyPercent]].GachaItem);
    //        }
    //        else if (rarity == Define.ERarityType.Legend)
    //        {
    //            int randomBuddyPercent = random.Next(Managers.Data.legendBuddies.Count);
    //            buddyNames.Add(Managers.Data.BuddyGachaDataDic[Managers.Data.legendBuddies[randomBuddyPercent]].GachaItem);
    //        }
    //    }

    //    // 버디 중복 체크
    //    foreach (var buddyName in buddyNames)
    //    {
    //        var buddyData = Managers.Data.BuddyDataDic[Managers.Data.BuddyGachaDataDic[buddyName].BuddyTemplateId];
    //        if (GetBuddySaveData(buddyData.TemplateId) == null)
    //        {
    //            gachaResult.Add(new BuddyGacha(buddyName, false));
    //            AddBuddySaveData(new BuddySaveData(buddyData.TemplateId, null, false));
    //        }
    //        else
    //        {
    //            gachaResult.Add(new BuddyGacha(buddyName, true));
    //            AddCurrency(Managers.Data.BuddyGachaDataDic[buddyName].CurrencyType, Managers.Data.BuddyGachaDataDic[buddyName].CurrencyCount);
    //        }
    //    }

    //    var result = Managers.UI.ShowPopupUI<UI_BuddyGachaPopup>();
    //    result.SetInfo(gachaResult);

    //    Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.DoBuddyGacha, count);
    //}

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

    public void Clear()
    {
        OnNowHeroChanged -= () => UpdateCurrencyAsync().Forget();
        OnNowBuddyChanged -= () => UpdateCurrencyAsync().Forget();
    }

    public void Init()
    {
        OnNowHeroChanged -= () => UpdateCurrencyAsync().Forget();
        OnNowHeroChanged += () => UpdateCurrencyAsync().Forget();

        OnNowBuddyChanged -= () => UpdateCurrencyAsync().Forget();
        OnNowBuddyChanged += () => UpdateCurrencyAsync().Forget();


        _path = Application.persistentDataPath + "/SaveData.json";

        if (LoadGame())
            return;

        // 세이브 파일이 없을 때
        // Mission
        _gameData.MissionSaves.Clear();
        foreach (var mission in Managers.Data.MissionDataDic)
        {
            _gameData.MissionSaves.Add(mission.Value.TemplateId, new MissionSaveData(mission.Value.TemplateId));
        }

        MissionSaveDatas = _gameData.MissionSaves.Values.ToList();

        _gameData.LastMissionTime = DateTime.Now;
        Managers.Time.lastMissionTime = _gameData.LastMissionTime;

        // Achievement
        _gameData.EventValues = Enumerable.Repeat(0, Enum.GetValues(typeof(Define.EBroadcastEventType)).Length).ToList();
        _gameData.AchievementClearList = new HashSet<int>();
        _gameData.AchievementSaveDatas = new List<AchievementSaveData>();

        var sameOriginalAndTemplateIdList = Managers.Data.AchievementDataDic.Values
            .Where(data => data.OriginalAchievementId == data.TemplateId)
            .ToList();

        var previewIdZeroList = sameOriginalAndTemplateIdList
            .Where(data => data.PreviewAchievementId == 0)
            .ToList();

        foreach (var previewId in previewIdZeroList)
        {
            _gameData.AchievementSaveDatas.Add(new AchievementSaveData(previewId.TemplateId));
        }

        EventValues = _gameData.EventValues;
        AchievementClearList = _gameData.AchievementClearList;
        AchievementSaveDats = _gameData.AchievementSaveDatas;

        //// 기본 동료 4개 넣어두기
        //buddies = new List<BuddySaveData>();
        //AddBuddySaveData(new BuddySaveData(100000100, Managers.Data.BuddyDataDic[100000100].SKillIds, true));
        //AddBuddySaveData(new BuddySaveData(300000100, Managers.Data.BuddyDataDic[300000100].SKillIds, true));
        //AddBuddySaveData(new BuddySaveData(100000300, Managers.Data.BuddyDataDic[100000300].SKillIds, true));
        //AddBuddySaveData(new BuddySaveData(100000500, Managers.Data.BuddyDataDic[100000500].SKillIds, true));

        //_gameData.HeroSaves.Add(100, new HeroSaveData(100, Managers.Data.HeroDataDic[100].SKillIds, true));
        //_gameData.HeroSaves.Add(200, new HeroSaveData(200, Managers.Data.HeroDataDic[200].SKillIds, false));

        ////buddies = _gameData.BuddySaves.Values.ToList();
        //int selectedIndex = 0;
        //foreach (var buddy in buddies)
        //{
        //    if (buddy.isSelected == true)
        //    {
        //        _selectedBuddies[selectedIndex++] = buddy.TemplateId;
        //    }
        //}

        //OnSelectedBuddyChanged?.Invoke();

        //var currencyTypes = Enum.GetValues(typeof(Define.ECurrencyType));

        //for (int i = 1; i < currencyTypes.Length; i++)
        //{
        //    AddCurrency((Define.ECurrencyType)i, 100);
        //}

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

        // 미션 가져오기
        MissionSaveDatas = _gameData.MissionSaves.Values.ToList();

        // 업적 가져오기
        EventValues = _gameData.EventValues;
        AchievementClearList = _gameData.AchievementClearList;
        AchievementSaveDats = _gameData.AchievementSaveDatas;

        // 신규 업적 추가
        {
            var filteredList = Managers.Data.AchievementDataDic.Values
                .Where(data => data.OriginalAchievementId == data.TemplateId && data.PreviewAchievementId == 0)
                .ToList();

            var unclearedList = filteredList
                .Where(data => AchievementClearList.Contains(data.TemplateId) == false)
                .ToList();

            foreach (var uncleared in unclearedList)
            {
                // 이미 있다면 추가할 필요가 없으니 체크
                bool alreadyExists = AchievementSaveDats.Any(save => save.TemplateId == uncleared.TemplateId);
                if (alreadyExists == false)
                {
                    AchievementSaveDats.Add(new AchievementSaveData(uncleared.TemplateId));
                }
            }

            SaveGame();
        }


        Managers.Time.lastMissionTime = _gameData.LastMissionTime;

        Debug.Log("Loading Sucess");
        return true;
    }
    #endregion
}
