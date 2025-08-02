using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ShopPopup : UI_Popup
{
    private enum Objects
    {
        HeroArea,
        BuddyArea,
        CurrenciesArea,
        GoodsArea,
    }

    private enum Buttons
    {
        Button_HeroGachaCount1,
        Button_HeroGachaCount10,
        Button_BuddyGachaCount1,
        Button_BuddyGachaCount10,
        Button_CurrenciesGachaCount1,
        Button_CurrenciesGachaCount10,
        Button_CurrenciesGachaCount100,
        Button_Dia1000,
        Button_Dia100,
        Button_Gold10000,
        Button_Gold1000,
    }

    private enum Toggles
    {
        Toggle_HeroGacha,
        Toggle_BuddyGacha,
        Toggle_Currencies,
        Toggle_Goods,
    }

    private enum ShopPopupState
    {
        None,
        HeroGacha,
        BuddyGacha,
        Currencies,
        Goods,
    }

    private ShopPopupState _shopPopupState = ShopPopupState.None;

    private Toggle _heroToggle;
    private Toggle _buddyToggle;
    private Toggle _currenciesToggle;
    private Toggle _goodsToggle;

    protected override void Awake()
    {
        base.Awake();

        // Bind
        BindObjects(typeof(Objects));
        BindButtons(typeof(Buttons));
        BindToggles(typeof(Toggles));

        // Init
        _heroToggle = GetToggle((int)Toggles.Toggle_HeroGacha);
        _buddyToggle = GetToggle((int)Toggles.Toggle_BuddyGacha);
        _currenciesToggle = GetToggle((int)Toggles.Toggle_Currencies);
        _goodsToggle = GetToggle((int)Toggles.Toggle_Goods);

        // Bind Event
        _heroToggle.gameObject.BindEvent(OnClickHeroToggle);
        _buddyToggle.gameObject.BindEvent(OnClickBuddyToggle);
        _currenciesToggle.gameObject.BindEvent(OnClickCurrenciesToggle);
        _goodsToggle.gameObject.BindEvent(OnClickGoodsToggle);

        //Button_HeroGachaCount1,
        //Button_HeroGachaCount10,
        //Button_BuddyGachaCount1,
        //Button_BuddyGachaCount10,
        //Button_CurrenciesGachaCount1,
        //Button_CurrenciesGachaCount10,
        //Button_CurrenciesGachaCount100,
        //Button_Dia1000,
        //Button_Dia100,
        //Button_Gold10000,
        //Button_Gold1000,
        //GetButton((int)Buttons.Button_HeroGachaCount10).gameObject.BindEvent(OnClickSkillGachaCount10);
        //GetButton((int)Buttons.Button_BuddyGachaCount10).gameObject.BindEvent(OnClickBuddyGachaCount10);
    }

    public void SetInfo()
    {
        _shopPopupState = ShopPopupState.HeroGacha;

        RefreshUI();
    }

    private void RefreshUI()
    {
        switch (_shopPopupState)
        {
            case ShopPopupState.HeroGacha:
                GetObject((int)Objects.HeroArea).SetActive(true);
                GetObject((int)Objects.BuddyArea).SetActive(false);
                GetObject((int)Objects.CurrenciesArea).SetActive(false);
                GetObject((int)Objects.GoodsArea).SetActive(false);
                break;
            case ShopPopupState.BuddyGacha:
                GetObject((int)Objects.HeroArea).SetActive(false);
                GetObject((int)Objects.BuddyArea).SetActive(true);
                GetObject((int)Objects.CurrenciesArea).SetActive(false);
                GetObject((int)Objects.GoodsArea).SetActive(false);
                break;
            case ShopPopupState.Currencies:
                GetObject((int)Objects.HeroArea).SetActive(false);
                GetObject((int)Objects.BuddyArea).SetActive(false);
                GetObject((int)Objects.CurrenciesArea).SetActive(true);
                GetObject((int)Objects.GoodsArea).SetActive(false);
                break;
            case ShopPopupState.Goods:
                GetObject((int)Objects.HeroArea).SetActive(false);
                GetObject((int)Objects.BuddyArea).SetActive(false);
                GetObject((int)Objects.CurrenciesArea).SetActive(false);
                GetObject((int)Objects.GoodsArea).SetActive(true);
                break;

            default:
                break;
        }
    }

    private void OnClickHeroToggle(PointerEventData data)
    {
        _shopPopupState = ShopPopupState.HeroGacha;
        RefreshUI();
    }

    private void OnClickBuddyToggle(PointerEventData data)
    {
        _shopPopupState = ShopPopupState.BuddyGacha;
        RefreshUI();
    }

    private void OnClickCurrenciesToggle(PointerEventData data)
    {
        _shopPopupState = ShopPopupState.Currencies;
        RefreshUI();
    }

    private void OnClickGoodsToggle(PointerEventData data)
    {
        _shopPopupState = ShopPopupState.Goods;
        RefreshUI();
    }
}
