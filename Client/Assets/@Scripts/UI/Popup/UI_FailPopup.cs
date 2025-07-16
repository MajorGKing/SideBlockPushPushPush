using System.Collections.Generic;
using UnityEngine;

public class UI_FailPopup : UI_Popup
{
    private enum GameObjects
    {
        
    }

    private enum Buttons
    {
        Button_Config,
    }

    //private List<UI_RewardsSubItem> _slotList = new List<UI_RewardsSubItem>();

    //private List<Reward> _rewardList = new List<Reward>();

    //private Define.ERewardType _type;

    protected override void Awake()
    {
        base.Awake();

        // Bind
        //BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));

        //// Init
        //foreach (Transform child in GetObject((int)GameObjects.Content_Reward).transform)
        //{
        //    UI_RewardsSubItem slot = child.GetComponent<UI_RewardsSubItem>();
        //    _slotList.Add(slot);
        //}

        // Bind Event
        GetButton((int)Buttons.Button_Config).onClick.AddListener(OnClickConfig);
    }

    //public void SetInfo(Define.ERewardType type, List<Reward> rewardList)
    //{
    //    _type = type;
    //    _rewardList = rewardList;

    //    RefreshUI();
    //}

    //private void RefreshUI()
    //{
    //    for (int index = 0; index < _slotList.Count; index++)
    //    {
    //        if (index < _rewardList.Count)
    //        {
    //            _slotList[index].SetInfo(_rewardList[index].CurrencyType, _rewardList[index].RewardAmount, _rewardList[index].IsFirst);
    //            _slotList[index].gameObject.SetActive(true);
    //        }
    //        else
    //        {
    //            _slotList[index].gameObject.SetActive(false);
    //        }
    //    }
    //}

    #region UI Event

    private void OnClickConfig()
    {
        ClosePopupUI();
        Managers.Scene.LoadScene(Define.EScene.LobbyScene);
    }

    #endregion
}
