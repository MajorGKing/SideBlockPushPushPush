using Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_HeroSkillUpSubItem : UI_SubItem
{
    enum Objects
    {
        UI_RewardSubItem1,
        UI_RewardSubItem2,
        UI_RewardSubItem3,
    }

    enum Images
    {
        Image_HeroSKill1,
        Image_HeroSKill2,
        Image_HeroSKill3,
        Image_HeroSKill4,
        Image_HeroSKill5,
        Image_HeroSKill6,
    }

    enum Buttons
    {
        Button_SkillUp,
    }

    enum Texts
    {
        Text_HeroSKillInfo,
    }

    private int templateId;
    private List<UI_RewardsSubItem> heroSkillUpCurrencies;
    private List<Image> heroSkillImages;

    protected override void Awake()
    {
        base.Awake();

        BindGameObjects(typeof(Objects));
        BindImages(typeof(Images));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        heroSkillImages = new List<Image>();
        heroSkillImages.Add(GetImage((int)Images.Image_HeroSKill1));
        heroSkillImages.Add(GetImage((int)Images.Image_HeroSKill2));
        heroSkillImages.Add(GetImage((int)Images.Image_HeroSKill3));
        heroSkillImages.Add(GetImage((int)Images.Image_HeroSKill4));
        heroSkillImages.Add(GetImage((int)Images.Image_HeroSKill5));
        heroSkillImages.Add(GetImage((int)Images.Image_HeroSKill6));

        heroSkillUpCurrencies = new List<UI_RewardsSubItem>();
        heroSkillUpCurrencies.Add(GetGameObject((int)Objects.UI_RewardSubItem1).GetComponent<UI_RewardsSubItem>());
        heroSkillUpCurrencies.Add(GetGameObject((int)Objects.UI_RewardSubItem2).GetComponent<UI_RewardsSubItem>());
        heroSkillUpCurrencies.Add(GetGameObject((int)Objects.UI_RewardSubItem3).GetComponent<UI_RewardsSubItem>());

        GetButton((int)Buttons.Button_SkillUp).gameObject.BindEvent(OnClickedSkillUpButton);

        RefreshUI();


    }

    public void SetInfo(int skillTemplateId)
    {
        templateId = skillTemplateId;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (templateId == 0)
            return;

        GetButton((int)Buttons.Button_SkillUp).interactable = false;

        var skillData = Managers.Data.HeroSkillDataDic[templateId];

        // skill image
        foreach(var image in heroSkillImages)
        {
            image.gameObject.SetActive(false);
        }


        for(int i = 0; i < skillData.IconImageKeys.Count; i++)
        {
            if(string.IsNullOrEmpty(skillData.IconImageKeys[i]) == true)
            {
                heroSkillImages[i].sprite = null;
            }
            else
            {
                heroSkillImages[i].gameObject.SetActive(true);
                heroSkillImages[i].sprite = Managers.Resource.Load<Sprite>(skillData.IconImageKeys[i]);
            }
        }

        // skill info text
        {
            string text = "";
            text = Managers.GetText(skillData.NameTextId);
            text += $"\tLevel : {skillData.SkillLevel}";
            text += $"\n{Managers.GetText(skillData.DescriptionTextId)}";
            text += $"\n{Managers.GetText(Define.DESCRIPTIONATTACKPERCENT)} : {Managers.Data.EffectDataDic[skillData.EffectDataId].DamageValue}";
            text += $"\t{Managers.GetText(Define.DESCRIPTIONATTACKSPEED)} : {skillData.AnimSpeed}";

            GetText((int)Texts.Text_HeroSKillInfo).text = text;
        }

        // skill level up cost
        foreach(var currency in heroSkillUpCurrencies)
        {
            currency.gameObject.SetActive(false);
        }

        for(int i = 0; i < skillData.LevelUpCurrencies.Count; i++)
        {
            heroSkillUpCurrencies[i].gameObject.SetActive(true);
            heroSkillUpCurrencies[i].SetInfo(skillData.LevelUpCurrencies[i].currencyType, skillData.LevelUpCurrencies[i].count, false);
        }

        if(skillData.LevelUpCurrencies.Count > 0)
        {
            GetButton((int)Buttons.Button_SkillUp).interactable = true;
        }
    }

    private void OnClickedSkillUpButton(PointerEventData eventData)
    {
        if (GetButton((int)Buttons.Button_SkillUp).interactable == false)
            return;

        Managers.Game.HeroSkillUp(templateId);
    }
}
