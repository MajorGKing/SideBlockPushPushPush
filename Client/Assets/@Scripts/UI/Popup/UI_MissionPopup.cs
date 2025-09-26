using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.EventSystems;


public class UI_MissionPopup : UI_Popup
{
    private enum GameObjects
    {
        CloseArea,

        NormalMissonListArea,
        UI_WeekMissionSubItem,
        UI_DayMissionSubItem,
    }

    private List<UI_NormalMissionSubItem> _normalMissionSlotUIList = new List<UI_NormalMissionSubItem>();
    private UI_DayMissionSubItem _dayMissionSlotUI;
    private UI_WeekMissionSubItem _weekMissionSlotUI;

    protected override void Awake()
    {
        base.Awake();

        BindGameObjects(typeof(GameObjects));

        _normalMissionSlotUIList.Clear();
        GetGameObject((int)GameObjects.NormalMissonListArea).transform.DestroyChildren();
        for (int index = 0; index < Managers.Game.NormalMissions.Count; index++)
        {
            UI_NormalMissionSubItem slotUI = Managers.UI.MakeSubItem<UI_NormalMissionSubItem>(GetGameObject((int)GameObjects.NormalMissonListArea).transform);
            _normalMissionSlotUIList.Add(slotUI);
        }

        _dayMissionSlotUI = GetGameObject((int)GameObjects.UI_DayMissionSubItem).GetComponent<UI_DayMissionSubItem>();
        _weekMissionSlotUI = GetGameObject((int)GameObjects.UI_WeekMissionSubItem).GetComponent<UI_WeekMissionSubItem>();

        GetGameObject((int)GameObjects.CloseArea).BindEvent(OnCloseAreaClick);
    }

    private void OnEnable()
    {
        Managers.Event.AddEvent(Define.EEventType.OnMissionChanged, RefreshUI);
    }

    private void OnDisable()
    {
        Managers.Event.RemoveEvent(Define.EEventType.OnMissionChanged, RefreshUI);
    }

    public void SetInfo()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        int index = 0;
        foreach (var normallMission in Managers.Game.NormalMissions)
        {
            _normalMissionSlotUIList[index].SetInfo(normallMission);
            index++;
        }

        _dayMissionSlotUI.SetInfo(Managers.Game.DayMissions[0]);
        _weekMissionSlotUI.SetInfo(Managers.Game.WeekMissions[0]);
    }


    private void OnCloseAreaClick(PointerEventData data)
    {
        ClosePopupUI();
    }
}
