

using UnityEngine;

public class UI_RewardsSubItem : UI_SubItem
{
	private enum Images
	{
		Image_Icon,
	}

	private enum Texts
	{
		Text_ItemCount,
        Text_ItemFirstTime,
    }

	// TODO Data
	private Define.ECurrencyType _currencyType;
	private int _currencyAmount;
	private bool _isFirst;
	private bool _isShow;

	protected override void Awake()
	{
		base.Awake();

        // Bind
        BindImages(typeof(Images));
        BindTexts(typeof(Texts));

        GetText((int)Texts.Text_ItemFirstTime).gameObject.SetActive(false);

		RefreshUI();
    }

	public void SetInfo(Define.ECurrencyType currency, int currencyAmount, bool isFirst = false, bool isShow = true)
	{
        _currencyType = currency;
        _currencyAmount = currencyAmount;
        _isFirst = isFirst;
        _isShow = isShow;

        if (isInit == false)
            return;

        RefreshUI();
	}

	private void RefreshUI()
	{
		if (_currencyType == Define.ECurrencyType.None)
			return;

		GetImage((int)Images.Image_Icon).sprite = Managers.Resource.Load<Sprite>(Managers.Data.CurrencyTypeDataDic[_currencyType].IconImage);
		GetText((int)Texts.Text_ItemCount).text = $"{_currencyAmount:N0}";

        if (_isShow == false)
        {
			GetImage((int)Images.Image_Icon).sprite = Managers.Resource.Load<Sprite>(Define.CHECKICON);
            GetText((int)Texts.Text_ItemCount).text = "";
        }

        GetText((int)Texts.Text_ItemFirstTime).gameObject.SetActive(_isFirst);

		
	}
}
