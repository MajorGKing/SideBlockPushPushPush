using Data;
using System.Collections.Generic;
using UnityEngine;

//public class Reward
//{
//    public Define.ECurrencyType CurrencyType { get; private set; }
//    public int RewardAmount { get; private set; }
//    public bool IsFirst { get; private set; }

//    public Reward(Define.ECurrencyType rewardType, int rewardAmount, bool isFirst = false)
//    {
//        CurrencyType = rewardType;
//        RewardAmount = rewardAmount;
//        IsFirst = isFirst;
//    }
//}

public class BuddyGacha
{
    public string buddyName;
    public bool duplication;

    public BuddyGacha() { }
    public BuddyGacha(string buddyName, bool duplication)
    {
        this.buddyName = buddyName;
        this.duplication = duplication;
    }
}

public class UI_BuddyGachaPopup : UI_Popup
{
    private enum GameObjects
    {
        Content_Gacha,
    }

    private enum Buttons
    {
        Button_Config,
    }

    private List<UI_GachaSubItem> _slotList = new List<UI_GachaSubItem>();

    private List<BuddyGacha> _gachaList = new List<BuddyGacha>();

    //private Define.ERewardType _type;

    protected override void Awake()
    {
        base.Awake();

        // Bind
        BindGameObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));

        // Init
        foreach (Transform child in GetGameObject((int)GameObjects.Content_Gacha).transform)
        {
            var slot = child.GetComponent<UI_GachaSubItem>();
            _slotList.Add(slot);
        }

        // Bind Event
        GetButton((int)Buttons.Button_Config).onClick.AddListener(OnClickConfig);
    }

    public void SetInfo(List<BuddyGacha> gachaList)
    {
        _gachaList = gachaList;

        RefreshUI();
    }

    private void RefreshUI()
    {
        for (int index = 0; index < _slotList.Count; index++)
        {
            if (index < _gachaList.Count)
            {
                _slotList[index].gameObject.SetActive(true);
                _slotList[index].SetInfo(_gachaList[index].buddyName, _gachaList[index].duplication);
            }
            else
            {
                _slotList[index].gameObject.SetActive(false);
            }
        }
    }

    #region UI Event

    private void OnClickConfig()
    {
        ClosePopupUI();

        //if(_type == Define.ERewardType.StageClear)
        //{
        //    Managers.Scene.LoadScene(Define.EScene.LobbyScene);
        //}
    }

    #endregion
}
