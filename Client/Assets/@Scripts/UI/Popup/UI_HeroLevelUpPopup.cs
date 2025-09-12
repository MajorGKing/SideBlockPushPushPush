using NUnit.Framework;
using Spine.Unity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_HeroLevelUpPopup : UI_Popup
{
    enum Texts
    {
        Text_ExpCount,
        Text_BlueGemCount,
        Text_GreenGemCount,
        Text_YellowGemCount,
        Text_HeroLevel,
        Text_HeroAttack,
        Text_HeroMagic,
        Text_HeroReload,
    }

    enum Objects
    {
        UI_NowHeroSubItem,
        UI_HeroExpBar,
        HeroContent,
        HeroSKillContent
    }

    enum Buttons
    {
        Button_HeroLevelUp,
    }

    private UI_BattleBarWorldSpace expBar;

    protected override void Awake()
    {
        base.Awake();

        BindTexts(typeof(Texts));
        BindGameObjects(typeof(Objects));
        BindButtons(typeof(Buttons));

        GetButton((int)Buttons.Button_HeroLevelUp).gameObject.BindEvent(OnClickedHeroLevelUpButton);

        GetGameObject((int)Objects.HeroContent).DestroyChildren();
        GetGameObject((int)Objects.HeroSKillContent).DestroyChildren();

        expBar = GetGameObject((int)Objects.UI_HeroExpBar).GetComponent<UI_BattleBarWorldSpace>();

        RefreshUI();
    }

    private void OnEnable()
    {
        Managers.Game.OnNowHeroChanged -= SetInfo;
        Managers.Game.OnNowHeroChanged += SetInfo;

        Managers.Game.OnCurrenciesChagned -= SetInfo;
        Managers.Game.OnCurrenciesChagned += SetInfo;

        RefreshUI();
    }

    private void OnDisable()
    {
        Managers.Game.OnNowHeroChanged -= SetInfo;

        Managers.Game.OnCurrenciesChagned -= SetInfo;
    }

    public void SetInfo()
    {
        Debug.Log("UILevelUpRefresh");
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (isInit == false)
            return;

        GetText((int)Texts.Text_ExpCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.Exp).ToString();
        GetText((int)Texts.Text_BlueGemCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.BlueGem).ToString();
        GetText((int)Texts.Text_GreenGemCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.GreenGem).ToString();
        GetText((int)Texts.Text_YellowGemCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.YellowGem).ToString();

        // Hero List
        {
            GetGameObject((int)Objects.HeroContent).DestroyChildren();

            var heroes = Managers.Game.HeroData;
            foreach( var hero in heroes )
            {
                if (hero == null) continue;

                if (hero.TemplateId == 0) continue;

                var item = Managers.UI.MakeSubItem<UI_BuddySlotSubItem>(GetGameObject((int)Objects.HeroContent).transform);
                item.SetInfo(hero.TemplateId, UI_BuddySlotSubItem.EBuddySlotTypte.Heroes);
            }
        }

        // Now Hero
        {
            var nowHeroIndex = Managers.Game.NowHero;
            Debug.Log($"Hero UI Now Hero {nowHeroIndex}");

            if (nowHeroIndex == 0)
                return;

            var nowHeroSkeletonGraphic = Utils.FindChild<SkeletonGraphic>(GetGameObject((int)Objects.UI_NowHeroSubItem), null, true);
            nowHeroSkeletonGraphic.enabled = true;
            var nowHeroData = Managers.Data.HeroDataDic[nowHeroIndex];
            nowHeroSkeletonGraphic.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(nowHeroData.SpineNameKey);
            nowHeroSkeletonGraphic.Initialize(true);
            nowHeroSkeletonGraphic.AnimationState.SetAnimation(0, "idle", true);

            // Stat Update
            GetText((int)Texts.Text_HeroLevel).text = $"Level : {nowHeroData.Level}";
            GetText((int)Texts.Text_HeroAttack).text = $"Attack : {nowHeroData.Attack}";
            GetText((int)Texts.Text_HeroMagic).text = $"Magic : {nowHeroData.MagicAttack}";

            // exp
            var nowHeroSaveData = Managers.Game.GetHeroData(nowHeroIndex);
            if (Managers.Data.HeroDataDic[nowHeroSaveData.TemplateId].LevelUpCurrencies.Count != 0)
            {
                expBar.gameObject.SetActive(true);
                expBar.SetInfo(nowHeroSaveData.NowExp, nowHeroSaveData.MaxExp, true);
                GetButton((int)Buttons.Button_HeroLevelUp).interactable = true;
            }
            else
            {
                expBar.gameObject.SetActive(false);
                GetButton((int)Buttons.Button_HeroLevelUp).interactable = false;
            }

            // skill
            GetGameObject((int)Objects.HeroSKillContent).DestroyChildren();

            foreach(var templateId in nowHeroSaveData.SkillTemplateIds)
            {
                if (templateId == 0) continue;

                var item = Managers.UI.MakeSubItem<UI_HeroSkillUpSubItem>(GetGameObject((int)Objects.HeroSKillContent).transform);
                item.SetInfo(templateId);
            }
        }
    }

    private void OnClickedHeroLevelUpButton(PointerEventData eventData)
    {
        if (GetButton((int)Buttons.Button_HeroLevelUp).interactable == false)
            return;

        Managers.Game.HeroLevelUp();
    }
}
