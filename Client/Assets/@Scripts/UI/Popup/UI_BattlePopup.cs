using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_BattlePopup : UI_Popup
{
    enum Buttons
    {
        GameStartButton,
        Button_Next,
        Button_Preview,
        Button_StageHard,
    }

    enum Texts
    {
        Text_Stage,
        Text_GameStartButton
    }

    protected override void Awake()
    {
        base.Awake();

        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.GameStartButton).gameObject.BindEvent(OnClickGameStartButton);
        GetButton((int)Buttons.GameStartButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.Button_Next).gameObject.BindEvent(OnClickNextButton);
        GetButton((int)Buttons.Button_Next).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.Button_Preview).gameObject.BindEvent(OnClickPreviewButton);
        GetButton((int)Buttons.Button_Preview).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.Button_StageHard).gameObject.BindEvent(OnClickStageHardButton);
        GetButton((int)Buttons.Button_StageHard).GetOrAddComponent<UI_ButtonAnimation>();

        RefreshUI();
    }

    private void OnEnable()
    {
        Managers.Game.OnCurrentStageChanged -= RefreshUI;
        Managers.Game.OnCurrentStageChanged += RefreshUI;
    }

    private void OnDisable()
    {
        Managers.Game.OnCurrentStageChanged -= RefreshUI;
    }

    private void OnClickGameStartButton(PointerEventData evt)
    {
        Managers.Sound.PlayButtonClick();

        Managers.Scene.LoadScene(Define.EScene.GameScene);
    }

    private void OnClickNextButton(PointerEventData evt)
    {
        Managers.Sound.PlayButtonClick();

        var templateId = Managers.Game.stageTemplateId;

        var stageData = Managers.Data.StageDataDic[templateId];

        Debug.Log(stageData.NextaStageId);

        Managers.Game.stageTemplateId = stageData.NextaStageId;
    }
    private void OnClickPreviewButton(PointerEventData evt)
    {
        Managers.Sound.PlayButtonClick();

        var templateId = Managers.Game.stageTemplateId;

        var stageData = Managers.Data.StageDataDic[templateId];

        Managers.Game.stageTemplateId = stageData.PreviewStageId;
    }

    private void OnClickStageHardButton(PointerEventData evt)
    {
        Managers.Sound.PlayButtonClick();

        var templateId = Managers.Game.stageTemplateId;

        var stageData = Managers.Data.StageDataDic[templateId];

        Managers.Game.stageTemplateId = stageData.OtherStageId;
    }

    private void RefreshUI()
    {
        var templateId = Managers.Game.stageTemplateId;

        var stageData = Managers.Data.StageDataDic[templateId];

        GetText((int)Texts.Text_Stage).text = stageData.WorldNumber + " - " + stageData.StageNumber;

        if(stageData.DifficultyLevel == Define.EDifficultyLevel.Hard)
        {
            GetText((int)Texts.Text_Stage).text += "\nHard";
            GetText((int)Texts.Text_GameStartButton).text = "Normal";
            GetButton((int)Buttons.Button_StageHard).GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(Define.GREENBUTTON);
        }
        else if (stageData.DifficultyLevel == Define.EDifficultyLevel.Normal)
        {
            GetText((int)Texts.Text_GameStartButton).text = "Hard";
            GetButton((int)Buttons.Button_StageHard).GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(Define.REDBUTTON);
        }
    }
}
