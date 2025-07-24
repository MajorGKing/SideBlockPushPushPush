using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_BuddySlotSubItem : UI_SubItem
{
    public enum EBuddySlotTypte
    {
        None,
        Buddies,
        BuddyInfo,
        BuddySelected
    }

    enum Images
    {
        UI_BuddySlotSubItem,
    }

    public EBuddySlotTypte slotType;
    private SkeletonAnimation skeletonAnimation;
    private Data.BuddyData buddyData;

    protected override void Awake()
    {
        base.Awake();
        skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();

        BindImages(typeof(Images));

        GetImage((int)Images.UI_BuddySlotSubItem).gameObject.BindEvent(BuddySlotClicked);
    }

    public void SetInfo(int templateId, EBuddySlotTypte setSlotType)
    {
        slotType = setSlotType;

        buddyData = Managers.Data.BuddyDataDic[templateId];

        skeletonAnimation.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(buddyData.SpineNameKey);
        skeletonAnimation.Initialize(true);

    }

    private void BuddySlotClicked(PointerEventData eventdata)
    {
        if(slotType == EBuddySlotTypte.BuddySelected)
        {
            // 이미지 지우고
            skeletonAnimation.skeletonDataAsset = null;
            skeletonAnimation.Initialize(true);

            // 셀렉트가 아닌걸로 해준다
            Managers.Game.SelectedBuddyRemove(buddyData.TemplateId);
        }
        else if (slotType == EBuddySlotTypte.Buddies)
        {
            Managers.Game.SelectedBuddySet(buddyData.TemplateId);
        }
    }
}
