using Cysharp.Threading.Tasks;
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
        BindGameObjects(typeof(Objects));

        skeletonAnimation = GetGameObject((int)Objects.BuddySlotObject).GetComponent<SkeletonGraphic>();

        GetImage((int)Images.Image_BuddySlotSubItem).gameObject.BindEvent(BuddySlotClicked);

        RefreshUI();
    }

    public void SetInfo(int templateId, EBuddySlotTypte setSlotType)
    {
        slotType = setSlotType;
        this.templateId = templateId;

        //if (templateId == 0)
        //    return;


        if(slotType == EBuddySlotTypte.None)
        {
            return;
        }
        else if(slotType == EBuddySlotTypte.Heroes)
        {
            if (templateId == 0)
                return;

            buddyData = null;
            heroData = Managers.Data.HeroDataDic[templateId];
        }
        // 빈칸이 있을 수 있어 특별히 따로 처리
        else if(slotType == EBuddySlotTypte.BuddySelected)
        {
            heroData = null;
            buddyData = null;

            if (templateId != 0)
            {
                buddyData = Managers.Data.BuddyDataDic[templateId];
            }
        }
        else
        {
            if (templateId == 0)
                return;

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
            return;

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
            Managers.Game.SelectedBuddyRemove(buddyData.TemplateId).Forget();
        }
        else if (slotType == EBuddySlotTypte.Buddies)
        {
            Managers.Game.SelectedBuddyAdd(buddyData.TemplateId).Forget();
        }
        else if(slotType == EBuddySlotTypte.Heroes)
        {
            Managers.Game.NowHeroSetAsync(heroData.TemplateId).Forget();
        }
    }
}
