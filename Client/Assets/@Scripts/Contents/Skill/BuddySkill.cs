using Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// TODO 스킬별로 상속 관계 다루기
public class BuddySkill : Skill
{
    public BuddySkillData skillData;

    //public BuddySkill(BuddyController owner, BuddySkillData skillInfo) : base(owner,  skillInfo)

    public BuddySkill(BuddyController owner, int templateId)
    {
        SetInfo(owner, templateId);
    }

    private void SetInfo(BuddyController owner, int templateId)
    {
        _owner = owner;
        skillData = Managers.Data.BuddySkillDataDic[templateId];
    }

    public override void Reset()
    {
        base.Reset();
        skillData = null;
    }

    #region Battle
    public override void UseSkill()
    {
        // TODO Owner의 데이터가 필요

        // 데미지를 계산한다
        int damageBase = 10;
        int effectNumber = skillData.EffectDataId;
        float damage = damageBase * Managers.Data.EffectDataDic[effectNumber].DamageValue;
        int roundedDamage = (int)(damage + 0.5f);

        foreach(MonsterController target in SkillTargetList)
        {
            target.OnDamage(_owner.GameObjectType, skillData, roundedDamage);
        }

        // Hero에 던져주기
        Managers.Object.Hero.AddBlock(skillData);
    }
    #endregion
}
