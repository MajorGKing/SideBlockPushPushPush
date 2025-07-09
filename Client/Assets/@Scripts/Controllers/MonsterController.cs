using Data;
using UnityEngine;

public class MonsterController : CreatureController
{
    public enum EMonsterState
    {
        None,
        Idle,
        Attack,
        Reload,
        Dead,
    }

    public int MaxHP { get; protected set; }
    public int HP { get; protected set; }

    //private GameScene _gameScene;
    private UI_BattleBarWorldSpace _battleBarUI;

    private EMonsterState _currentState;
    public EMonsterState currentState
    {
        get { return _currentState; }
        set { _currentState = value; }
    }

    public bool IsAlive
    {
        get { return currentState != EMonsterState.Dead; }
    }


    protected override void Init()
    {
        base.Init();

        _battleBarUI = GetComponentInChildren<UI_BattleBarWorldSpace>();

        currentState = EMonsterState.Idle;
        PlayAnimation(0, ANIMATION_IDLE, true);

        GameObjectType = Define.EGameObjectType.Monster;
    }

    public void SetInfo(int templateID)
    {
        //_gameScene = gameScene;

        MaxHP = 1000;
        HP = MaxHP;

        UpdateHpText();
    }

    public void OnDamage(Define.EGameObjectType type, SkillData skillData, int damage)
    {
        TakeDamage(damage);

        if (type == Define.EGameObjectType.Hero)
        {
            var heroSKill = skillData as HeroSkillData;

            Managers.Object.SpawnSkillEffect(transform.position + Vector3.up, heroSKill.HitEffectPrefabKey, 1.0f);
            UI_DamageText damageText = Managers.UI.MakeSubItem<UI_DamageText>(transform, "UI_CriticalDamageText");
            Managers.Sound.Play(Define.ESound.Effect, heroSKill.HitSoundKey);
            damageText.SetInfo(damage);
        }
        else if (type == Define.EGameObjectType.Buddy)
        {
            var buddySKill = skillData as BuddySkillData;

            Managers.Object.SpawnSkillEffect(transform.position + Vector3.up, buddySKill.HitEffectPrefabKey, 1.0f);
            UI_DamageText damageText = Managers.UI.MakeSubItem<UI_DamageText>(transform, "UI_DamageText");
            Managers.Sound.Play(Define.ESound.Effect, buddySKill.HitSoundKey);
            damageText.SetInfo(damage);
        }
    }

    private void TakeDamage(int damage)
    {
        if (currentState == EMonsterState.Dead)
            return;

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
        currentState = EMonsterState.Dead;
        PlayAnimation(0, ANIMATION_DIE, false);
    }

    protected void UpdateHpText()
    {
        _battleBarUI.SetInfo(HP, MaxHP);
    }
}
