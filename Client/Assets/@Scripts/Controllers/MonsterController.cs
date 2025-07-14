using Data;
using Spine.Unity;
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

    private class CurrentMonsterData : MonsterData
    {
        public int currentHp;
        public int currentNormalDefence;
        public int currentMagicDefence;

        public CurrentMonsterData(MonsterData data)
        {
            TemplateId = data.TemplateId;
            Name = data.Name;
            NameTextId = data.NameTextId;
            DescriptionTextId = data.DescriptionTextId;
            StageInfoImageKey = data.SpineNameKey;
            SpineNameKey = data.SpineNameKey;
            MaxHp = data.MaxHp;
            NormalDefence = data.NormalDefence;
            MagicDefence = data.MagicDefence;
            ProgressionTypeId = data.ProgressionTypeId;

            currentHp = data.MaxHp;
            currentNormalDefence = data.NormalDefence;
            currentMagicDefence = data.MagicDefence;
        }
    }


    private MonsterData _monsterData;
    private CurrentMonsterData _currentMonsterData;

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

    public void SetInfo(int templateID, int level)
    {
        MonsterData data = Managers.Data.MonsterDataDic[templateID];
        ProgressionTypeData type = Managers.Data.ProgressionTypeDataDic[data.ProgressionTypeId];

        _monsterData = data;

        _monsterData.MaxHp = data.MaxHp + (type.MaxHp * (level - 1));
        _monsterData.NormalDefence = data.NormalDefence + (type.NormalDefence * (level - 1));
        _monsterData.MagicDefence = data.MagicDefence + (type.MagicDefence * (level - 1));

        _currentMonsterData = new CurrentMonsterData(_monsterData);

        skeletonAnimation.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(Managers.Data.MonsterDataDic[templateID].SpineNameKey);
        skeletonAnimation.Initialize(true);

        AnimationBindEventInit();

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

        _currentMonsterData.currentHp -= damage;
        if (_currentMonsterData.currentHp <= 0)
        {
            _currentMonsterData.currentHp = 0;
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
        _battleBarUI.SetInfo(_currentMonsterData.currentHp, _currentMonsterData.MaxHp);
    }
}
