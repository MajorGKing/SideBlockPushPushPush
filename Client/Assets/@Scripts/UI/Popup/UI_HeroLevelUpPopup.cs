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
        BindObjects(typeof(Objects));
        BindButtons(typeof(Buttons));

        GetButton((int)Buttons.Button_HeroLevelUp).gameObject.BindEvent(OnClickedHeroLevelUpButton);

        GetObject((int)Objects.HeroContent).DestroyChildren();
        GetObject((int)Objects.HeroSKillContent).DestroyChildren();

        expBar = GetObject((int)Objects.UI_HeroExpBar).GetComponent<UI_BattleBarWorldSpace>();

        RefreshUI();
    }

    private void OnEnable()
    {
        Managers.Game.OnNowHeroChanged -= SetInfo;
        Managers.Game.OnNowHeroChanged += SetInfo;

        RefreshUI();
    }

    private void OnDisable()
    {
        Managers.Game.OnNowHeroChanged -= SetInfo;
    }

    public void SetInfo()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (isInit == false)
            return;

        Debug.Log("Hero UI RefreshUI start");

        GetText((int)Texts.Text_ExpCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.Exp).ToString();
        GetText((int)Texts.Text_BlueGemCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.BlueGem).ToString();
        GetText((int)Texts.Text_GreenGemCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.GreenGem).ToString();
        GetText((int)Texts.Text_YellowGemCount).text = Managers.Game.GetCurrency(Define.ECurrencyType.YellowGem).ToString();

        // Hero List
        {
            GetObject((int)Objects.HeroContent).DestroyChildren();

            var heroes = Managers.Game.heroes;
            foreach( var hero in heroes )
            {
                if (hero == null) continue;

                if (hero.TemplateId == 0) continue;

                var item = Managers.UI.MakeSubItem<UI_BuddySlotSubItem>(GetObject((int)Objects.HeroContent).transform);
                item.SetInfo(hero.TemplateId, UI_BuddySlotSubItem.EBuddySlotTypte.Heroes);
            }
        }

        // Now Hero
        {
            var nowHeroIndex = Managers.Game.NowHero;

            if (nowHeroIndex == 0)
                return;


            var nowHeroSkeletonGraphic = Utils.FindChild<SkeletonGraphic>(GetObject((int)Objects.UI_NowHeroSubItem), null, true);
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
            var nowHeroSaveData = Managers.Game.GetHeroSaveData(nowHeroIndex);
            expBar.SetInfo(nowHeroSaveData.nowExp, nowHeroSaveData.maxExp, true);

            // skill
            GetObject((int)Objects.HeroSKillContent).DestroyChildren();

            foreach(var templateId in nowHeroSaveData.SkillTemplateId)
            {
                if (templateId == 0) continue;

                var item = Managers.UI.MakeSubItem<UI_HeroSkillUpSubItem>(GetObject((int)Objects.HeroSKillContent).transform);
                item.SetInfo(templateId);
            }
        }
    }

    private void OnClickedHeroLevelUpButton(PointerEventData eventData)
    {
        Managers.Game.HeroLevelUp();
    }
}
