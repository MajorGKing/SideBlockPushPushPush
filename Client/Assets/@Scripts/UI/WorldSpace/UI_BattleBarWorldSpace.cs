using UnityEngine;

public class UI_BattleBarWorldSpace : UI_WorldSpace
{
    private enum Texts
    {
        Text_Hp,
    }

    private enum Sliders
    {
        Slider_HP,
    }

    private int _hp;
    private int _maxHP;

    protected override void Awake()
    {
        base.Awake();

        // Bind
        BindTexts(typeof(Texts));
        BindSliders(typeof(Sliders));
    }

    public void SetInfo(int hp, int maxHP)
    {
        _hp = hp;
        _maxHP = maxHP;

        RefreshUI();
    }

    public void RefreshUI()
    {
        GetSlider((int)Sliders.Slider_HP).value = (float)_hp / _maxHP;
        GetText((int)Texts.Text_Hp).text = $"{_hp:N0}";
    }
}
