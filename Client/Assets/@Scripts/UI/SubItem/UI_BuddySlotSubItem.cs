using Spine.Unity;
using UnityEngine.EventSystems;

public class UI_BuddySlotSubItem : UI_SubItem
{
    public enum EBuddySlotTypte
    {
        None,
        Buddies,
        BuddyInfo,
        BuddySelected,
        Heroes,
    }

    enum Images
    {
        Image_BuddySlotSubItem,
    }

    enum Objects
    {
        BuddySlotObject,
    }

    public EBuddySlotTypte slotType;
    private SkeletonGraphic skeletonAnimation;
    private Data.BuddyData buddyData;
    private Data.HeroData heroData;
    private int templateId;

    protected override void Awake()
    {
        base.Awake();

        BindImages(typeof(Images));
        BindObjects(typeof(Objects));

        skeletonAnimation = GetObject((int)Objects.BuddySlotObject).GetComponent<SkeletonGraphic>();

        GetImage((int)Images.Image_BuddySlotSubItem).gameObject.BindEvent(BuddySlotClicked);

        RefreshUI();
    }

    public void SetInfo(int templateId, EBuddySlotTypte setSlotType)
    {
        slotType = setSlotType;
        this.templateId = templateId;

        if (templateId == 0)
            return;


        if(slotType == EBuddySlotTypte.None)
        {
            return;
        }
        else if(slotType == EBuddySlotTypte.Heroes)
        {
            buddyData = null;
            heroData = Managers.Data.HeroDataDic[templateId];
        }
        else
        {
            buddyData = Managers.Data.BuddyDataDic[templateId];
            heroData = null;
        }

        if (isInit == false)
            return;

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (isInit == false)
            return;

        skeletonAnimation.enabled = false;

        if (templateId == 0)
            return ;

        if (slotType == EBuddySlotTypte.None)
            return;

        skeletonAnimation.enabled = true;
        if(slotType == EBuddySlotTypte.Heroes)
        {
            skeletonAnimation.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(heroData.SpineNameKey);
            skeletonAnimation.Initialize(true);
            // TO DO need to chage?
            skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);

        }
        else
        {
            skeletonAnimation.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(buddyData.SpineNameKey);
            skeletonAnimation.Initialize(true);
        }

        //skeletonAnimation.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(buddyData.SpineNameKey) ?? Managers.Resource.Load<SkeletonDataAsset>(heroData.SpineNameKey);
        
    }

    private void BuddySlotClicked(PointerEventData eventdata)
    {
        if(slotType == EBuddySlotTypte.BuddySelected)
        {
            // 셀렉트가 아닌걸로 해준다
            if(Managers.Game.SelectedBuddyRemove(buddyData.TemplateId) == true)
            {
                // 이미지 지우고
                //skeletonAnimation.gameObject.SetActive(false);
                //skeletonAnimation.Initialize(true);
            }
        }
        else if (slotType == EBuddySlotTypte.Buddies)
        {
            Managers.Game.SelectedBuddySet(buddyData.TemplateId);
        }
        else if(slotType == EBuddySlotTypte.Heroes)
        {
            Managers.Game.NowHero = heroData.TemplateId;
        }
    }
}
