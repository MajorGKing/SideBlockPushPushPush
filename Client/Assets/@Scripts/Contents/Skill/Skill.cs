using Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Skill
{
    public SkillData SkillData;
    protected CreatureController _owner;
    //public Skill(CreatureController owner, SkillData skillData)
    //{
    //    _owner = owner;
    //    SkillData = skillData;
    //}

    public abstract void UseSkill();

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

    public virtual void Reset()
    {
        _owner = null;
    }
}
