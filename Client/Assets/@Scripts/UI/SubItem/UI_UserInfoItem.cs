using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_UserInfoItem : UI_SubItem
{
    enum GameObjects
    {

    }

    enum Buttons
    {
        StaminaButton,
        DiaButton,
        //GoldButton,
    }

    enum Texts
    {
        //UserLevelText, // 유저 계정 레벨
        StaminaValueText,
        DiaValueText,
        GoldValueText,
    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.StaminaButton).gameObject.BindEvent(OnClickStaminaButton);
        GetButton((int)Buttons.StaminaButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.DiaButton).gameObject.BindEvent(OnClickDiaButton);
        GetButton((int)Buttons.DiaButton).GetOrAddComponent<UI_ButtonAnimation>();

        RefreshUI();
    }

    private void OnEnable()
    {
        Managers.Game.OnCurrenciesChagned -= SetInfo;
        Managers.Game.OnCurrenciesChagned += SetInfo;

        Managers.Game.OnCurrentStageChanged -= SetInfo;
        Managers.Game.OnCurrentStageChanged += SetInfo;
    }

    private void OnDisable()
    {
        Managers.Game.OnCurrenciesChagned -= SetInfo;
        Managers.Game.OnCurrentStageChanged -= SetInfo;
    }

    //public void SetInfo(Define.EUserInfoItem type, int value)
    //{

    //    if(type == Define.EUserInfoItem.Stamina)
    //    {
    //        GetText((int)Texts.StaminaValueText).text = value.ToString();
    //    }
    //    else if (type == Define.EUserInfoItem.Dia)
    //    {
    //        GetText((int)Texts.DiaValueText).text = value.ToString();
    //    }
    //    else if (type == Define.EUserInfoItem.Gold)
    //    {
    //        GetText((int)Texts.GoldValueText).text = value.ToString();
    //    }

    //    Refresh();
    //}

    public void SetInfo()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
        GetText((int)Texts.StaminaValueText).text = Managers.Game.Stamina.ToString();
        GetText((int)Texts.DiaValueText).text = Managers.Game.GetCurrency(Define.ECurrencyType.Dia).ToString();
        GetText((int)Texts.GoldValueText).text = Managers.Game.GetCurrency(Define.ECurrencyType.Gold).ToString();
    }

    void OnClickStaminaButton(PointerEventData evt)
    {

    }

    void OnClickDiaButton(PointerEventData evt)
    {

    }
}
