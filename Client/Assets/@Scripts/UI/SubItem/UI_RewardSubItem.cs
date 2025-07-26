

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

	protected override void Awake()
	{
		base.Awake();

        // Bind
        BindImages(typeof(Images));
        BindTexts(typeof(Texts));

        GetText((int)Texts.Text_ItemFirstTime).gameObject.SetActive(false);

		RefreshUI();
    }

	public void SetInfo(Define.ECurrencyType currency, int currencyAmount, bool isFirst)
	{
        _currencyType = currency;
        _currencyAmount = currencyAmount;
        _isFirst = isFirst;

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

		GetText((int)Texts.Text_ItemFirstTime).gameObject.SetActive(_isFirst);
	}
}
