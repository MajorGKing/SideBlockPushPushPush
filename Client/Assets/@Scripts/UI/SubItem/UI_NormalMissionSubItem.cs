using Data;
using UnityEngine;
using UnityEngine.EventSystems;

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

    private MissionSaveData missionSaveData;
    private MissionData missionData;

    protected override void Awake()
    {
        base.Awake();

        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));
        BindSliders(typeof(Sliders));

        GetButton((int)Buttons.Button_Take).gameObject.BindEvent(OnTakeButtonClick);

        RefreshUI();
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
        if (missionData == null)
            return;

        if (missionSaveData == null)
            return;

        GetText((int)Texts.Text_NormalMissionListIcon).text = $"+{missionData.Point:N0}";
        GetText((int)Texts.Text_MissonTitle).text = $"{Managers.GetText(missionData.NameTextId)}";
        GetText((int)Texts.Text_NormalMissionCount).text = $"{missionSaveData.StackedPoint:N0}/{missionData.MissionCount:N0}";

        GetButton((int)Buttons.Button_Take).interactable = missionSaveData.MissionState == Define.EMissionState.Rewardable && missionSaveData.StackedPoint >= missionData.MissionCount;

        GetSlider((int)Sliders.Slider_NormalMission).value = (float)missionSaveData.StackedPoint / missionData.MissionCount;
    }

    private void OnTakeButtonClick(PointerEventData data)
    {
        if (GetButton((int)Buttons.Button_Take).interactable)
        {
            Managers.Game.GetMissionSubItemReward(missionData.TemplateId);
        }
    }
}
