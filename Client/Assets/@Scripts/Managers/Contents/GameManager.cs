using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
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
        _playerData.LastStaminaUpdateTime = newData.LastStaminaUpdateTime;
        _playerData.CurrentStage = newData.CurrentStage;

        // TODO ILHAK UI정보 갱신하기
        OnCurrentStageChanged?.Invoke();
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

    public int GetCurrency(Define.ECurrencyType currencyType)
    {
        // None은 처리하지 않음
        if (currencyType == Define.ECurrencyType.None)
            return 0;

        // Define.ECurrencyType은 CurrencyType보다 1 큰 인덱스라고 가정
        CurrencyType type = (CurrencyType)((int)currencyType - 1);
        return GetCurrency(type);
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
        //Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.HeroLevelUp, 1);
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
        //Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.HeroSkillUp, 1);
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
            //Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.DoHeroGacha, count);
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
            //Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.DoHeroGacha, count);
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
            //Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.DoCurrencyGacha, count);
        }
        else
        {
            Debug.LogError($"error: {res.Message}");
        }

        await UpdateCurrencyAsync();
    }
    #endregion

    #region WebStage
    private List<StageClearDTO> _stageClears = new List<StageClearDTO>();

    public async UniTask UpdateStageClearList()
    {
        {
            var req = new StageClearListReq { Jwt = Managers.Web.jwt, };
            StageClearListRes res = await Managers.Web.SendPostRequestAsync<StageClearListRes>("api/game/stage/getClearStageList", req);

            if (res.Success)
            {
                UpdateStageClear(res.Stages);
            }
            else
            {
                Debug.LogError($"error: {res.Message}");
            }
        }

        {
            var req = new PlayerPacketReq()
            {
                jwt = Managers.Web.jwt,
            };

            PlayerPacketRes res = await Managers.Web.SendPostRequestAsync<PlayerPacketRes>("api/game/player", req);

            if (res.Success)
            {
                Managers.Game.UpdatePlayerData(res.PlayerData);
            }
            else
            {
                Debug.LogError("Get Player Failed.");
            }
        }

        {
            await NowStageTemplateIdSet(_playerData.CurrentStage);
        }
    }
    public void UpdateStageClear(List<StageClearDTO> data)
    {
        if (data == null) return;

        // 기존 리스트를 지우고 새 데이터로 교체
        _stageClears.Clear();

        foreach (var stage in data)
        {
            // 깊은 복사를 위해 새로운 객체 생성
            var copy = new StageClearDTO
            {
                TemplateId = stage.TemplateId,
                IsEnable = stage.IsEnable,
                IsClear = stage.IsClear,
            };

            _stageClears.Add(copy);
        }

        Debug.Log($"Stage updated. Total stage: {_stageClears.Count}");
    }

    private int _nowStageTemplateId;
    public int nowStageTemplateId
    { 
        get => _nowStageTemplateId;
        private set
        {
            if( _nowStageTemplateId != value )
            {
                _nowStageTemplateId = value;
                OnCurrentStageChanged?.Invoke();
            }
        }
    }
    public async UniTask NowStageTemplateIdSet(int stageTemplateId)
    {
        if (stageTemplateId == 0)
            return;

        StageClearDTO foundStage = _stageClears.FirstOrDefault(s => s.TemplateId == stageTemplateId);
        bool enable = (foundStage != null) && (foundStage.IsEnable == true);

        if(enable == false)
        {
            if (Managers.Data.StageDataDic[stageTemplateId].PreviewStageId == 0)
                return;

            var prevStage = Managers.Data.StageDataDic[Managers.Data.StageDataDic[stageTemplateId].PreviewStageId];

            var message = $"Need to Clear {prevStage.DifficultyLevel} {prevStage.WorldNumber} - {prevStage.StageNumber}";

            Managers.UI.ShowToast(message, 1f, Define.EToastColor.Red, Define.EToastPosition.MiddleCenter);

            return;
        }

        // 웹서버에 현재 스테이지 저장 요청
        _nowStageTemplateId = stageTemplateId;
        OnCurrentStageChanged?.Invoke();
    }

    public async UniTask ChangeStageNext(bool isNext)
    {
        if (isNext == true)
        {
            var req = new SetNextStageReq()
            {
                Jwt = Managers.Web.jwt,
            };

            SetNextStageRes res = await Managers.Web.SendPostRequestAsync<SetNextStageRes>("api/game/stage/setClearStageNext", req);

            if(res.Success == true)
            {
                nowStageTemplateId = res.StageTemplateId;
            }
            else
            {
                Debug.LogError(res.Message);
            }

            if(res.CanChange == false)
            {
                Managers.UI.ShowToast(res.Message, 1f, Define.EToastColor.Red, Define.EToastPosition.MiddleCenter);
            }
        }
        else
        {
            var req = new SetBackStageReq()
            {
                Jwt = Managers.Web.jwt,
            };

            SetBackStageRes res = await Managers.Web.SendPostRequestAsync<SetBackStageRes>("api/game/stage/setClearStageBack", req);

            if (res.Success == true)
            {
                nowStageTemplateId = res.StageTemplateId;
            }
            else
            {
                Debug.LogError(res.Message);
            }

            if (res.CanChange == false)
            {
                Managers.UI.ShowToast(res.Message, 1f, Define.EToastColor.Red, Define.EToastPosition.MiddleCenter);
            }
        }
    }

    public async UniTask ChangeStageHard()
    {
        var req = new SetHardNormalStageReq()
        {
            Jwt = Managers.Web.jwt,
        };

        SetHardNormalStageRes res = await Managers.Web.SendPostRequestAsync<SetHardNormalStageRes>("api/game/stage/setClearStageHardNormal", req);

        if (res.Success == true)
        {
            nowStageTemplateId = res.StageTemplateId;
        }
        else
        {
            Debug.LogError(res.Message);
        }

        if (res.CanChange == false)
        {
            Managers.UI.ShowToast(res.Message, 1f, Define.EToastColor.Red, Define.EToastPosition.MiddleCenter);
        }
    }
    #endregion

    #region WebStageBattle
    public HeroSnapshot stageHero { get; private set; }
    public List<BuddySnapshot> stageBuddies { get; private set; } = new List<BuddySnapshot>();

    public List<MonsterSnapshot> stageFirstWave { get; private set; } = new List<MonsterSnapshot>();
    public List<MonsterSnapshot> stageSecondWave { get; private set; } = new List<MonsterSnapshot>();
    public List<MonsterSnapshot> stageBossWave { get; private set; } = new List<MonsterSnapshot>();
    public async UniTask SaveStageData(StageStartDataRes data)
    {
        // Save hero
        stageHero = data.Hero;

        // Save buddies
        stageBuddies = new List<BuddySnapshot>(data.Buddies);

        // Save monster waves
        stageFirstWave = new List<MonsterSnapshot>(data.FirstWave);
        stageSecondWave = new List<MonsterSnapshot>(data.SecondWave);
        stageBossWave = new List<MonsterSnapshot>(data.BossWave);
    }

    public BuddySnapshot GetBuddySnapshotData(int templateId)
    {
        foreach (var buddy in stageBuddies)
        {
            if (buddy.TemplateId == templateId)
                return buddy;
        }

        return null;
    }

    public MonsterSnapshot GetMonsterSnapshotData(int templateId, int level)
    {
        var monster = stageFirstWave.Find(m => m.TemplateId == templateId && m.Level == level);
        if (monster != null)
            return monster;

        monster = stageSecondWave.Find(m => m.TemplateId == templateId && m.Level == level);
        if (monster != null)
            return monster;

        monster = stageBossWave.Find(m => m.TemplateId == templateId && m.Level == level);
        if (monster != null)
            return monster;

        // Not found
        return null;
    }

    public async UniTask<List<Reward>> GetStageRewardAsync()
    {
        {
            var req = new StageRewardReq { Jwt = Managers.Web.jwt, };
            StageRewardRes res = await Managers.Web.SendPostRequestAsync<StageRewardRes>("api/game/stage/getStageReward", req);

            if (res.Success)
            {
                List<Reward> rewards = new List<Reward>();
                foreach(var reward in res.Rewards)
                {
                    rewards.Add(new Reward(reward.RewardType, reward.RewardAmount, reward.IsFirst));
                }

                return rewards;
            }
            else
            {
                Debug.LogError($"error: {res.Message}");
            }
        }

        return null;
    }
    #endregion

    #region WebMission

    public class MissionSavedData
    {
        public int TemplateId;
        public int StackedPoint;
        public Define.EMissionState MissionState;
        public List<Define.EMissionState> PointStepMissionState;

        public MissionSavedData(MissionDTO mission)
        {
            TemplateId = mission.TemplateId;
            StackedPoint = mission.StackedPoint;
            MissionState = mission.MissionState;
            PointStepMissionState = new List<Define.EMissionState>();
            for (int i = 0; i < Managers.Data.MissionDataDic[mission.TemplateId].RewardCurrencies.Count; i++)
            {
                if (i < mission.GetRewardCount)
                {
                    PointStepMissionState.Add(Define.EMissionState.Finish);
                    continue;
                }
                
                if(StackedPoint < Managers.Data.MissionDataDic[mission.TemplateId].RewardCurrencies[i].point)
                {
                    PointStepMissionState.Add(Define.EMissionState.Progress);
                }
                else
                {
                    PointStepMissionState.Add(Define.EMissionState.Rewardable);
                }
                    
            }
        }

    }
    public List<MissionSavedData> Missions { get; private set; }
    public List<MissionSavedData> NormalMissions { get; private set; }
    public List<MissionSavedData> DayMissions { get; private set; }
    public List<MissionSavedData> WeekMissions { get; private set; }

    public async UniTask UpdateMission()
    {
        var req = new GetMissionListReq() { Jwt = Managers.Web.jwt, };

        GetMissionListRes res = await Managers.Web.SendPostRequestAsync<GetMissionListRes>("api/game/mission/getMissionList", req);

        if (res.Success == false)
        {
            Debug.LogError(res.Message);
            return;
        }

        UpdateMission(res.Missions);
    }

    public void UpdateMission(List<MissionDTO> missions)
    {
        // Convert to DTOs in one shot
        Missions = missions
            .Select(m => new MissionSavedData(m))
            .ToList();

        // Group by mission type
        var grouped = Missions
            .GroupBy(m => Managers.Data.MissionDataDic[m.TemplateId].MissionType)
            .ToDictionary(g => g.Key, g => g.ToList());

        NormalMissions = grouped.GetValueOrDefault(Define.EMissionType.Normal, new List<MissionSavedData>());
        DayMissions = grouped.GetValueOrDefault(Define.EMissionType.Day, new List<MissionSavedData>());
        WeekMissions = grouped.GetValueOrDefault(Define.EMissionType.Week, new List<MissionSavedData>());

        Managers.Event.TriggerEvent(Define.EEventType.OnMissionChanged);
    }

    public async UniTask ShowMissionPopup()
    {
        await UpdateMission();

        var mission = Managers.UI.ShowPopupUI<UI_MissionPopup>();
        mission.SetInfo();
    }

    public MissionSavedData GetMissionData(int templateId)
    {
        return Missions.FirstOrDefault(m => m.TemplateId == templateId);
    }

    public async UniTask GetMissionSubItemReward(int templateId)
    {
        var req = new GetNormalMissionRewardReq() { Jwt = Managers.Web.jwt, TemplatedId = templateId };

        GetMissionListRes res = await Managers.Web.SendPostRequestAsync<GetMissionListRes>("api/game/mission/getNormalMissionReward", req);

        if(res.Success)
        {
            UpdateMission(res.Missions);
        }
    }

    public async UniTask GetMissionReward(int templateId)
    {
        var req = new GetMissionRewardReq() { Jwt = Managers.Web.jwt, TemplatedId = templateId };

        GetMissionRewardRes res = await Managers.Web.SendPostRequestAsync<GetMissionRewardRes>("api/game/mission/getlMissionReward", req);

        if (res.Success)
        {
            UI_RewardPopup rewardPopup = Managers.UI.ShowPopupUI<UI_RewardPopup>();

            List<Reward> rewards = new List<Reward>();
            foreach (var reward in res.Rewards)
            {
                rewards.Add(new Reward(reward.RewardType, reward.RewardAmount, reward.IsFirst));
            }

            rewardPopup.SetInfo(Define.ERewardType.Mission, rewards);
            
            await UniTask.WhenAll(UpdateMission(), UpdateCurrencyAsync());
        }

    }
    #endregion

    #region WebAchievement
    private List<AchievementDTO> _AchievementSaveDats = new List<AchievementDTO>();
    public List<AchievementDTO> AchievementSaveDats
    {
        get
        {
            return _AchievementSaveDats
            .OrderByDescending(data => data.MissionState == Define.EMissionState.Rewardable)
            //.ThenBy(data => data.TemplateId) // 필요 시 TemplateId 기준 2차 정렬
            .ToList();
        }
    }
    public AchievementDTO GetAchievmentSaveData(int templateId)
    {
        return AchievementSaveDats.FirstOrDefault(m => m.TemplateId == templateId);
    }

    public async UniTask UpdateAchievement()
    {
        var req = new AchievementListReq() { Jwt = Managers.Web.jwt };

        AchievementListRes res = await Managers.Web.SendPostRequestAsync<AchievementListRes>("api/game/achievement/getAchievementList", req);

        if(res.Success == false)
        {
            Debug.LogError(res.Message);
            return;
        }

        _AchievementSaveDats.Clear();
        foreach(var achievement in res.Achievements)
        {
            _AchievementSaveDats.Add(new AchievementDTO
            {
                TemplateId = achievement.TemplateId,
                StackedPoint = achievement.StackedPoint,
                MissionState = achievement.MissionState,
            });
        }

        Managers.Event.TriggerEvent(Define.EEventType.OnMissionChanged);
    }

    public async UniTask ShowAchievementPopup()
    {
        await UpdateAchievement();

        var achievment = Managers.UI.ShowPopupUI<UI_AchievementPopup>();
        achievment.SetInfo();
    }

    public async UniTask GetAchievementReward(int templateId)
    {
        var req = new GetAchievementRewardReq() { Jwt = Managers.Web.jwt, TemplatedId = templateId };

        GetAchievementRewardRes res = await Managers.Web.SendPostRequestAsync<GetAchievementRewardRes>("api/game/achievement/getAchievementReward", req);

        if (res.Success)
        {
            UI_RewardPopup rewardPopup = Managers.UI.ShowPopupUI<UI_RewardPopup>();

            List<Reward> rewards = new List<Reward>();
            foreach (var reward in res.Rewards)
            {
                rewards.Add(new Reward(reward.RewardType, reward.RewardAmount, reward.IsFirst));
            }

            rewardPopup.SetInfo(Define.ERewardType.Mission, rewards);
            
            await UniTask.WhenAll(UpdateAchievement(), UpdateCurrencyAsync());
        }
    }

    #endregion

    #region WebTime
    public async UniTask PlayerTimeCheck()
    {
        var req = new PlayerTimeCheckReq() { Jwt = Managers.Web.jwt };
        PlayerTimeCheckRes res = await Managers.Web.SendPostRequestAsync<PlayerTimeCheckRes>("api/game/player/timecheck", req);
        await UniTask.SwitchToMainThread();
        if (res.Success)
        {
            if(res.PlayerInfo != null)
            {
                UpdatePlayerData(res.PlayerInfo.PlayerData);
            }

            if(res.MissionList != null)
            {
                UpdateMission(res.MissionList.Missions);
            }
        }
    }
    #endregion

    string _path;

    #region Achievement
    List<int> EventValues;
    public HashSet<int> AchievementClearList;
    //private List<AchievementSaveData> _AchievementSaveDats;
    //public List<AchievementSaveData> AchievementSaveDats
    //{
    //    get
    //    {
    //        foreach (var achievement in _AchievementSaveDats)
    //        {
    //            achievement.CheckRewardAble();
    //        }

    //        return _AchievementSaveDats
    //        .OrderByDescending(data => data.MissionState == Define.EMissionState.Rewardable)
    //        //.ThenBy(data => data.TemplateId) // 필요 시 TemplateId 기준 2차 정렬
    //        .ToList();
    //    }
    //    set
    //    {
    //        _AchievementSaveDats = value;
    //    }
    //}
    //public AchievementSaveData GetAchievmentSaveData(int templateId)
    //{
    //    return AchievementSaveDats.FirstOrDefault(m => m.TemplateId == templateId);
    //}

    //public int GetAcievementValue(int templateId)
    //{
    //    var missionGoal = Managers.Data.AchievementDataDic[templateId].MissionGoal;

    //    if (missionGoal == Define.EMissionGoal.MonsterKill)
    //    {
    //        return EventValues[(int)Define.EBroadcastEventType.KillMonster];
    //    }
    //    else if (missionGoal == Define.EMissionGoal.ConsumGold)
    //    {
    //        return EventValues[(int)Define.EBroadcastEventType.UseGold];
    //    }
    //    else if (missionGoal == Define.EMissionGoal.StageClear)
    //    {
    //        return EventValues[(int)Define.EBroadcastEventType.StageClear];
    //    }
    //    else if (missionGoal == Define.EMissionGoal.CurrencyGacha)
    //    {
    //        return EventValues[(int)Define.EBroadcastEventType.DoCurrencyGacha];
    //    }
    //    else if (missionGoal == Define.EMissionGoal.BuddySkillUp)
    //    {
    //        return EventValues[(int)Define.EBroadcastEventType.BuddySkillUp];
    //    }
    //    else if (missionGoal == Define.EMissionGoal.BuddyLevelUp)
    //    {
    //        return EventValues[(int)Define.EBroadcastEventType.BuddyLevelUp];
    //    }
    //    else if (missionGoal == Define.EMissionGoal.HeroSkillUp)
    //    {
    //        return EventValues[(int)Define.EBroadcastEventType.HeroSkillUp];
    //    }
    //    else if (missionGoal == Define.EMissionGoal.HeroLevelUp)
    //    {
    //        return EventValues[(int)Define.EBroadcastEventType.HeroLevelUp];
    //    }
    //    else if (missionGoal == Define.EMissionGoal.StageClearAt)
    //    {
    //        var stageIndex = Managers.Data.AchievementDataDic[templateId].MissionCount;

    //        if (IsStageClearedAt(stageIndex) == true)
    //            return 1;

    //        return 0;
    //    }
    //    else if (missionGoal == Define.EMissionGoal.HeroGacha)
    //    {
    //        return EventValues[(int)Define.EBroadcastEventType.DoHeroGacha];
    //    }
    //    else if (missionGoal == Define.EMissionGoal.BuddyGacha)
    //    {
    //        return EventValues[(int)Define.EBroadcastEventType.DoBuddyGacha];
    //    }

    //    return 0;
    //}

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
        //achievmentSaveData.SetNextAchievment();

        //SaveGame();
        Managers.Event.TriggerEvent(Define.EEventType.OnMissionChanged);
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
}
