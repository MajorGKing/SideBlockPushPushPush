using Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static GameManager;

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

    private MissionSavedData _missionSaveData;
    private MissionData _missionData;

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

    public void SetInfo(MissionSavedData saveData)
    {
        // 미션 데이터를 받는다
        _missionData = Managers.Data.MissionDataDic[saveData.TemplateId];

        // 미션 진행 데이터를 받는다
        _missionSaveData = saveData;

        RefreshUI();
    }

    private void RefreshUI()
    {
        for (int index = 0; index < _missionData.PointStep.Count; index++)
        {
            _dayMissionCountTextList[index].text = $"{_missionData.PointStep[index]:N0}";
        }

        int rewardCount = 0;
        for (int index = 0; index < _missionRewardSubItemList.Count; index++)
        {
            _missionRewardSubItemList[index].SetInfo(_missionData.RewardCurrencies[index].currencyType, _missionData.RewardCurrencies[index].count, false);

            bool isActive = true;
            if (_missionSaveData.StackedPoint < _missionData.PointStep[index])
            {
                isActive = false;
            }

            if (_missionSaveData.PointStepMissionState[index] == Define.EMissionState.Finish)
            {
                isActive = false;
                _missionRewardSubItemList[index].SetInfo(_missionData.RewardCurrencies[index].currencyType, _missionData.RewardCurrencies[index].count, false, false);
            }

            if (isActive)
            {
                rewardCount++;
            }
        }

        GetButton((int)Buttons.Button_AllTake).interactable = _missionSaveData.MissionState == Define.EMissionState.Rewardable && rewardCount > 0;

        GetSlider((int)Sliders.Slider_DayMissionExp).value = (float)_missionSaveData.StackedPoint / _missionData.MaxPoint;
    }

    private void OnAllTakeButtonClick(PointerEventData data)
    {
        if (GetButton((int)Buttons.Button_AllTake).interactable)
        {
            Managers.Game.GetMissionReward(_missionData.TemplateId);
        }
    }
}
