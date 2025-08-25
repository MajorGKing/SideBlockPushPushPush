using Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_WeekMissionSubItem : UI_SubItem
{
    private enum GameObjects
    {
        WeekMissionTakeArea,
    }

    private enum Texts
    {
        Text_WeekMissionCount1,
        Text_WeekMissionCount2,
        Text_WeekMissionCount3,
        Text_WeekMissionCount4,
        Text_WeekMissionCount5,
    }

    private enum Buttons
    {
        Button_AllTake,
    }

    private enum Sliders
    {
        Slider_WeekMissionExp,
    }

    private List<TMP_Text> _weekMissionCountTextList = new List<TMP_Text>();
    private List<UI_RewardsSubItem> _missionRewardSubItemList = new List<UI_RewardsSubItem>();

    private MissionSaveData missionSaveData;
    private MissionData missionData;

    protected override void Awake()
    {
        base.Awake();

        BindGameObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));
        BindSliders(typeof(Sliders));

        _weekMissionCountTextList.Clear();
        _weekMissionCountTextList.Add(GetText((int)Texts.Text_WeekMissionCount1));
        _weekMissionCountTextList.Add(GetText((int)Texts.Text_WeekMissionCount2));
        _weekMissionCountTextList.Add(GetText((int)Texts.Text_WeekMissionCount3));
        _weekMissionCountTextList.Add(GetText((int)Texts.Text_WeekMissionCount4));
        _weekMissionCountTextList.Add(GetText((int)Texts.Text_WeekMissionCount5));

        _missionRewardSubItemList.Clear();
        foreach (Transform child in GetGameObject((int)GameObjects.WeekMissionTakeArea).transform)
        {
            UI_RewardsSubItem subItemUI = child.GetComponent<UI_RewardsSubItem>();
            _missionRewardSubItemList.Add(subItemUI);
        }

        GetButton((int)Buttons.Button_AllTake).gameObject.BindEvent(OnAllTakeButtonClick);
    }

    public void SetInfo(int templateId)
    {
        // 미션 데이터를 받는다
        missionData = Managers.Data.MissionDataDic[templateId];

        // 미션 진행 데이터를 받는다
        missionSaveData = Managers.Game.GetMissionSaveData(templateId);

        RefreshUI();
    }
    private void RefreshUI()
    {
        for (int index = 0; index < missionData.PointStep.Count; index++)
        {
            _weekMissionCountTextList[index].text = missionData.PointStep[index].ToString();
        }

        int rewardCount = 0;
        for (int index = 0; index < _missionRewardSubItemList.Count; index++)
        {
            bool isActive = true;
            if (missionSaveData.StackedPoint < missionData.PointStep[index])
            {
                isActive = false;
            }

            if (missionSaveData.PointStepMissionState[index] == Define.EMissionState.Finish)
            {
                isActive = false;
            }

            if (isActive)
            {
                rewardCount++;
            }

            _missionRewardSubItemList[index].SetInfo(missionData.RewardCurrencies[index].currencyType, missionData.RewardCurrencies[index].count, false);
        }

        GetButton((int)Buttons.Button_AllTake).interactable = missionSaveData.MissionState == Define.EMissionState.Rewardable && rewardCount > 0;

        GetSlider((int)Sliders.Slider_WeekMissionExp).value = (float)missionSaveData.StackedPoint / missionData.MaxPoint;
    }

    private void OnAllTakeButtonClick(PointerEventData data)
    {
        if (GetButton((int)Buttons.Button_AllTake).interactable)
        {
            Managers.Game.GetMissionReward(missionData.TemplateId);
        }
    }

}
