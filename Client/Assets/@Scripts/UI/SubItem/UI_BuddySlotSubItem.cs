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
        Image_BuddySlotSubItem,
    }

    enum Objects
    {
        BuddySlotObject,
    }

    public EBuddySlotTypte slotType;
    private SkeletonGraphic skeletonAnimation;
    private Data.BuddyData buddyData;
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

        if(templateId != 0)
        {
            buddyData = Managers.Data.BuddyDataDic[templateId];
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

        skeletonAnimation.enabled = true;
        skeletonAnimation.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(buddyData.SpineNameKey);
        skeletonAnimation.Initialize(true);
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
    }
}
