using System.Collections.Generic;
using UnityEngine;

public class UI_MissionPopup : UI_Popup
{
    private enum GameObjects
    {
        CloseArea,

        NormalMissonListArea,
        UI_WeekMissionSlot,
        UI_DayMissionSlot,
    }

    private List<UI_NormalMissionSubItem> _normalMissionSlotUIList = new List<UI_NormalMissionSubItem>();
    private UI_DayMissionSubItem _dayMissionSlotUI;
    private UI_WeekMissionSubItem _weekMissionSlotUI;

    protected override void Awake()
    {
        base.Awake();
    }
}
