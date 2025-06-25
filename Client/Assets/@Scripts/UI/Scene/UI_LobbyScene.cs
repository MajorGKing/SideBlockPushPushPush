using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LobbyScene : UI_Scene
{
    enum GameObjects
    {
        ShopToggleRedDotObject, // �˸� ��Ȳ �� ��� �� �����
        LevelToggleRedDotObject,
        BattleToggleRedDotObject,

        MenuToggleGroup,
        CheckShopImageObject,
        CheckLevelImageObject,
        CheckBattleImageObject,
    }

    enum Texts
    {
        ShopToggleText,
        LevelToggleText,
        BattleToggleText,
    }

    enum Toggles
    {
        ShopToggle,
        LevelToggle,
        BattleToggle,
    }

    public UI_BattlePopup BattlePopupUI { get; private set; }
    bool _isSelectedBattle = false;

    bool _isSelectedLevel = false;

    bool _isSelectedShop = false;


    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));
        BindToggles(typeof(Toggles));

        // ��� Ŭ�� �� �ൿ
        GetToggle((int)Toggles.ShopToggle).gameObject.BindEvent(OnClickShopToggle);
        GetToggle((int)Toggles.LevelToggle).gameObject.BindEvent(OnClickLevelToggle);
        GetToggle((int)Toggles.BattleToggle).gameObject.BindEvent(OnClickBattleToggle);

        BattlePopupUI = Managers.UI.ShowPopupUI<UI_BattlePopup>();
        GetToggle((int)Toggles.BattleToggle).gameObject.GetComponent<Toggle>().isOn = true;
        OnClickBattleToggle();

        TogglesInit();

        GetText((int)Texts.ShopToggleText).gameObject.SetActive(true);
        GetObject((int)GameObjects.CheckShopImageObject).SetActive(true);
    }

    #region Toggle
    private void TogglesInit()
    {
        BattlePopupUI.gameObject.SetActive(false);

        // �� Ŭ�� ���� Ʈ���� �ʱ�ȭ
        _isSelectedLevel = false;
        _isSelectedShop = false;
        _isSelectedBattle = false;

        // ��ư ����� �ʱ�ȭ
        GetObject((int)GameObjects.ShopToggleRedDotObject).SetActive(false);
        GetObject((int)GameObjects.LevelToggleRedDotObject).SetActive(false);
        GetObject((int)GameObjects.BattleToggleRedDotObject).SetActive(false);

        // ���� ��� ������ �ʱ�ȭ
        GetObject((int)GameObjects.CheckShopImageObject).SetActive(false);
        GetObject((int)GameObjects.CheckLevelImageObject).SetActive(false);
        GetObject((int)GameObjects.CheckBattleImageObject).SetActive(false);

        // �޴� �ؽ�Ʈ �ʱ�ȭ
        GetText((int)Texts.ShopToggleText).gameObject.SetActive(false);
        GetText((int)Texts.LevelToggleText).gameObject.SetActive(false);
        GetText((int)Texts.BattleToggleText).gameObject.SetActive(false);
    }

    private void OnClickBattleToggle(PointerEventData evt)
    {
        OnClickBattleToggle();
    }

    private void OnClickBattleToggle()
    {
        Managers.Sound.PlayButtonClick();
        if (_isSelectedBattle == true) // Ȱ��ȭ �� ��� Ŭ�� ����
            return;

        TogglesInit();
        BattlePopupUI.gameObject.SetActive(true);
        GetText((int)Texts.BattleToggleText).gameObject.SetActive(true);
        GetObject((int)GameObjects.CheckBattleImageObject).SetActive(true);
        _isSelectedBattle = true;
    }

    private void OnClickShopToggle(PointerEventData evt)
    {
        Managers.Sound.PlayButtonClick();
        if(_isSelectedShop == true)
            return;

        TogglesInit();
        GetText((int)Texts.ShopToggleText).gameObject.SetActive(true);
        GetObject((int)GameObjects.CheckShopImageObject).SetActive(true);
        _isSelectedShop = true;

    }

    private void OnClickLevelToggle(PointerEventData evt)
    {
        Managers.Sound.PlayButtonClick();
        if(_isSelectedLevel == true)
            return;

        TogglesInit();
        GetText((int)Texts.LevelToggleText).gameObject.SetActive(true);
        GetObject((int)GameObjects.CheckLevelImageObject).SetActive(true);
        _isSelectedLevel = true;
    }

    #endregion
}
