using Cysharp.Threading.Tasks;
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
        BindGameObjects(typeof(Objects));
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
        GetButton((int)Buttons.Button_HeroGachaCount1).gameObject.BindEvent(OnClickHeroGachaCount1, Define.ETouchEvent.PointerUp);
        GetButton((int)Buttons.Button_HeroGachaCount10).gameObject.BindEvent(OnClickHeroGachaCount10, Define.ETouchEvent.PointerUp);
        //GetButton((int)Buttons.Button_BuddyGachaCount10).gameObject.BindEvent(OnClickBuddyGachaCount10);

        GetButton((int)Buttons.Button_CurrenciesGachaCount1).gameObject.BindEvent(OnClickCurrencyGachaCount1, Define.ETouchEvent.PointerUp);
        GetButton((int)Buttons.Button_CurrenciesGachaCount10).gameObject.BindEvent(OnClickCurrencyGachaCount10, Define.ETouchEvent.PointerUp);
        GetButton((int)Buttons.Button_CurrenciesGachaCount100).gameObject.BindEvent(OnClickCurrencyGachaCount100, Define.ETouchEvent.PointerUp);

        GetButton((int)Buttons.Button_BuddyGachaCount1).gameObject.BindEvent(OnClickBuddyGachaCount1, Define.ETouchEvent.PointerUp);
        GetButton((int)Buttons.Button_BuddyGachaCount10).gameObject.BindEvent(OnClickBuddyGachaCount10, Define.ETouchEvent.PointerUp);

        RefreshUI();
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
                GetGameObject((int)Objects.HeroArea).SetActive(true);
                GetGameObject((int)Objects.BuddyArea).SetActive(false);
                GetGameObject((int)Objects.CurrenciesArea).SetActive(false);
                GetGameObject((int)Objects.GoodsArea).SetActive(false);
                break;
            case ShopPopupState.BuddyGacha:
                GetGameObject((int)Objects.HeroArea).SetActive(false);
                GetGameObject((int)Objects.BuddyArea).SetActive(true);
                GetGameObject((int)Objects.CurrenciesArea).SetActive(false);
                GetGameObject((int)Objects.GoodsArea).SetActive(false);
                break;
            case ShopPopupState.Currencies:
                GetGameObject((int)Objects.HeroArea).SetActive(false);
                GetGameObject((int)Objects.BuddyArea).SetActive(false);
                GetGameObject((int)Objects.CurrenciesArea).SetActive(true);
                GetGameObject((int)Objects.GoodsArea).SetActive(false);
                break;
            case ShopPopupState.Goods:
                GetGameObject((int)Objects.HeroArea).SetActive(false);
                GetGameObject((int)Objects.BuddyArea).SetActive(false);
                GetGameObject((int)Objects.CurrenciesArea).SetActive(false);
                GetGameObject((int)Objects.GoodsArea).SetActive(true);
                break;

            default:
                GetGameObject((int)Objects.HeroArea).SetActive(false);
                GetGameObject((int)Objects.BuddyArea).SetActive(false);
                GetGameObject((int)Objects.CurrenciesArea).SetActive(false);
                GetGameObject((int)Objects.GoodsArea).SetActive(false);
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

    private void OnClickHeroGachaCount(int count)
    {
        Managers.Game.DoHeroGacha(count).Forget();
    }

    private void OnClickHeroGachaCount1(PointerEventData data)
    {
        OnClickHeroGachaCount(1);
    }

    private void OnClickHeroGachaCount10(PointerEventData data)
    {
        OnClickHeroGachaCount(10);
    }

    private void OnClickCurrencyGachaCount(int count)
    {
        Managers.Game.DoCurrencyGacha(count);
    }

    private void OnClickCurrencyGachaCount1(PointerEventData data)
    {
        OnClickCurrencyGachaCount(1);
    }

    private void OnClickCurrencyGachaCount10(PointerEventData data)
    {
        OnClickCurrencyGachaCount(10);
    }

    private void OnClickCurrencyGachaCount100(PointerEventData data)
    {
        OnClickCurrencyGachaCount(100);
    }

    private void OnClickBuddyGachaCount(int count)
    {
        Managers.Game.DoBuddyGacha(count);
    }

    private void OnClickBuddyGachaCount10(PointerEventData data)
    {
        OnClickBuddyGachaCount(10);
    }

    private void OnClickBuddyGachaCount1(PointerEventData data)
    {
        OnClickBuddyGachaCount(1);
    }
}