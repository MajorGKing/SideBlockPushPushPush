using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LobbyScene : UI_Scene
{
    enum GameObjects
    {
        ShopToggleRedDotObject, // 알림 상황 시 사용 할 레드닷
        LevelToggleRedDotObject,
        BattleToggleRedDotObject,
        HeroToggleRedDotObject,

        MenuToggleGroup,
        CheckShopImageObject,
        CheckLevelImageObject,
        CheckBattleImageObject,
        CheckHeroImageObject,
    }

    enum Texts
    {
        ShopToggleText,
        LevelToggleText,
        BattleToggleText,
        HeroToggleText,
    }

    enum Toggles
    {
        ShopToggle,
        LevelToggle,
        BattleToggle,
        HeroToggle,
    }

    public UI_BattlePopup BattlePopupUI { get; private set; }
    public UI_BuddyLevelUpPopup BuddyPopupUI { get; private set; }
    public UI_HeroLevelUpPopup HeroPopupUI { get; private set; }
    public UI_ShopPopup ShopPopupUI { get; private set; }

    bool _isSelectedBattle = false;

    bool _isSelectedLevel = false;

    bool _isSelectedShop = false;
    bool _isSelectedHero = false;

    private UI_UserInfoItem _userInfoItem;


    protected override void Awake()
    {
        base.Awake();

        BindGameObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));
        BindToggles(typeof(Toggles));

        // 토글 클릭 시 행동
        GetToggle((int)Toggles.ShopToggle).gameObject.BindEvent(OnClickShopToggle);
        GetToggle((int)Toggles.LevelToggle).gameObject.BindEvent(OnClickLevelToggle);
        GetToggle((int)Toggles.BattleToggle).gameObject.BindEvent(OnClickBattleToggle);
        GetToggle((int)Toggles.HeroToggle).gameObject.BindEvent(OnClickHeroToggle);

        BattlePopupUI = Managers.UI.ShowPopupUI<UI_BattlePopup>();
        GetToggle((int)Toggles.BattleToggle).gameObject.GetComponent<Toggle>().isOn = true;
        OnClickBattleToggle();

        BuddyPopupUI = Managers.UI.ShowPopupUI<UI_BuddyLevelUpPopup>();
        
        HeroPopupUI = Managers.UI.ShowPopupUI<UI_HeroLevelUpPopup>();

        ShopPopupUI = Managers.UI.ShowPopupUI<UI_ShopPopup>();

        TogglesInit();

        GetText((int)Texts.ShopToggleText).gameObject.SetActive(true);
        GetGameObject((int)GameObjects.CheckShopImageObject).SetActive(true);

        _userInfoItem = Utils.FindChild<UI_UserInfoItem>(gameObject);
        //_userInfoItem.SetInfo(Define.EUserInfoItem.Stamina, Managers.Game.Stamina);
        //_userInfoItem.SetInfo(Define.EUserInfoItem.Dia, Managers.Game.GetCurrency(Define.ECurrencyType.Dia));
        //_userInfoItem.SetInfo(Define.EUserInfoItem.Gold, Managers.Game.GetCurrency(Define.ECurrencyType.Gold));

        UICanvas.renderMode = RenderMode.ScreenSpaceCamera;
        UICanvas.worldCamera = Camera.main;
    }

    #region Toggle
    private void TogglesInit()
    {
        if (BattlePopupUI == null)
            return;

        if (BuddyPopupUI == null)
            return;

        if (HeroPopupUI == null)
            return;
        
        if (ShopPopupUI == null)
            return;

        BattlePopupUI.gameObject.SetActive(false);
        BuddyPopupUI.gameObject.SetActive(false);
        HeroPopupUI.gameObject.SetActive(false);
        ShopPopupUI.gameObject.SetActive(false);

        // 재 클릭 방지 트리거 초기화
        _isSelectedLevel = false;
        _isSelectedShop = false;
        _isSelectedBattle = false;
        _isSelectedHero = false;

        // 버튼 레드닷 초기화
        GetGameObject((int)GameObjects.ShopToggleRedDotObject).SetActive(false);
        GetGameObject((int)GameObjects.LevelToggleRedDotObject).SetActive(false);
        GetGameObject((int)GameObjects.BattleToggleRedDotObject).SetActive(false);
        GetGameObject((int)GameObjects.HeroToggleRedDotObject).SetActive(false);

        // 선택 토글 아이콘 초기화
        GetGameObject((int)GameObjects.CheckShopImageObject).SetActive(false);
        GetGameObject((int)GameObjects.CheckLevelImageObject).SetActive(false);
        GetGameObject((int)GameObjects.CheckBattleImageObject).SetActive(false);
        GetGameObject((int)GameObjects.CheckHeroImageObject).SetActive(false);

        // 메뉴 텍스트 초기화
        GetText((int)Texts.ShopToggleText).gameObject.SetActive(false);
        GetText((int)Texts.LevelToggleText).gameObject.SetActive(false);
        GetText((int)Texts.BattleToggleText).gameObject.SetActive(false);
        GetText((int)Texts.HeroToggleText).gameObject.SetActive(false);
    }

    private void OnClickBattleToggle(PointerEventData evt)
    {
        OnClickBattleToggle();
    }

    private void OnClickBattleToggle()
    {
        Managers.Sound.PlayButtonClick();
        if (_isSelectedBattle == true) // 활성화 후 토글 클릭 방지
            return;

        TogglesInit();
        BattlePopupUI.gameObject.SetActive(true);
        GetText((int)Texts.BattleToggleText).gameObject.SetActive(true);
        GetGameObject((int)GameObjects.CheckBattleImageObject).SetActive(true);
        _isSelectedBattle = true;
    }

    private void OnClickShopToggle(PointerEventData evt)
    {
        Managers.Sound.PlayButtonClick();
        if(_isSelectedShop == true)
            return;

        TogglesInit();
        ShopPopupUI.gameObject.SetActive(true);
        GetText((int)Texts.ShopToggleText).gameObject.SetActive(true);
        GetGameObject((int)GameObjects.CheckShopImageObject).SetActive(true);
        _isSelectedShop = true;

    }

    private void OnClickLevelToggle(PointerEventData evt)
    {
        Managers.Sound.PlayButtonClick();
        if(_isSelectedLevel == true)
            return;

        TogglesInit();
        BuddyPopupUI.gameObject.SetActive(true);
        GetText((int)Texts.LevelToggleText).gameObject.SetActive(true);
        GetGameObject((int)GameObjects.CheckLevelImageObject).SetActive(true);
        _isSelectedLevel = true;
    }

    private void OnClickHeroToggle(PointerEventData evt)
    {
        Managers.Sound.PlayButtonClick();
        if (_isSelectedHero == true)
            return;

        TogglesInit();
        HeroPopupUI.gameObject.SetActive(true);
        GetText((int)Texts.HeroToggleText).gameObject.SetActive(true);
        GetGameObject((int)GameObjects.CheckHeroImageObject).SetActive(true);
        _isSelectedHero = true;

    }

    #endregion
}
