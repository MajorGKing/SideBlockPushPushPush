using UnityEngine;

public class UI_ShopPopup : UI_Popup
{
    private enum GameObjects
    {
        HeroArea,
        BuddyArea,
        CurrenyArea,
        GoodsArea,
    }

    private enum Buttons
    {
        Button_HeroGachaCount1,
        Button_HeroGachaCount10,
        Button_BuddyGachaCount1,
        Button_BuddyGachaCount10,
        Button_InApp,
        Button_AD,
    }

    private enum Toggles
    {
        Toggle_SkillGacha,
        Toggle_BuddyGacha,
        Toggle_Currenies,
        Toggle_Goods,
    }

    private enum ShopPopupState
    {
        None,
        HeroGacha,
        BuddyGacha,
        Currency,
        Goods,
    }
}
