using Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

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

    public Dictionary<int, MissionSaveData> MissionSaves = new Dictionary<int, MissionSaveData>();

    public bool BGMOn = true;
    public bool EffectSoundOn = true;

    public DateTime LastMissionTime;
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

[SerializeField]
public class MissionSaveData
{
    public int TemplateId;
    public int StackedPoint;
    public Define.EMissionState MissionState;
    public List<Define.EMissionState> PointStepMissionState;

    public MissionSaveData(int templateId)
    {
        TemplateId = templateId;
        StackedPoint = 0;
        MissionState = Define.EMissionState.Progress;
        PointStepMissionState = new List<Define.EMissionState>();
        for (int i = 0; i < Managers.Data.MissionDataDic[templateId].RewardCurrencies.Count; i++)
        {
            PointStepMissionState.Add(Define.EMissionState.Progress);
        }
    }

    public void OnHandleBroadcastMissionEvent(Define.EBroadcastEventType eventType, int value)
    {
        if (MissionState != Define.EMissionState.Progress)
            return;

        switch(Managers.Data.MissionDataDic[TemplateId].MissionGoal)
        {
            case Define.EMissionGoal.MonsterKill:
                if(eventType == Define.EBroadcastEventType.KillMonster)
                {
                    StackedPoint += value;
                    Managers.Game.SaveMission(TemplateId);
                }
                break;
            case Define.EMissionGoal.ConsumGold:
                if (eventType == Define.EBroadcastEventType.UseGold)
                {
                    StackedPoint += value;
                }
                break;
            case Define.EMissionGoal.StageClear:
                if (eventType == Define.EBroadcastEventType.StageClear)
                {
                    StackedPoint += value;
                }
                break;
            case Define.EMissionGoal.CurrencyGacha:
                if (eventType == Define.EBroadcastEventType.DoCurrencyGacha)
                {
                    StackedPoint += value;
                }
                break;
            case Define.EMissionGoal.BuddySkillUp:
                if (eventType == Define.EBroadcastEventType.BuddySkillUp)
                {
                    StackedPoint += value;
                }
                break;
            case Define.EMissionGoal.BuddyLevelUp:
                if (eventType == Define.EBroadcastEventType.BuddyLevelUp)
                {
                    StackedPoint += value;
                }
                break;
            case Define.EMissionGoal.HeroSkillUp:
                if (eventType == Define.EBroadcastEventType.HeroSkillUp)
                {
                    StackedPoint += value;
                }
                break;
            case Define.EMissionGoal.HeroLevelUp:
                if (eventType == Define.EBroadcastEventType.HeroLevelUp)
                {
                    StackedPoint += value;
                }
                break;
        }

        if(StackedPoint >= Managers.Data.MissionDataDic[TemplateId].MissionCount)
        {
            MissionState = Define.EMissionState.Rewardable;
        }
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

        if(currencyType == Define.ECurrencyType.Gold && value < 0)
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

    #region Hero
    private int _nowHero;
    public int NowHero
    {
        get { return _nowHero; }
        set
        {
            if (value == NowHero)
                return;

            // ���� ���� ���� ���
            if(_gameData.HeroSaves.ContainsKey(NowHero))
            {
                _gameData.HeroSaves[NowHero].isSelected = false;
            }
            
            // ���ο� ���� ��������
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
        // ���� ���õ� ���ΰ� ������ �������� üũ
        {
            // ���� ������ �־� ������ �������� Ȯ��
            if (heroData.NextLevelId == 0)
                return;

            // �ڿ� �������� üũ
            var currencies = heroData.LevelUpCurrencies;

            foreach (var currency in currencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                if (currency.count > GetCurrency(currency.currencyType))
                    return;
            }

            // �ڿ������ϸ� �ڿ� ���� ����
            foreach (var currency in currencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                AddCurrency(currency.currencyType, -currency.count);
            }
        }

        // ���õ� ������ ������
        {
            var heroSavedata = _gameData.HeroSaves[NowHero];
            // ���� ���� ������ ����
            int removeIndex = RemoveHeroSaveData(NowHero);

            // ���ο� ���� ������ �߰�
            {
                heroSavedata.TemplateId = heroData.NextLevelId;

                var nextHeroData = Managers.Data.HeroDataDic[heroSavedata.TemplateId];

                List<int> orgSkillId = new List<int>();

                foreach (int skillId in heroSavedata.SkillTemplateId)
                {
                    orgSkillId.Add(Managers.Data.HeroSkillDataDic[skillId].OriginalLevelId);
                }

                // ������ �߰� ��ų ������ �߰�
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

        // �������� ���� ���� ����
        NowHero = heroData.NextLevelId;

        // ���̺�
        SaveGame();

        Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.HeroLevelUp, 1);
    }

    public void HeroSkillUp(int skillTemplateId)
    {
        if (skillTemplateId == 0)
            return;

        // NowHero�� HeroSaveData�� ���� skill�� templateId�� ����

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

        // ���׷��̵� �������� üũ
        {
            // ���� ������ ���� �����Ѱ�
            if (skillData.NextLevelId == 0)
                return;

            // �ڿ��� ����Ѱ�
            var currencies = skillData.LevelUpCurrencies;

            foreach (var currency in currencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                if (currency.count > GetCurrency(currency.currencyType))
                    return;
            }

            // �ڿ������ϸ� �ڿ� ���� ����
            foreach (var currency in currencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                AddCurrency(currency.currencyType, -currency.count);
            }
        }

        // ���õ� ��ų ������
        {
            // ���ð� ����
            var nowSkillIndex = currentData.SkillTemplateId.IndexOf(skillTemplateId);
            currentData.SkillTemplateId[nowSkillIndex] = skillData.NextLevelId;

            // ���̺� �� �� ���� - ������ ��ũ�� �����Ǿ��� ������ gameData���� �ڵ� ������
            //var nowSKillIndexSave = _gameData.BuddySaves[NowBuddy].SkillTemplateId.IndexOf(skillTemplateId);
            //_gameData.BuddySaves[NowBuddy].SkillTemplateId[nowSKillIndexSave] = skillData.NextLevelId;

            SaveGame();
            OnNowHeroChanged?.Invoke();

            Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.HeroSkillUp, 1);
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

        // ���� ����Ʈ�� ����(�� ����)�� �ִٸ� �ֽ� ����� ���� ���ش�
        {
            var previewIndex = Managers.Data.BuddyDataDic[buddySaveData.TemplateId].PreviewLevelId;

            var selectedIndex = Array.IndexOf(_selectedBuddies, previewIndex);

            // ���� �ش��ϴ� ������ �ִٸ� �������ش�
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
                    // 0�� �ƴ� ��Ҹ� writeIndex ��ġ�� ����
                    if (_selectedBuddies[j] != 0)
                    {
                        _selectedBuddies[writeIndex] = _selectedBuddies[j];
                        writeIndex++;
                    }
                }
                // ���� ������ 0���� ä��
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
        // ���� ���õ� ���� �������� �������� üũ
        {
            // ���� ������ �־� ������ �������� Ȯ��
            if (buddyData.NextLevelId == 0)
                return;

            // �ڿ� �������� üũ
            var currencies = buddyData.LevelUpCurrencies;

            foreach (var currency in currencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                if (currency.count > GetCurrency(currency.currencyType))
                    return;
            }

            // �ڿ������ϸ� �ڿ� ���� ����
            foreach (var currency in currencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                AddCurrency(currency.currencyType, -currency.count);
            }
        }

        // ���õ� ���� ������
        {
            var buddySavedata = _gameData.BuddySaves[NowBuddy];
            // ���� ���� ������ ����
            int removeIndex = RemoveBuddySaveData(NowBuddy);

            // ���ο� ���� ������ �߰�
            {
                buddySavedata.TemplateId = buddyData.NextLevelId;

                var nextBuddyData = Managers.Data.BuddyDataDic[buddySavedata.TemplateId];

                List<int> orgSkillId = new List<int>();

                foreach(int skillId in buddySavedata.SkillTemplateId)
                {
                    orgSkillId.Add(Managers.Data.BuddySkillDataDic[skillId].OriginalLevelId);
                }

                // ������ �߰� ��ų ������ �߰�
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

        // �������� ���� ���� ����
        NowBuddy = buddyData.NextLevelId;

        // ���̺�
        SaveGame();

        Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.BuddyLevelUp, 1);
    }

    public void BuddySkillUp(int skillTemplateId)
    {
        if (skillTemplateId == 0)
            return;

        // NowBuddy�� BuddySaveData�� ���� skill�� templateId�� ����

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

        // ���׷��̵� �������� üũ
        {
            // ���� ������ ���� �����Ѱ�
            if (skillData.NextLevelId == 0)
                return;

            // �ڿ��� ����Ѱ�
            var currencies = skillData.LevelUpCurrencies;

            foreach (var currency in currencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                if (currency.count > GetCurrency(currency.currencyType))
                    return;
            }

            // �ڿ������ϸ� �ڿ� ���� ����
            foreach (var currency in currencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                AddCurrency(currency.currencyType, -currency.count);
            }
        }

        // ���õ� ��ų ������
        {
            // ���ð� ����
            var nowSkillIndex = currentData.SkillTemplateId.IndexOf(skillTemplateId);
            currentData.SkillTemplateId[nowSkillIndex] = skillData.NextLevelId;

            // ���̺� �� �� ���� - ������ ��ũ�� �����Ǿ��� ������ gameData���� �ڵ� ������
            //var nowSKillIndexSave = _gameData.BuddySaves[NowBuddy].SkillTemplateId.IndexOf(skillTemplateId);
            //_gameData.BuddySaves[NowBuddy].SkillTemplateId[nowSKillIndexSave] = skillData.NextLevelId;

            SaveGame();
            OnNowBuddyChanged?.Invoke();

            Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.BuddySkillUp, 1);
        }

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
        
        if(missionSavewData == null)
            return;

        if (missionSavewData.MissionState != Define.EMissionState.Rewardable)
            return;

        int point = Managers.Data.MissionDataDic[templateId].Point;
        
        int dayIndex = Managers.Data.MissionDataDic.Values.FirstOrDefault(m => m.MissionType == Define.EMissionType.Day).TemplateId;
        var dayMissionSaveData = GetMissionSaveData(dayIndex);
        dayMissionSaveData.StackedPoint += point;

        if(dayMissionSaveData.StackedPoint > Managers.Data.MissionDataDic[dayIndex].MaxPoint)
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

    public void GetDayMissionReward(int templateId)
    {
        var missionSavewData = GetMissionSaveData(templateId);
        var missionData = Managers.Data.MissionDataDic[templateId];

        if (missionSavewData == null)
            return;

        List<Reward> rewardList = new List<Reward>();
        for(int index = 0; index < missionSavewData.PointStepMissionState.Count; index++)
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
        Managers.Event.TriggerEvent(EEventType.OnMissionChanged);
    }

    public void GetWeekMissionReward()
    {

    }

    public void SaveMission(int templateId)
    {
        SaveGame();
    }

    //public void OnHandleMissionEvent(Define.EBroadcastEventType eventType, int value)
    //{
    //    foreach(MissionSaveData missionSaveData in MissionSaveDatas)
    //    {
    //        if(missionSaveData.MissionState == EMissionState.Progress)
    //        {
    //            missionSaveData.OnHandleBroadcastEvent(eventType, value);
    //        }
    //    }
    //}




    #endregion

    #region Action
    public event Action OnCurrenciesChagned;
    public event Action OnCurrentStageChanged;
    public event Action OnNowBuddyChanged;
    public event Action OnNowHeroChanged;
    public event Action OnSelectedBuddyChanged;
    #endregion

    #region Time
    private DateTime _missionTime;
    public DateTime MissionTime
    {
        get { return _missionTime; }
        set
        {
            _missionTime = value;
            _gameData.LastMissionTime = MissionTime;
            SaveGame();
        }
    }
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

        // ���̺� ������ ���� ��

        // Mission
        _gameData.MissionSaves.Clear();
        foreach (var mission in Managers.Data.MissionDataDic)
        {
            _gameData.MissionSaves.Add(mission.Value.TemplateId, new MissionSaveData(mission.Value.TemplateId));
        }

        MissionSaveDatas = _gameData.MissionSaves.Values.ToList();

        // �⺻ ���� 4�� �־�α�
        buddies = new List<BuddySaveData>();
        AddBuddySaveData(new BuddySaveData(100000100, Managers.Data.BuddyDataDic[100000100].SKillIds, true));
        AddBuddySaveData(new BuddySaveData(300000100, Managers.Data.BuddyDataDic[300000100].SKillIds, true));
        AddBuddySaveData(new BuddySaveData(100000300, Managers.Data.BuddyDataDic[100000300].SKillIds, true));
        AddBuddySaveData(new BuddySaveData(100000500, Managers.Data.BuddyDataDic[100000500].SKillIds, true));

        //_gameData.BuddySaves.Add(1, new BuddySaveData(1, Managers.Data.BuddyDataDic[1].SKillIds, true));
        //_gameData.BuddySaves.Add(3, new BuddySaveData(3, Managers.Data.BuddyDataDic[3].SKillIds, true));
        //_gameData.BuddySaves.Add(5, new BuddySaveData(5, Managers.Data.BuddyDataDic[5].SKillIds, true));
        //_gameData.BuddySaves.Add(7, new BuddySaveData(7, Managers.Data.BuddyDataDic[7].SKillIds, true));

        _gameData.HeroSaves.Add(100, new HeroSaveData(100, Managers.Data.HeroDataDic[100].SKillIds, true));
        _gameData.HeroSaves.Add(200, new HeroSaveData(200, Managers.Data.HeroDataDic[200].SKillIds, false));

        //buddies = _gameData.BuddySaves.Values.ToList();
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

        MissionTime = DateTime.Now;

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

        // �Է� ó��
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
            // ���⼭ �ϴ°� �³�?
            AddCurrency((Define.ECurrencyType)i, currencyCounts[i]);
        }

        if (_gameData.StageClears[stageTemplateId].isClear == false)
        {
            for(int i = 0; i < stageData.RewardFirstType.Count; i++)
            {
                rewards.Add(new Reward(stageData.RewardFirstType[i], stageData.RewardFirstCount[i], true));
                // ���⼭ �ϴ°� �³�?
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

        Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.StageClear, 1);

        SaveGame();
    }
    #endregion

    #region Gacha
    public void DoHeroGacha(int count)
    {
        // TODO ILHAK price data
        var needDia = 0;

        if(count == 1)
        {
            needDia = 110;
        }
        else if(count == 10)
        {
            needDia = 1000;
        }

        if (needDia == 0)
            return;

        List<Reward> rewards = new List<Reward>();
        System.Random random = new System.Random();

        for (int i = 0; i <  count; i++)
        {
            int randomNumber = random.Next(Managers.Data.HeroGachaDataDic.First().Value.Max);

            foreach (var heroGachaData in Managers.Data.HeroGachaDataDic.Values)
            {
                if (heroGachaData.Percent > randomNumber)
                {
                    Debug.Log($"{heroGachaData.CurrencyType} : {heroGachaData.CurrencyCount}");
                    rewards.Add(new Reward(heroGachaData.CurrencyType, heroGachaData.CurrencyCount));
                    AddCurrency(heroGachaData.CurrencyType, heroGachaData.CurrencyCount);
                    break;
                }
            }

            var clear = Managers.UI.ShowPopupUI<UI_RewardPopup>();

            clear.SetInfo(Define.ERewardType.HeroGacha, rewards);
        }
    }

    public void DoCurrencyGacha(int count)
    {
        // TODO ILHAK price data
        var needGold = 0;

        if (count == 1)
        {
            needGold = 100;
        }
        else if (count == 10)
        {
            needGold = 1000;
        }
        else if(count == 100)
        {
            needGold = 10000;
        }

        if (needGold == 0)
            return;

        List<Reward> rewards = new List<Reward>();
        System.Random random = new System.Random();

        for (int i = 0; i < count; i++)
        {
            int randomNumber = random.Next(Managers.Data.CurrencyGachaDataDic.First().Value.Max);

            foreach (var currencyGachaData in Managers.Data.CurrencyGachaDataDic.Values)
            {
                if (currencyGachaData.Percent > randomNumber)
                {
                    Debug.Log($"{currencyGachaData.CurrencyType} : {currencyGachaData.CurrencyCount}");
                    rewards.Add(new Reward(currencyGachaData.CurrencyType, currencyGachaData.CurrencyCount));
                    AddCurrency(currencyGachaData.CurrencyType, currencyGachaData.CurrencyCount);
                    break;
                }
            }

            var clear = Managers.UI.ShowPopupUI<UI_RewardPopup>();

            clear.SetInfo(Define.ERewardType.CurrencyGacha, rewards);

            Managers.Event.BroadcastMissionEvent(Define.EBroadcastEventType.DoCurrencyGacha, count);
        }
    }

    public void DoBuddyGacha(int count)
    {
        Debug.Log("Start Buddy Gacha");
        // TODO ILHAK price data
        var needDia = 0;

        if (count == 1)
        {
            needDia = 110;
        }
        else if (count == 10)
        {
            needDia = 1000;
        }

        if (needDia == 0)
            return;

        List<BuddyGacha> gachaResult = new List<BuddyGacha>();
        List<string> buddyNames = new List<string>();
        System.Random random = new System.Random();

        for (int i = 0; i < count; i++)
        {
            int randomNumber = random.Next(Managers.Data.BuddyGachaRarityDataDic.First().Value.Max);

            Define.ERarityType rarity = Define.ERarityType.None;

            foreach (var buddyRarity in Managers.Data.BuddyGachaRarityDataDic.Values)
            {
                if (buddyRarity.Percent > randomNumber)
                {
                    // ���Ƽ ������
                    Debug.Log($"{buddyRarity.RarityType} : {buddyRarity.Percent}");
                    rarity = buddyRarity.RarityType;
                    break;
                }
            }

            // ���� �̱�
            if(rarity == Define.ERarityType.Common)
            {
                int randomBuddyPercent = random.Next(Managers.Data.commonBuddies.Count);
                buddyNames.Add(Managers.Data.BuddyGachaDataDic[Managers.Data.commonBuddies[randomBuddyPercent]].GachaItem);
            }
            else if (rarity == Define.ERarityType.Rare)
            {
                int randomBuddyPercent = random.Next(Managers.Data.rareBuddies.Count);
                buddyNames.Add(Managers.Data.BuddyGachaDataDic[Managers.Data.rareBuddies[randomBuddyPercent]].GachaItem);
            }
            else if (rarity == Define.ERarityType.Epic)
            {
                int randomBuddyPercent = random.Next(Managers.Data.epicBuddies.Count);
                buddyNames.Add(Managers.Data.BuddyGachaDataDic[Managers.Data.epicBuddies[randomBuddyPercent]].GachaItem);
            }
            else if (rarity == Define.ERarityType.Unique)
            {
                int randomBuddyPercent = random.Next(Managers.Data.uniqueBuddies.Count);
                buddyNames.Add(Managers.Data.BuddyGachaDataDic[Managers.Data.uniqueBuddies[randomBuddyPercent]].GachaItem);
            }
            else if (rarity == Define.ERarityType.Legend)
            {
                int randomBuddyPercent = random.Next(Managers.Data.legendBuddies.Count);
                buddyNames.Add(Managers.Data.BuddyGachaDataDic[Managers.Data.legendBuddies[randomBuddyPercent]].GachaItem);
            }
        }

        // ���� �ߺ� üũ
        foreach (var buddyName in buddyNames)
        {
            var buddyData = Managers.Data.BuddyDataDic[Managers.Data.BuddyGachaDataDic[buddyName].BuddyTemplateId];
            if (GetBuddySaveData(buddyData.TemplateId) == null)
            {
                gachaResult.Add(new BuddyGacha(buddyName, false));
                AddBuddySaveData(new BuddySaveData(buddyData.TemplateId, null, false));
            }
            else
            {
                gachaResult.Add(new BuddyGacha(buddyName, true));
                AddCurrency(Managers.Data.BuddyGachaDataDic[buddyName].CurrencyType, Managers.Data.BuddyGachaDataDic[buddyName].CurrencyCount);
            }
        }

        var result = Managers.UI.ShowPopupUI<UI_BuddyGachaPopup>();
        result.SetInfo(gachaResult);
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

        // ����, ���� ���� ó��

        // ���� ���� ��������
        buddies = _gameData.BuddySaves.Values.ToList();
        int i = 0;
        foreach ( var buddy in buddies )
        {
            if( buddy.isSelected == true )
            {
                _selectedBuddies[i++] = buddy.TemplateId;
            }
        }

        // ���� ��������
        heroes = _gameData.HeroSaves.Values.ToList();
        foreach (var hero in heroes)
        {
            if (hero.isSelected == true)
            {
                NowHero = hero.TemplateId;
            }
        }

        OnSelectedBuddyChanged?.Invoke();

        // �̼� ��������
        MissionSaveDatas = _gameData.MissionSaves.Values.ToList();

        MissionTime = _gameData.LastMissionTime;

        Debug.Log("Loading Sucess");
        return true;
    }
    #endregion
}
