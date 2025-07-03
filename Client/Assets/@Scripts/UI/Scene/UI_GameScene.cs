using System.Linq;
using Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_GameScene : UI_Scene
{
    #region Enum
    enum GameObjects
    {

    }

    enum Images
    {

    }

    enum Buttons
    {
        Button_Exit,
        Button_Replay,
        Button_Auto,
    }

    enum Texts
    {
        Text_Exit, 
        Text_Replay, 
        Text_Auto
    }

    enum Sliders
    {

    }

    #endregion

    private bool _isAuto;
    private GameScene _scene;

    protected override void Awake()
    {
        base.Awake();

        //BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        //BindImages(typeof(Images));
        //BindSliders(typeof(Sliders));

        GetButton((int)Buttons.Button_Exit).gameObject.BindEvent(OnClickExitButton);
        GetButton((int)Buttons.Button_Auto).gameObject.BindEvent(OnClickAutoButton);
    }

    //private float elapsedTime;
    //private float updateInterval = 0.3f;

    private void Update()
    {
        //elapsedTime += Time.deltaTime;

        //if (elapsedTime >= updateInterval)
        //{
        //    float fps = 1.0f / Time.deltaTime;
        //    float ms = Time.deltaTime * 1000.0f;
        //    string text = string.Format("{0:N1} FPS ({1:N1}ms)", fps, ms);
        //    // GetText((int)Texts.FpsText).text = text;

        //    elapsedTime = 0;
        //}
    }

    public void SetInfo(bool doAuto, GameScene scene)
    {
        _isAuto = doAuto;

        _scene = scene;

        RefreshUI();
    }

    public void RefreshUI()
    {
        if(_isAuto == true)
        {
            var pause = Managers.Resource.Load<Sprite>("Pause_Icon");
            GetButton((int)Buttons.Button_Auto).transform.GetComponent<Image>().sprite = pause;
        }
        if (_isAuto == false)
        {
            var pause = Managers.Resource.Load<Sprite>("Next_Icon");
            GetButton((int)Buttons.Button_Auto).transform.GetComponent<Image>().sprite = pause;
        }

        //LayoutRebuilder.ForceRebuildLayoutImmediate(GetButton((int)Buttons.Button_Auto).GetComponent<RectTransform>());
    }

    private void OnClickExitButton(PointerEventData data)
    {
        Managers.Scene.LoadScene(Define.EScene.LobbyScene);
    }

    private void OnClickAutoButton(PointerEventData data)
    {
        _scene.SetAuto();
    }

    public void SetAutoUI(bool auto)
    {
        _isAuto = auto;
        RefreshUI();
    }
}