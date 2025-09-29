using System.Collections.Generic;
using System;
using UnityEngine;
using Data;

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
    public List<int> EventValues = new List<int>();

    public List<AchievementSaveData> AchievementSaveDatas;
    public HashSet<int> AchievementClearList;
    
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
public class AchievementSaveData
{
    public int TemplateId;
    public Define.EMissionState MissionState;
    public int OriginalTemplateId;

    public AchievementSaveData(int templateId)
    {
        TemplateId = templateId;
        MissionState = Define.EMissionState.Progress;
        OriginalTemplateId = Managers.Data.AchievementDataDic[templateId].OriginalAchievementId;
    }

    public void SetNextAchievment()
    {
        var nextTempalteId = Managers.Data.AchievementDataDic[TemplateId].NextAchievementId;
        MissionState = Define.EMissionState.Finish;

        if (nextTempalteId != 0)
        {
            TemplateId = nextTempalteId;
            MissionState = Define.EMissionState.Progress;
        }
    }

    public bool CheckRewardAble()
    {
        if (MissionState == Define.EMissionState.Finish)
            return false;

        int stackPoint = Managers.Game.GetAcievemntValue(TemplateId);

        var achievementData = Managers.Data.AchievementDataDic[TemplateId];

        if (stackPoint >= achievementData.MissionCount)
        {
            MissionState = Define.EMissionState.Rewardable;
        }

        if (MissionState == Define.EMissionState.Rewardable)
            return true;

        return false;
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

        switch (Managers.Data.MissionDataDic[TemplateId].MissionGoal)
        {
            case Define.EMissionGoal.MonsterKill:
                if (eventType == Define.EBroadcastEventType.KillMonster)
                {
                    StackedPoint += value;
                    //Managers.Game.SaveMission(TemplateId);
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

        if (StackedPoint >= Managers.Data.MissionDataDic[TemplateId].MissionCount)
        {
            MissionState = Define.EMissionState.Rewardable;
        }
    }
}
