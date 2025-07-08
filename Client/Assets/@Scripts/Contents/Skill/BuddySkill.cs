using Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// TODO ��ų���� ��� ���� �ٷ��
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
        // TODO Owner�� �����Ͱ� �ʿ�

        // �������� ����Ѵ�
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
