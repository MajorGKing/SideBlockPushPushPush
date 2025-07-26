using UnityEngine;

public class UI_BuddySKillSubItem : UI_SubItem
{
    enum Images
    {
        Image_BuddySKill,
    }

    enum Texts
    {
        Text_BuddySKillInfo,
    }

    private int templateId;

    protected override void Awake()
    {
        base.Awake();

        BindImages(typeof(Images));
        BindTexts(typeof(Texts));

        RefreshUI();
    }

    public void SetInfo(int buddySkillTemplateId)
    {
        templateId = buddySkillTemplateId;

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (templateId == 0)
            return;

        var buddySkillData = Managers.Data.BuddySkillDataDic[templateId];
        GetImage((int)Images.Image_BuddySKill).sprite = Managers.Resource.Load<Sprite>(buddySkillData.IconImageKey);

        string text = "";
        text = Managers.GetText(buddySkillData.NameTextId);
        text += $"\tLevel : {buddySkillData.SkillLevel}";
        text += $"\n{Managers.GetText(buddySkillData.DescriptionTextId)}";
        text += $"\n{Managers.GetText(Define.DESCRIPTIONATTACKPERCENT)} : {Managers.Data.EffectDataDic[buddySkillData.EffectDataId].DamageValue}";
        text += $"\t{Managers.GetText(Define.DESCRIPTIONATTACKSPEED)} : {buddySkillData.AnimSpeed}";
        text += $"\t{Managers.GetText(Define.DESCRIPTIONCOOLTIME)} : {buddySkillData.Cooltime}";

        GetText((int)Texts.Text_BuddySKillInfo).text = text ;
    }
}
