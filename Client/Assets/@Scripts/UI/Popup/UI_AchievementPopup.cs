using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;


public class UI_AchievementPopup : UI_Popup
{
    private enum GameObjects
    {
        CloseArea,

        NormalMissonListArea,
    }

    private List<UI_NormalArchievementSubItem> _normalArchievementSlotUIList = new List<UI_NormalArchievementSubItem>();

    protected override void Awake()
    {
        base.Awake();

        BindGameObjects(typeof(GameObjects));

        _normalArchievementSlotUIList.Clear();
        GetGameObject((int)GameObjects.NormalMissonListArea).transform.DestroyChildren();
        for (int index = 0; index < Managers.Game.AchievementSaveDatas.Count; index++)
        {
            UI_NormalArchievementSubItem slotUI = Managers.UI.MakeSubItem<UI_NormalArchievementSubItem>(GetGameObject((int)GameObjects.NormalMissonListArea).transform);
            _normalArchievementSlotUIList.Add(slotUI);
        }

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
        foreach (var normallMission in Managers.Game.AchievementSaveDatas)
        {
            _normalArchievementSlotUIList[index].SetInfo(normallMission.TemplateId);
            index++;
        }
    }


    private void OnCloseAreaClick(PointerEventData data)
    {
        ClosePopupUI();
    }
}
