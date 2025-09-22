using Data;
using WebPacket;

public class HeroSkill : Skill
{
    public HeroSkillData skillData;
    private float _damage;
    private int _roundedDamage;

    public HeroSkill(HeroController owner, SkillSnapshot skill)
    {
        SetInfo(owner, skill);
    }

    private void SetInfo(HeroController owner, SkillSnapshot skill)
    {
        _owner = owner;
        skillData = Managers.Data.HeroSkillDataDic[skill.TemplateId];

        {
            skillData.TemplateId = skill.TemplateId;
            skillData.SkillLevel = skill.SkillLevel;
            skillData.SkillType = skill.SkillType;
            skillData.AnimSpeed = skill.AnimSpeed;
            skillData.UseSkillTargetType = skill.UseSkillTargetType;
            skillData.GatherTargetCounts = skill.GatherTargetCounts;
            skillData.GatherTargetType = skill.GatherTargetType;
            skillData.TargetFriendType = skill.TargetFriendType;
            skillData.IconImageKeys = skill.IconImageKeys;
        }

        // TODO Owner의 데이터가 필요

        // 데미지를 계산한다
        // Effect의 값을 Data에서 가저오지 않고 Web에서 가저온 값으로 대처
        // TODO Effect관련 새로운 기능이 추가되면 여기서 구현 필요
        int damageBase = 10;
        _damage = damageBase * skill.Effect.DamageValue;
        _roundedDamage = (int)(_damage + 0.5f);
    }

    public override void Reset()
    {
        base.Reset();
        skillData = null;
        _damage = 0f;
    }

    public override void UseSkill()
    {
        // TODO Owner의 데이터가 필요

        // 데미지를 계산한다
        //int damageBase = 10;
        //int effectNumber = skillData.EffectDataId;
        //float damage = damageBase * Managers.Data.EffectDataDic[effectNumber].DamageValue;
        //int roundedDamage = (int)(damage + 0.5f);

        foreach (MonsterController target in SkillTargetList)
        {
            target.OnDamage(_owner.GameObjectType, skillData, _roundedDamage);
        }
    }
}
