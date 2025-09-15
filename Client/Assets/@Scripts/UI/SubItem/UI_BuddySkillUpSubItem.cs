using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_BuddySkillUpSubItem : UI_SubItem
{
    enum Buttons
    {
        Button_SkillUp,
    }

    enum Images
    {
        Button_SkillUp,
    }

    enum Objects
    {
        UI_RewardSubItem1,
        UI_RewardSubItem2,
        UI_RewardSubItem3,
        UI_RewardSubItem4,
        UI_RewardSubItem5,
    }

    private List<UI_RewardsSubItem> buddySKillUpCurrencies;
    private int templateId;

    protected override void Awake()
    {
        base.Awake();

        // Bind
        BindButtons(typeof(Buttons));
        BindGameObjects(typeof(Objects));
        BindImages(typeof(Images));

        buddySKillUpCurrencies = new List<UI_RewardsSubItem>();
        buddySKillUpCurrencies.Add(GetGameObject((int)Objects.UI_RewardSubItem1).GetComponent<UI_RewardsSubItem>());
        buddySKillUpCurrencies.Add(GetGameObject((int)Objects.UI_RewardSubItem2).GetComponent<UI_RewardsSubItem>());
        buddySKillUpCurrencies.Add(GetGameObject((int)Objects.UI_RewardSubItem3).GetComponent<UI_RewardsSubItem>());
        buddySKillUpCurrencies.Add(GetGameObject((int)Objects.UI_RewardSubItem4).GetComponent<UI_RewardsSubItem>());
        buddySKillUpCurrencies.Add(GetGameObject((int)Objects.UI_RewardSubItem5).GetComponent<UI_RewardsSubItem>());

        GetButton((int)Buttons.Button_SkillUp).gameObject.BindEvent(OnClickedBuddySkillUpButton);

        RefreshUI();
    }

    public void SetInfo(int skillTemplateId)
    {
        templateId = skillTemplateId;
        RefreshUI();
    }

    private void RefreshUI()
    {
        GetButton((int)Buttons.Button_SkillUp).gameObject.SetActive(false);
        if (templateId == 0)
        {
            return;
        }

        var data = Managers.Data.BuddySkillDataDic[templateId];
        GetImage((int)Images.Button_SkillUp).sprite = Managers.Resource.Load<Sprite>(data.IconImageKey);

        foreach(var currency in buddySKillUpCurrencies)
        {
            currency.gameObject.SetActive(false);
        }

        for(int i = 0; i < data.LevelUpCurrencies.Count; i++)
        {
           // Debug.Log($"{data.LevelUpCurrencies[i].currencyType} : {data.LevelUpCurrencies[i].count}");
            
            if (data.LevelUpCurrencies[i].currencyType == Define.ECurrencyType.None)
                continue;

            buddySKillUpCurrencies[i].gameObject.SetActive(true);
            buddySKillUpCurrencies[i].SetInfo(data.LevelUpCurrencies[i].currencyType, data.LevelUpCurrencies[i].count, false);
        }

        if(data.LevelUpCurrencies.Count > 0)
        {
            GetButton((int)Buttons.Button_SkillUp).gameObject.SetActive(true);
        }
    }

    private void OnClickedBuddySkillUpButton(PointerEventData eventData)
    {
        if (GetButton((int)Buttons.Button_SkillUp).interactable == false)
            return;

        Managers.Game.BuddySkillUp(templateId).Forget();
    }
}
