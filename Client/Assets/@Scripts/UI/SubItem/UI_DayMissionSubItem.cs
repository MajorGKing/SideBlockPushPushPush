using Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_DayMissionSubItem : UI_SubItem
{
    private enum GameObjects
    {
        DayMissionTakeArea,
    }

    private enum Texts
    {
        Text_DayMissionCount1,
        Text_DayMissionCount2,
        Text_DayMissionCount3,
        Text_DayMissionCount4,
        Text_DayMissionCount5,
    }

    private enum Buttons
    {
        Button_AllTake,
    }

    private enum Sliders
    {
        Slider_DayMissionExp,
    }

    private List<TMP_Text> _dayMissionCountTextList = new List<TMP_Text>();
    private List<UI_RewardsSubItem> _missionRewardSubItemList = new List<UI_RewardsSubItem>();

    private MissionSaveData missionSaveData;
    private MissionData missionData;

    protected override void Awake()
    {
        base.Awake();

        // Bind
        BindGameObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));
        BindSliders(typeof(Sliders));

        // Init
        _dayMissionCountTextList.Clear();
        _dayMissionCountTextList.Add(GetText((int)Texts.Text_DayMissionCount1));
        _dayMissionCountTextList.Add(GetText((int)Texts.Text_DayMissionCount2));
        _dayMissionCountTextList.Add(GetText((int)Texts.Text_DayMissionCount3));
        _dayMissionCountTextList.Add(GetText((int)Texts.Text_DayMissionCount4));
        _dayMissionCountTextList.Add(GetText((int)Texts.Text_DayMissionCount5));

        _missionRewardSubItemList.Clear();
        foreach (Transform child in GetGameObject((int)GameObjects.DayMissionTakeArea).transform)
        {
            UI_RewardsSubItem slotUI = child.GetComponent<UI_RewardsSubItem>();
            _missionRewardSubItemList.Add(slotUI);
        }

        // Bind Event
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
            _dayMissionCountTextList[index].text = $"{missionData.PointStep[index]:N0}";
        }

        int rewardCount = 0;
        for (int index = 0; index < _missionRewardSubItemList.Count; index++)
        {
            _missionRewardSubItemList[index].SetInfo(missionData.RewardCurrencies[index].currencyType, missionData.RewardCurrencies[index].count, false);

            bool isActive = true;
            if (missionSaveData.StackedPoint < missionData.PointStep[index])
            {
                isActive = false;
            }

            if (missionSaveData.PointStepMissionState[index] == Define.EMissionState.Finish)
            {
                isActive = false;
                _missionRewardSubItemList[index].SetInfo(missionData.RewardCurrencies[index].currencyType, missionData.RewardCurrencies[index].count, false, false);
            }

            if (isActive)
            {
                rewardCount++;
            }
        }

        GetButton((int)Buttons.Button_AllTake).interactable = missionSaveData.MissionState == Define.EMissionState.Rewardable && rewardCount > 0;

        GetSlider((int)Sliders.Slider_DayMissionExp).value = (float)missionSaveData.StackedPoint / missionData.MaxPoint;
    }

    private void OnAllTakeButtonClick(PointerEventData data)
    {
        if (GetButton((int)Buttons.Button_AllTake).interactable)
        {
            Managers.Game.GetMissionReward(missionData.TemplateId);
        }
    }
}
