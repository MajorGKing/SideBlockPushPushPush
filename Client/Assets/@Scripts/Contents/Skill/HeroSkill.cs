using Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeroSkill : Skill
{
    public HeroSkillData skillData;

    public HeroSkill(HeroController owner, int templateId)
    {
        SetInfo(owner, templateId);
    }

    private void SetInfo(HeroController owner, int templateId)
    {
        _owner = owner;
        skillData = Managers.Data.HeroSkillDataDic[templateId];
    }

    public override void Reset()
    {
        base.Reset();
        skillData = null;
    }

    public override void UseSkill()
    {
        // TODO Owner의 데이터가 필요

        // 데미지를 계산한다
        int damageBase = 10;
        int effectNumber = skillData.EffectDataId;
        float damage = damageBase * Managers.Data.EffectDataDic[effectNumber].DamageValue;
        int roundedDamage = (int)(damage + 0.5f);

        foreach (MonsterController target in SkillTargetList)
        {
            target.OnDamage(_owner.GameObjectType, skillData, roundedDamage);
        }
    }
}
