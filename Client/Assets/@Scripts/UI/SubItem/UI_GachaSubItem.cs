using Data;
using Spine.Unity;
using System.Collections;
using UnityEngine;

public class UI_GachaSubItem : UI_SubItem
{
    private enum Images
    {
        Image_BuddyArea,
        Image_BuddyCurrency,
    }

    private enum Objects
    {
        Spine_Buddy,
    }

    private enum Texts
    {
        Text_BuddyRarity,
        Text_BuddyCurrencyCount,
    }

    private SkeletonGraphic skeletonAnimation;
    private bool isDuplication;
    private BuddyGachaData gachaData;

    private IEnumerator showCurrencyCoroutine;

    protected override void Awake()
    {
        base.Awake();

        BindImages(typeof(Images));
        BindObjects(typeof(Objects));
        BindTexts(typeof(Texts));

        skeletonAnimation = GetObject((int)Objects.Spine_Buddy).GetComponent<SkeletonGraphic>();
        skeletonAnimation.gameObject.SetActive(false);

        GetImage((int)Images.Image_BuddyCurrency).gameObject.SetActive(false);
        GetText((int)Texts.Text_BuddyRarity).gameObject.SetActive(false);
        GetText((int)Texts.Text_BuddyCurrencyCount).gameObject.SetActive(false);

        showCurrencyCoroutine = null;

        RefreshUI();
    }

    private void OnDisable()
    {
        //if (showCurrencyCoroutine != null)
        //{
        //    StopAllCoroutines();
        //    showCurrencyCoroutine = null;
        //}
    }

    public void SetInfo(string buddyName, bool duplication = false)
    {
        gachaData = Managers.Data.BuddyGachaDataDic[buddyName];
        isDuplication = duplication;

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (isInit == false)
            return;

        if (gachaData == null)
            return;

        skeletonAnimation.gameObject.SetActive(false);
        GetText((int)Texts.Text_BuddyRarity).gameObject.SetActive(false);

        GetImage((int)Images.Image_BuddyCurrency).gameObject.SetActive(false);
        GetText((int)Texts.Text_BuddyCurrencyCount).gameObject.SetActive(false);


        skeletonAnimation.gameObject.SetActive(true);

        GetText((int)Texts.Text_BuddyRarity).gameObject.SetActive(true);

        skeletonAnimation.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(gachaData.SpineNameKey);
        skeletonAnimation.Initialize(true);
        skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);

        GetText((int)Texts.Text_BuddyRarity).text = gachaData.Rarity.ToString();

        if(isDuplication == true)
        {
            //if (showCurrencyCoroutine != null)
            //{
            //    StopCoroutine(showCurrencyCoroutine);
            //    showCurrencyCoroutine = null;
            //}

            showCurrencyCoroutine = ShowCurrency();
            StartCoroutine(showCurrencyCoroutine);
        }
    }

    private IEnumerator ShowCurrency()
    {
        Debug.Log($"{gameObject.name} is currency");
        yield return new WaitForSeconds(2f);

        skeletonAnimation.gameObject.SetActive(false);
        GetText((int)Texts.Text_BuddyRarity).gameObject.SetActive(false);

        GetImage((int)Images.Image_BuddyCurrency).gameObject.SetActive(true);
        GetText((int)Texts.Text_BuddyCurrencyCount).gameObject.SetActive(true);

        GetImage((int)Images.Image_BuddyCurrency).sprite = Managers.Resource.Load<Sprite>(Managers.Data.CurrencyTypeDataDic[gachaData.CurrencyType].IconImage);
        GetText((int)Texts.Text_BuddyCurrencyCount).text = gachaData.CurrencyCount.ToString();
    }
}
