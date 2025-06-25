using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_BattlePopup : UI_Popup
{
    enum Buttons
    {
        GameStartButton,
    }

    protected override void Awake()
    {
        base.Awake();

        BindButtons(typeof(Buttons));

        GetButton((int)Buttons.GameStartButton).gameObject.BindEvent(OnClickGameStartButton);
        GetButton((int)Buttons.GameStartButton).GetOrAddComponent<UI_ButtonAnimation>();
    }

    private void OnClickGameStartButton(PointerEventData evt)
    {
        Managers.Sound.PlayButtonClick();

        Managers.Scene.LoadScene(Define.EScene.GameScene);
    }
}
