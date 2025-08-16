using UnityEngine;

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

    protected override void Awake()
    {
        base.Awake();
    }
}
