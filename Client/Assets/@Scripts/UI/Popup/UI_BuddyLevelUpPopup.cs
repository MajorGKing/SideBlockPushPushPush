using UnityEngine;
using UnityEngine.EventSystems;

public class UI_BuddyLevelUpPopup : UI_Popup
{
    enum Texts
    {
        Text_WeaponStoneCount,
        Text_GlovesStoneCount,
        Text_RingStoneCount,
        Text_ArmorStoneCount,
        Text_BeltStoneCount,
        Text_BootsStoneCount,
        Text_WeaponScrollCount,
        Text_GlovesScrollCount,
        Text_RingScrollCount,
        Text_ArmorScrollCount,
        Text_BeltScrollCount,
        Text_BootsScrollCount,
    }

    enum Images
    {
        Image_SelectedBuddy1,
        Image_SelectedBuddy2,
        Image_SelectedBuddy3,
        Image_SelectedBuddy4,
    }

    protected override void Awake()
    {
        base.Awake();

        BindTexts(typeof(Texts));
        BindImages(typeof(Images));



        RefreshUI();
    }

    public void SetInfo()
    {
        RefreshUI();
    }

    private void OnEnable()
    {
        Managers.Game.OnCurrenciesChagned -= SetInfo;
        Managers.Game.OnCurrenciesChagned += SetInfo;

        Managers.Game.OnNowBuddyChanged -= SetInfo;
        Managers.Game.OnNowBuddyChanged += SetInfo;
    }

    private void OnDisable()
    {
        Managers.Game.OnCurrenciesChagned -= SetInfo;
        Managers.Game.OnNowBuddyChanged -= SetInfo;
    }

    private void RefreshUI()
    {
        // stone
        GetText((int)Texts.Text_WeaponStoneCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.StoneWeapon).ToString();
        GetText((int)Texts.Text_GlovesStoneCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.StoneGloves).ToString();
        GetText((int)Texts.Text_RingStoneCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.StoneRing).ToString();
        GetText((int)Texts.Text_ArmorStoneCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.StoneArmor).ToString();
        GetText((int)Texts.Text_BeltStoneCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.StoneBelt).ToString();
        GetText((int)Texts.Text_BootsStoneCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.StoneBoots).ToString();

        // scroll
        GetText((int)Texts.Text_WeaponScrollCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.ScrollWeapon).ToString();
        GetText((int)Texts.Text_GlovesScrollCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.ScrollGloves).ToString();
        GetText((int)Texts.Text_RingScrollCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.ScrollRing).ToString();
        GetText((int)Texts.Text_ArmorScrollCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.ScrollArmor).ToString();
        GetText((int)Texts.Text_BeltScrollCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.ScrollBelt).ToString();
        GetText((int)Texts.Text_BootsScrollCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.ScrollBoots).ToString();

        // Selected Buddy
        GetImage((int)Images.Image_SelectedBuddy1).gameObject.BindEvent(OnClickedSelectedBuddy1);
        GetImage((int)Images.Image_SelectedBuddy2).gameObject.BindEvent(OnClickedSelectedBuddy2);
        GetImage((int)Images.Image_SelectedBuddy3).gameObject.BindEvent(OnClickedSelectedBuddy3);
        GetImage((int)Images.Image_SelectedBuddy4).gameObject.BindEvent(OnClickedSelectedBuddy4);
    }

    private void OnClickedSelectedBuddy1(PointerEventData eventData)
    {
        Debug.Log("Touch One");
    }

    private void OnClickedSelectedBuddy2(PointerEventData eventData)
    {
        Debug.Log("Touch 2");
    }

    private void OnClickedSelectedBuddy3(PointerEventData eventData)
    {
        Debug.Log("Touch 3");
    }

    private void OnClickedSelectedBuddy4(PointerEventData eventData)
    {
        Debug.Log("Touch 4");
    }
}
