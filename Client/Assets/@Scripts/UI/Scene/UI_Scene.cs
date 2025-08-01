

using UnityEngine;

public class UI_Scene : UI_Base
{
    public Canvas UICanvas;
    protected override void Awake()
    {
        base.Awake();

        UICanvas = Managers.UI.SetCanvas(gameObject, false);
    }
}
