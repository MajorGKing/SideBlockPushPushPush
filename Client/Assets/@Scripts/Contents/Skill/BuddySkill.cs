using Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// TODO 스킬별로 상속 관계 다루기
public class BuddySkill : Skill
{
    public BuddySkillData skillData;

    public List<CreatureController> SkillTargetList
    {
        get
        {
            List<CreatureController> targetList = new List<CreatureController>();

            //if (_owner.GameObjectType == Define.EGameObjectType.Hero || _owner.GameObjectType == Define.EGameObjectType.Buddy)
            {
                var target = Managers.Object.LivingMonsterList.FirstOrDefault();
                if (target != null)
                {
                    targetList.Add(target);
                }
            }

            return targetList;
        }

        set
        {

        }
    }

    //public BuddySkill(BuddyController owner, BuddySkillData skillInfo) : base(owner,  skillInfo)

    public BuddySkill(BuddyController owner, BuddySkillData skillInfo)
    {
        SetInfo(owner, skillInfo);
    }

    private void SetInfo(BuddyController owner, BuddySkillData skillInfo)
    {
        _owner = owner;
        skillData = skillInfo;
    }

    public void Reset()
    {
        _owner = null;
        skillData = null;
    }

    #region Battle
    public void UseSkill()
    {
        // TODO Owner의 데이터가 필요

        // 데미지를 계산한다
        int damageBase = 10;
        int effectNumber = skillData.EffectDataId;
        float damage = damageBase * Managers.Data.EffectDataDic[effectNumber].DamageValue;
        int roundedDamage = (int)(damage + 0.5f);

        foreach(MonsterController target in SkillTargetList)
        {
            target.OnDamage(_owner.GameObjectType, roundedDamage);
        }
    }
    #endregion
}
