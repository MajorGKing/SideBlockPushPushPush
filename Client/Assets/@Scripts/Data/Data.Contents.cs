using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ExcludeFieldAttribute : Attribute
    {
    }
    #region CreatureData
    [Serializable]
    public class CreatureData
    {
        public int TemplateId;
        public string NameTextID;
        public float ColliderOffsetX;
        public float ColliderOffsetY;
        public float ColliderRadius;
        public float MaxHp;
        public float UpMaxHpBonus;
        public float Atk;
        public float MissChance;
        public float AtkBonus;
        public float MoveSpeed;
        public float CriRate;
        public float CriDamage;
        public string IconImage;
        public string SkeletonDataID;
        public int DefaultSkillId;
        public int EnvSkillId;
        public int SkillAId;
        public int SkillBId;
       
    }

    [Serializable]
    public class CreatureDataLoader : ILoader<int, CreatureData>
    {
        public List<CreatureData> creatures = new List<CreatureData>();
        public Dictionary<int, CreatureData> MakeDict()
        {
            Dictionary<int, CreatureData> dict = new Dictionary<int, CreatureData>();
            foreach (CreatureData creature in creatures)
                dict.Add(creature.TemplateId, creature);
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    public class SkillData
    {

    }

    #region BuddySkillData
    [Serializable]
    public class BuddySkillData : SkillData
    {
        public int TemplateId;
        public string Name;
        public string NameTextId;
        public string DescriptionTextId;
        public string IconImageKey;
        public Define.ESkillType SkillType;
        public string SkillEffectPrefabKey;
        public string HitEffectPrefabKey;
        public string StartSoundKey;
        public string HitSoundKey;
        public float Cooltime;
        public string AnimName;
        public float AnimSpeed;
        public Define.EUseSkillTargetType UseSkillTargetType;
        public int GatherTargetCounts;
        public int GatherTargetType;
        public Define.ETargetFriendType TargetFriendType;
        public int EffectDataId;
        public int NextLevelSkillId;
    }

    public class BuddySkillDataLoader : ILoader<int, BuddySkillData>
    {
        public List<BuddySkillData> buddySkills = new List<BuddySkillData>();
        public Dictionary<int, BuddySkillData> MakeDict()
        {
            Dictionary<int, BuddySkillData> dict = new Dictionary<int, BuddySkillData>();
            foreach (BuddySkillData skill in buddySkills)
                dict.Add(skill.TemplateId, skill);
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region EffectData
    [Serializable]
    public class EffectData
    {
        public int TemplateId;
        public string Name;
        public string NameText;
        public string DescriptionText;
        public string IconImage;
        public Define.EEffectType EffectType;
        public Define.EDurationPolicy EDurationPolicy;
        public float Duration;
        public float DamageValue;
        public int StatType;
        public float AddValue;
        public int LifeStealValue;
        public int StunValue;
    }

    public class EffectDataLoader : ILoader<int, EffectData>
    {
        public List<EffectData> effects = new List<EffectData>();

        public Dictionary<int, EffectData> MakeDict()
        {
            Dictionary<int, EffectData> dict = new Dictionary<int, EffectData>();
            foreach (EffectData effect in effects)
                dict.Add(effect.TemplateId, effect);
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }

    #endregion

}