using Data;
using UnityEngine;


// TODO 스킬별로 상속 관계 다루기
public class BuddySkill : Skill
{
    public BuddySkillData skillData;

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

    }
    #endregion
}
