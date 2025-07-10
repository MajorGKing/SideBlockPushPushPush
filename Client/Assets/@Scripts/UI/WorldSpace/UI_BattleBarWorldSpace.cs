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
        Slider_Skill_Guage,
    }

    private int _hp;
    private int _maxHP;

    private float _coolTime;
    private float _maxCoolTime;

    protected override void Awake()
    {
        base.Awake();

        // Bind
        BindTexts(typeof(Texts));
        BindSliders(typeof(Sliders));
    }

    public void SetInfo(int hp, int maxHP)
    {
        GetSlider((int)Sliders.Slider_Skill_Guage).gameObject.SetActive(false);

        _hp = hp;
        _maxHP = maxHP;

        RefreshUI();
    }

    public void SetInfo(float coolTime, float maxCoolTime)
    {
        GetSlider((int)Sliders.Slider_HP).gameObject.SetActive(false);

        _coolTime = coolTime;
        _maxCoolTime = maxCoolTime;

        RefreshUI();
    }

    public void RefreshUI()
    {
        GetSlider((int)Sliders.Slider_HP).value = (float)_hp / _maxHP;
        GetText((int)Texts.Text_Hp).text = $"{_hp:N0}";

        GetSlider((int)Sliders.Slider_Skill_Guage).value = _coolTime / _maxCoolTime;
    }
}
