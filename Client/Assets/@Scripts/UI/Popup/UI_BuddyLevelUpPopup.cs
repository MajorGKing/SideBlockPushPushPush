using Cysharp.Threading.Tasks;
using Spine.Unity;
using System.Collections.Generic;
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
        Text_BuddyLevel,
        Text_BuddyAttack,
        Text_BuddyMagic,
        Text_BuddyReload,
    }

    enum Objects
    {
        UI_SelectedBuddySlotSubItem1,
        UI_SelectedBuddySlotSubItem2,
        UI_SelectedBuddySlotSubItem3,
        UI_SelectedBuddySlotSubItem4,
        UI_LevelUpCurrencySubItem1,
        UI_LevelUpCurrencySubItem2,
        UI_LevelUpCurrencySubItem3,
        UI_LevelUpCurrencySubItem4,
        UI_NowBuddySubItem,
        BuddySKillContent,
        BuddySkillUpContent,
        BuddiesContent,
    }

    enum Buttons
    {
        Button_BuddyLevelUp,
    }

    private List<UI_RewardsSubItem> buddyLevelUpCurrencies;
    private UI_BuddySlotSubItem[] selectedBuddies;

    protected override void Awake()
    {
        base.Awake();

        BindTexts(typeof(Texts));
        BindGameObjects(typeof(Objects));
        BindButtons(typeof(Buttons));

        // Selected Buddy
        GetButton((int)Buttons.Button_BuddyLevelUp).gameObject.BindEvent(OnClickedBuddyLevelUpButton);

        buddyLevelUpCurrencies = new List<UI_RewardsSubItem>();
        buddyLevelUpCurrencies.Add(GetGameObject((int)Objects.UI_LevelUpCurrencySubItem1).GetComponent<UI_RewardsSubItem>());
        buddyLevelUpCurrencies.Add(GetGameObject((int)Objects.UI_LevelUpCurrencySubItem2).GetComponent<UI_RewardsSubItem>());
        buddyLevelUpCurrencies.Add(GetGameObject((int)Objects.UI_LevelUpCurrencySubItem3).GetComponent<UI_RewardsSubItem>());
        buddyLevelUpCurrencies.Add(GetGameObject((int)Objects.UI_LevelUpCurrencySubItem4).GetComponent<UI_RewardsSubItem>());

        selectedBuddies = new UI_BuddySlotSubItem[4];
        selectedBuddies[0] = GetGameObject((int)Objects.UI_SelectedBuddySlotSubItem1).GetComponent<UI_BuddySlotSubItem>();
        selectedBuddies[1] = GetGameObject((int)Objects.UI_SelectedBuddySlotSubItem2).GetComponent<UI_BuddySlotSubItem>();
        selectedBuddies[2] = GetGameObject((int)Objects.UI_SelectedBuddySlotSubItem3).GetComponent<UI_BuddySlotSubItem>();
        selectedBuddies[3] = GetGameObject((int)Objects.UI_SelectedBuddySlotSubItem4).GetComponent<UI_BuddySlotSubItem>();

        GetGameObject((int)Objects.BuddySKillContent).DestroyChildren();
        GetGameObject((int)Objects.BuddySkillUpContent).DestroyChildren();
        GetGameObject((int)Objects.BuddiesContent).DestroyChildren();


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

        Managers.Game.OnSelectedBuddyChanged -= SetInfo;
        Managers.Game.OnSelectedBuddyChanged += SetInfo;

        RefreshUI();
    }

    private void OnDisable()
    {
        Managers.Game.OnCurrenciesChagned -= SetInfo;
        Managers.Game.OnNowBuddyChanged -= SetInfo;
        Managers.Game.OnSelectedBuddyChanged -= SetInfo;
    }

    private void RefreshUI()
    {
        if (isInit == false)
            return;

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

        // Buddies 관련
        {
            GetGameObject((int)Objects.BuddiesContent).DestroyChildren();

            var buddies = Managers.Game.BuddyData;
            foreach(var buddy in buddies)
            {
                if(buddy == null) 
                    continue;

                if (buddy.TemplateId == 0)
                    continue;

                var item = Managers.UI.MakeSubItem<UI_BuddySlotSubItem>(GetGameObject((int)Objects.BuddiesContent).transform);
                item.SetInfo(buddy.TemplateId, UI_BuddySlotSubItem.EBuddySlotTypte.Buddies);
            }
        }

        // Selected
        {
            for(int i = 0; i < selectedBuddies.Length; i++)
            {
                selectedBuddies[i].SetInfo(Managers.Game.SelectedBuddyGet(i), UI_BuddySlotSubItem.EBuddySlotTypte.BuddySelected);
            }
        }

        // Now Buddy 관련
        int nowBuddyTemplateId = Managers.Game.NowBuddy;
        GetButton((int)Buttons.Button_BuddyLevelUp).interactable = false;
        if (nowBuddyTemplateId == 0)
        {
            Utils.FindChild<SkeletonGraphic>(GetGameObject((int)Objects.UI_NowBuddySubItem), null, true).enabled = false;

            foreach (var buddyLevelUpCurrency in buddyLevelUpCurrencies)
            {
                buddyLevelUpCurrency.gameObject.SetActive(false);
            }

            GetGameObject((int)Objects.BuddySKillContent).DestroyChildren();

            // Stat Update
            GetText((int)Texts.Text_BuddyLevel).text = $"Level : ";
            GetText((int)Texts.Text_BuddyAttack).text = $"Attack : ";
            GetText((int)Texts.Text_BuddyMagic).text = $"Magic : ";
            GetText((int)Texts.Text_BuddyReload).text = $"Reload : ";
        }
        else
        {
            var nowBuddySkeletonGraphic = Utils.FindChild<SkeletonGraphic>(GetGameObject((int)Objects.UI_NowBuddySubItem), null, true);
            nowBuddySkeletonGraphic.enabled = true;
            var nowBuddyData = Managers.Data.BuddyDataDic[nowBuddyTemplateId];
            nowBuddySkeletonGraphic.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(nowBuddyData.SpineNameKey);
            nowBuddySkeletonGraphic.Initialize(true);

            // Stat Update
            GetText((int)Texts.Text_BuddyLevel).text = $"Level : {nowBuddyData.Level}";
            GetText((int)Texts.Text_BuddyAttack).text = $"Attack : {nowBuddyData.Attack}";
            GetText((int)Texts.Text_BuddyMagic).text = $"Magic : {nowBuddyData.MagicAttack}";
            GetText((int)Texts.Text_BuddyReload).text = $"Reload : {nowBuddyData.Reload:F2}";

            foreach (var buddyLevelUpCurrency in buddyLevelUpCurrencies)
            {
                buddyLevelUpCurrency.gameObject.SetActive(false);
            }

            for (int i = 0; i < nowBuddyData.LevelUpCurrencies.Count; i++)
            {
                buddyLevelUpCurrencies[i].gameObject.SetActive(true);
                buddyLevelUpCurrencies[i].SetInfo(nowBuddyData.LevelUpCurrencies[i].currencyType, nowBuddyData.LevelUpCurrencies[i].count, false);
            }

            if(nowBuddyData.LevelUpCurrencies.Count > 0)
            {
                GetButton((int)Buttons.Button_BuddyLevelUp).interactable = true;
            }
            

            var buddySaveData = Managers.Game.GetBuddyData(nowBuddyTemplateId);

            if (buddySaveData == null)
                return;

            GetGameObject((int)Objects.BuddySKillContent).DestroyChildren();

            foreach (var templatedId in buddySaveData.SkillTemplateId)
            {
                Debug.Log("templatedId " + templatedId);

                if (templatedId == 0)
                    continue;

                var item = Managers.UI.MakeSubItem<UI_BuddySKillSubItem>(GetGameObject((int)Objects.BuddySKillContent).transform);
                item.SetInfo(templatedId);
            }

        }

        // SKillLevelUp
        {
            //if (nowBuddyIndex == 0)
            //{
            //    
            //}
            //else
            //{
            GetGameObject((int)Objects.BuddySkillUpContent).DestroyChildren();

            var saveData = Managers.Game.GetBuddyData(nowBuddyTemplateId);

            if (saveData == null)
                return;

            foreach (var tempalteId in saveData.SkillTemplateId)
            {
                if (tempalteId == 0)
                    continue;

                //GetObject((int)Objects.BuddySkillUpContent).DestroyChildren();

                var item = Managers.UI.MakeSubItem<UI_BuddySkillUpSubItem>(GetGameObject((int)Objects.BuddySkillUpContent).transform);
                item.SetInfo(tempalteId);
            }
            //}
        }
    }

    private void OnClickedBuddyLevelUpButton(PointerEventData eventData)
    {
        if (GetButton((int)Buttons.Button_BuddyLevelUp).interactable == false)
            return;

        Managers.Game.BuddyLevelUp().Forget();

        Debug.Log("On Button Clicked");
    }
}
