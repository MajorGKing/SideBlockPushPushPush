using Data;
using UnityEngine;
using UnityEngine.EventSystems;
using WebPacket;
using static GameManager;

public class UI_NormalMissionSubItem : UI_SubItem
{
    private enum Texts
    {
        Text_NormalMissionListIcon,
        Text_MissonTitle,
        Text_NormalMissionCount,
    }

    private enum Buttons
    {
        Button_Take,
    }

    private enum Sliders
    {
        Slider_NormalMission,
    }

    private MissionSavedData _missionSaveData;
    private MissionData _missionData;

    protected override void Awake()
    {
        base.Awake();

        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));
        BindSliders(typeof(Sliders));

        GetButton((int)Buttons.Button_Take).gameObject.BindEvent(OnTakeButtonClick);

        RefreshUI();
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
        if (_missionData == null)
            return;

        if (_missionSaveData == null)
            return;

        GetText((int)Texts.Text_NormalMissionListIcon).text = $"+{_missionData.Point:N0}";
        GetText((int)Texts.Text_MissonTitle).text = $"{Managers.GetText(_missionData.NameTextId)}";
        GetText((int)Texts.Text_NormalMissionCount).text = $"{_missionSaveData.StackedPoint:N0}/{_missionData.MissionCount:N0}";

        GetButton((int)Buttons.Button_Take).interactable = _missionSaveData.MissionState == Define.EMissionState.Rewardable && _missionSaveData.StackedPoint >= _missionData.MissionCount;

        GetSlider((int)Sliders.Slider_NormalMission).value = (float)_missionSaveData.StackedPoint / _missionData.MissionCount;
    }

    private void OnTakeButtonClick(PointerEventData data)
    {
        if (GetButton((int)Buttons.Button_Take).interactable)
        {
            Managers.Game.GetMissionSubItemReward(_missionData.TemplateId);
        }
    }
}
