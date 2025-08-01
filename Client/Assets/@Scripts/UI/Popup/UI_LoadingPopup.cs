using System;
using UnityEngine;

public class UI_LoadingPopup : UI_Popup
{
    enum GameObjects
    {
    }

    public Action OnClosed;

    protected override void Awake()
    {
        base.Awake();

        UICanvas.renderMode = RenderMode.ScreenSpaceCamera;
        UICanvas.worldCamera = Camera.main;
    }
}