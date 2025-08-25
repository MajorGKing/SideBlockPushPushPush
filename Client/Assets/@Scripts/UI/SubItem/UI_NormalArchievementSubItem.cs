using UnityEngine.EventSystems;

public class UI_NormalArchievementSubItem : UI_SubItem
{
    private enum GameObjects
    {
        UI_RewardSubItem,
    }
    private enum Texts
    {
        Text_MissonTitle,
        Text_NormalMissionCount,
    }

    private enum Buttons
    {
        Button_Take,
    }

    private enum Sliders
    {
        Slider_NormalMission,
    }

    private AchievementSaveData achievmentSaveData;
    private Data.AchievementData achievementData;

    private UI_RewardsSubItem rewardsSubItem;

    protected override void Awake()
    {
        base.Awake();

        BindGameObjects(typeof(GameObjects));

        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));
        BindSliders(typeof(Sliders));

        GetButton((int)Buttons.Button_Take).gameObject.BindEvent(OnTakeButtonClick);

        rewardsSubItem = GetGameObject((int)GameObjects.UI_RewardSubItem).GetComponent<UI_RewardsSubItem>();

        RefreshUI();
    }

    public void SetInfo(int templateId)
    {
        // 미션 데이터를 받는다
        achievementData = Managers.Data.AchievementDataDic[templateId];

        // 미션 진행 데이터를 받는다
        achievmentSaveData = Managers.Game.GetAchievmentSaveData(templateId);

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (achievementData == null)
            return;

        if (achievmentSaveData == null)
            return;

        rewardsSubItem.SetInfo(achievementData.RewardType, achievementData.RewardCount);

        int stackPoint = Managers.Game.GetAcievemntValue(achievmentSaveData.TemplateId);

        GetText((int)Texts.Text_MissonTitle).text = $"{Managers.GetText(achievementData.NameTextId)}";
        GetText((int)Texts.Text_NormalMissionCount).text = $"{stackPoint:N0}/{achievementData.MissionCount:N0}";
        GetSlider((int)Sliders.Slider_NormalMission).value = (float)stackPoint / achievementData.MissionCount;
        GetButton((int)Buttons.Button_Take).interactable = achievmentSaveData.CheckRewardAble();

        if (achievementData.MissionGoal == Define.EMissionGoal.StageClearAt)
        {
            GetText((int)Texts.Text_NormalMissionCount).text = $"{stackPoint:N0}/{1:N0}";
            GetSlider((int)Sliders.Slider_NormalMission).value = (float)stackPoint / 1;
            GetButton((int)Buttons.Button_Take).interactable = achievmentSaveData.CheckRewardAble();
        }
    }

    private void OnTakeButtonClick(PointerEventData data)
    {
        if (GetButton((int)Buttons.Button_Take).interactable)
        {
            Managers.Game.GetAchievmentReward(achievementData.TemplateId);
        }
    }
}
