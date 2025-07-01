using UnityEngine;

public class MonsterController : CreatureController
{
    public int MaxHP { get; protected set; }
    public int HP { get; protected set; }

    private GameScene _gameScene;
    private UI_BattleBarWorldSpace _battleBarUI;


    protected override void Init()
    {
        base.Init();

        _battleBarUI = GetComponentInChildren<UI_BattleBarWorldSpace>();
    }

    public void SetInfo(GameScene gameScene)
    {
        _gameScene = gameScene;

        MaxHP = 1000;
        HP = MaxHP;

        UpdateHpText();
    }

    public void OnDamage(int type, int damage)
    {
        TakeDamage(damage);

        if (type == 0)
        {
            Managers.Object.SpawnSkillEffect(transform.position + Vector3.up, "VFX_buddy_common_skill_hit", 1.0f);
            UI_DamageText damageText = Managers.UI.MakeSubItem<UI_DamageText>(transform, "UI_DamageText");
            damageText.SetInfo(damage);
        }
        else if(type == 1)
        {
            Managers.Object.SpawnSkillEffect(transform.position + Vector3.up, "VFX_hero_skill_common_attack_hit", 1.0f);
            UI_DamageText damageText = Managers.UI.MakeSubItem<UI_DamageText>(transform, "UI_DamageText");
            damageText.SetInfo(damage);
        }
    }

    private void TakeDamage(int damage)
    {
        HP -= damage;
        if (HP <= 0)
        {
            HP = 0;
            OnDead();
        }

        UpdateHpText();
    }

    private void OnDead()
    {
        PlayAnimation(0, "dead", true);
    }

    protected void UpdateHpText()
    {
        _battleBarUI.SetInfo(HP, MaxHP);
    }
}
