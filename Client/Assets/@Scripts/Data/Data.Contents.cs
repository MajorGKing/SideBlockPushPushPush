using System;
using System.Collections.Generic;

namespace Data
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ExcludeFieldAttribute : Attribute
    {
    }

    #region TextData
    public class TextData
    {
        public string TemplateId;
        public string KOR;
    }

    [Serializable]
    public class TextDataLoader : ILoader<string, TextData>
    {
        public List<TextData> texts = new List<TextData>();

        public Dictionary<string, TextData> MakeDict()
        {
            Dictionary<string, TextData> dict = new Dictionary<string, TextData>();
            foreach (TextData text in texts)
                dict.Add(text.TemplateId, text);

            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

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

    #region StageData
    [Serializable]
    public class StageData
    {
        public int TemplateId;
        public int WorldNumber;
        public int StageNumber;
        public Define.EDifficultyLevel DifficultyLevel;
        public List<int> FirstWaveMonsterList;
        public List<int> FirstWaveMonsterLevelList;
        public List<int> SecondWaveMonsterList;
        public List<int> SecondWaveMonsterLevelList;
        public List<int> BossWaveMonsterList;
        public List<int> BossWaveMonsterLevelList;
        public int RewardTimes;
        public List<Define.ECurrencyType> RewardType;
        public List<int> RewardCount;
        public List<int> RewardPercent;
        public List<Define.ECurrencyType> RewardFirstType;
        public List<int> RewardFirstCount;
        public int PreviewStageId;
        public int NextaStageId;
        public int OtherStageId;
    }

    public class StageDataLoader : ILoader<int, StageData>
    {
        public List<StageData> stages = new List<StageData>();

        public Dictionary<int, StageData> MakeDict()
        {
            Dictionary<int, StageData> dict = new Dictionary<int, StageData>();
            foreach (StageData stage in stages)
                dict.Add(stage.TemplateId, stage);
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region MonsterData
    public class MonsterData
    {
        public int TemplateId;
        public string Name;
        public string NameTextId;
        public string DescriptionTextId;
        public string StageInfoImageKey;
        public string SpineNameKey;
        public int MaxHp;
        public int NormalDefence;
        public int MagicDefence;
        public int ProgressionTypeId;
    }

    public class MonsterDataLoader : ILoader<int, MonsterData>
    {
        public List<MonsterData> Monsters = new List<MonsterData>();

        public Dictionary<int, MonsterData> MakeDict()
        {
            Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();
            foreach (MonsterData monster in Monsters)
                dict.Add(monster.TemplateId, monster);
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region ProgressionTypeData
    public class ProgressionTypeData
    {
        public int TemplateId;
        public string Name;
        public int MaxHp;
        public int NormalDefence;
        public int MagicDefence;
    }

    public class ProgressionTypeDataLoader : ILoader<int, ProgressionTypeData>
    {
        public List<ProgressionTypeData> ProgressionTypes = new List<ProgressionTypeData>();

        public Dictionary<int, ProgressionTypeData> MakeDict()
        {
            Dictionary<int, ProgressionTypeData> dict = new Dictionary<int, ProgressionTypeData>();
            foreach (ProgressionTypeData ProgressionType in ProgressionTypes)
                dict.Add(ProgressionType.TemplateId, ProgressionType);
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

    #region HeroSkillData
    [Serializable]
    public class HeroSkillData : SkillData
    {
        public int TemplateId;
        public string Name;
        public string NameTextId;
        public string DescriptionTextId;
        public int SkillLevel;
        public string IconImageKey1;
        public string IconImageKey2;
        public string IconImageKey3;
        public string IconImageKey4;
        public string IconImageKey5;
        public string IconImageKey6;
        public Define.ESkillType SkillType;
        public string SkillEffectPrefabKey;
        public string HitEffectPrefabKey;
        public string StartSoundKey;
        public string HitSoundKey;
        public string AnimName;
        public float AnimSpeed;
        public Define.EUseSkillTargetType UseSkillTargetType;
        public int GatherTargetCounts;
        public int GatherTargetType;
        public Define.ETargetFriendType TargetFriendType;
        public int EffectDataId;
        public Define.ECurrencyType LevelUpCurrency1;
        public int LevelUpCurrency1Count;
        public Define.ECurrencyType LevelUpCurrency2;
        public int LevelUpCurrency2Count;
        public Define.ECurrencyType LevelUpCurrency3;
        public int LevelUpCurrency3Count;
        public int OriginalLevelId;
        public int PreviewLevelId;
        public int NextLevelId;

        [ExcludeFieldAttribute]
        public List<string> IconImageKeys;

        [ExcludeFieldAttribute]
        public List<LevelUpCurrency> LevelUpCurrencies;
    }

    public class HeroSkillDataLoader : ILoader<int, HeroSkillData>
    {
        public List<HeroSkillData> heroSkills = new List<HeroSkillData>();
        public Dictionary<int, HeroSkillData> MakeDict()
        {
            Dictionary<int, HeroSkillData> dict = new Dictionary<int, HeroSkillData>();
            foreach (HeroSkillData skill in heroSkills)
                dict.Add(skill.TemplateId, skill);
            return dict;
        }

        public bool Validate()
        {
            foreach(var skill in heroSkills)
            {
                skill.IconImageKeys = new List<string>();

                if(string.IsNullOrEmpty(skill.IconImageKey1) == false)
                    skill.IconImageKeys.Add(skill.IconImageKey1);
                if (string.IsNullOrEmpty(skill.IconImageKey2) == false)
                    skill.IconImageKeys.Add(skill.IconImageKey2);
                if (string.IsNullOrEmpty(skill.IconImageKey3) == false)
                    skill.IconImageKeys.Add(skill.IconImageKey3);
                if (string.IsNullOrEmpty(skill.IconImageKey4) == false)
                    skill.IconImageKeys.Add(skill.IconImageKey4);
                if (string.IsNullOrEmpty(skill.IconImageKey5) == false)
                    skill.IconImageKeys.Add(skill.IconImageKey5);
                if (string.IsNullOrEmpty(skill.IconImageKey6) == false)
                    skill.IconImageKeys.Add(skill.IconImageKey6);

                skill.LevelUpCurrencies = new List<LevelUpCurrency>();
                if (skill.LevelUpCurrency1 != Define.ECurrencyType.None)
                {
                    skill.LevelUpCurrencies.Add(new LevelUpCurrency(skill.LevelUpCurrency1, skill.LevelUpCurrency1Count));
                }
                if (skill.LevelUpCurrency2 != Define.ECurrencyType.None)
                {
                    skill.LevelUpCurrencies.Add(new LevelUpCurrency(skill.LevelUpCurrency2, skill.LevelUpCurrency2Count));
                }
                if (skill.LevelUpCurrency3 != Define.ECurrencyType.None)
                {
                    skill.LevelUpCurrencies.Add(new LevelUpCurrency(skill.LevelUpCurrency3, skill.LevelUpCurrency3Count));
                }
            }

            return true;
        }
    }
    #endregion

    #region HeroData
    [Serializable]
    public class HeroData
    {
        public int TemplateId;
        public string Name;
        public string NameTextId;
        public string DescriptionTextId;
        public string SpineNameKey;
        public int Level;
        public int Attack;
        public int MagicAttack;
        public int Skill1Id;
        public int Skill2Id;
        public int Skill3Id;
        public int Skill4Id;
        public int Skill5Id;
        public Define.ECurrencyType LevelUpCurrency1;
        public int LevelUpCurrency1Count;
        public int OriginalLevelId;
        public int PreviewLevelId;
        public int NextLevelId;

        [ExcludeFieldAttribute]
        public List<int> SKillIds;

        [ExcludeFieldAttribute]
        public List<LevelUpCurrency> LevelUpCurrencies;
    }

    public class HeroDataLoader : ILoader<int, HeroData>
    {
        public List<HeroData> heroes = new List<HeroData>();
        public Dictionary<int, HeroData> MakeDict()
        {
            Dictionary<int, HeroData> dict = new Dictionary<int, HeroData>();
            foreach (HeroData hero in heroes)
                dict.Add(hero.TemplateId, hero);
            return dict;
        }

        public bool Validate()
        {
            foreach (HeroData hero in heroes)
            {
                hero.SKillIds = new List<int>();
                if (hero.Skill1Id != 0)
                    hero.SKillIds.Add(hero.Skill1Id);
                if (hero.Skill2Id != 0)
                    hero.SKillIds.Add(hero.Skill2Id);
                if (hero.Skill3Id != 0)
                    hero.SKillIds.Add(hero.Skill3Id);
                if (hero.Skill4Id != 0)
                    hero.SKillIds.Add(hero.Skill4Id);
                if (hero.Skill5Id != 0)
                    hero.SKillIds.Add(hero.Skill5Id);

                hero.LevelUpCurrencies = new List<LevelUpCurrency>();
                if (hero.LevelUpCurrency1 != Define.ECurrencyType.None)
                {
                    hero.LevelUpCurrencies.Add(new LevelUpCurrency(hero.LevelUpCurrency1, hero.LevelUpCurrency1Count));
                }
                //if (hero.LevelUpCurrency2 != Define.ECurrencyType.None)
                //{
                //    hero.LevelUpCurrencies.Add(new LevelUpCurrency(hero.LevelUpCurrency2, hero.LevelUpCurrency2Count));
                //}
                //if (hero.LevelUpCurrency3 != Define.ECurrencyType.None)
                //{
                //    hero.LevelUpCurrencies.Add(new LevelUpCurrency(hero.LevelUpCurrency3, hero.LevelUpCurrency3Count));
                //}
                //if (hero.LevelUpCurrency4 != Define.ECurrencyType.None)
                //{
                //    hero.LevelUpCurrencies.Add(new LevelUpCurrency(hero.LevelUpCurrency4, hero.LevelUpCurrency4Count));
                //}
            }

            return true;
        }
    }

    #endregion

    #region Buddy
    [Serializable]
    public class BuddyData
    {
        public int TemplateId;
        public string Name;
        public string NameTextId;
        public string DescriptionTextId;
        public string SpineNameKey;
        public int Level;
        public int Attack;
        public int MagicAttack;
        public float Reload;
        public int Skill1Id;
        public int Skill2Id;
        public int Skill3Id;
        public int Skill4Id;
        public int Skill5Id;
        public Define.ECurrencyType LevelUpCurrency1;
        public int LevelUpCurrency1Count;
        public Define.ECurrencyType LevelUpCurrency2;
        public int LevelUpCurrency2Count;
        public Define.ECurrencyType LevelUpCurrency3;
        public int LevelUpCurrency3Count;
        public Define.ECurrencyType LevelUpCurrency4;
        public int LevelUpCurrency4Count;
        public int OriginalLevelId;
        public int PreviewLevelId;
        public int NextLevelId;

        [ExcludeFieldAttribute]
        public List<int> SKillIds;

        [ExcludeFieldAttribute]
        public List<LevelUpCurrency> LevelUpCurrencies;
    }

    public class BuddyDataLoader : ILoader<int, BuddyData>
    {
        public List<BuddyData> buddies = new List<BuddyData>();
        public Dictionary<int, BuddyData> MakeDict()
        {
            Dictionary<int, BuddyData> dict = new Dictionary<int, BuddyData>();
            foreach (BuddyData buddy in buddies)
                dict.Add(buddy.TemplateId, buddy);
            return dict;
        }

        public bool Validate()
        {
            foreach (BuddyData buddy in buddies)
            {
                buddy.SKillIds = new List<int>();
                if(buddy.Skill1Id != 0)
                    buddy.SKillIds.Add(buddy.Skill1Id);
                if (buddy.Skill2Id != 0)
                    buddy.SKillIds.Add(buddy.Skill2Id);
                if(buddy.Skill3Id != 0)
                    buddy.SKillIds.Add(buddy.Skill3Id);
                if(buddy.Skill4Id != 0)
                    buddy.SKillIds.Add(buddy.Skill4Id);
                if(buddy.Skill5Id != 0)
                    buddy.SKillIds.Add(buddy.Skill5Id);

                buddy.LevelUpCurrencies = new List<LevelUpCurrency>();
                if (buddy.LevelUpCurrency1 != Define.ECurrencyType.None)
                {
                    buddy.LevelUpCurrencies.Add(new LevelUpCurrency(buddy.LevelUpCurrency1, buddy.LevelUpCurrency1Count));
                }
                if (buddy.LevelUpCurrency2 != Define.ECurrencyType.None)
                {
                    buddy.LevelUpCurrencies.Add(new LevelUpCurrency(buddy.LevelUpCurrency2, buddy.LevelUpCurrency2Count));
                }
                if (buddy.LevelUpCurrency3 != Define.ECurrencyType.None)
                {
                    buddy.LevelUpCurrencies.Add(new LevelUpCurrency(buddy.LevelUpCurrency3, buddy.LevelUpCurrency3Count));
                }
                if (buddy.LevelUpCurrency4 != Define.ECurrencyType.None)
                {
                    buddy.LevelUpCurrencies.Add(new LevelUpCurrency(buddy.LevelUpCurrency4, buddy.LevelUpCurrency4Count));
                }
            }

            return true;
        }
    }
    #endregion

    #region BuddySkillData

    public class LevelUpCurrency
    {
        public Define.ECurrencyType currencyType;
        public int count;

        public LevelUpCurrency(Define.ECurrencyType currencyType, int count)
        {
            this.currencyType = currencyType;
            this.count = count;
        }
    }

    [Serializable]
    public class BuddySkillData : SkillData
    {
        public int TemplateId;
        public string Name;
        public string NameTextId;
        public string DescriptionTextId;
        public int SkillLevel;
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
        public Define.ECurrencyType LevelUpCurrency1;
        public int LevelUpCurrency1Count;
        public Define.ECurrencyType LevelUpCurrency2;
        public int LevelUpCurrency2Count;
        public Define.ECurrencyType LevelUpCurrency3;
        public int LevelUpCurrency3Count;
        public Define.ECurrencyType LevelUpCurrency4;
        public int LevelUpCurrency4Count;
        public Define.ECurrencyType LevelUpCurrency5;
        public int LevelUpCurrency5Count;
        public int OriginalLevelId;
        public int PreviewLevelId;
        public int NextLevelId;

        [ExcludeFieldAttribute]
        public List<LevelUpCurrency> LevelUpCurrencies;

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
            foreach(BuddySkillData skill in buddySkills)
            {
                skill.LevelUpCurrencies = new List<LevelUpCurrency>();
                if(skill.LevelUpCurrency1 != Define.ECurrencyType.None)
                {
                    skill.LevelUpCurrencies.Add(new LevelUpCurrency(skill.LevelUpCurrency1, skill.LevelUpCurrency1Count));
                }
                if (skill.LevelUpCurrency2 != Define.ECurrencyType.None)
                {
                    skill.LevelUpCurrencies.Add(new LevelUpCurrency(skill.LevelUpCurrency2, skill.LevelUpCurrency2Count));
                }
                if (skill.LevelUpCurrency3 != Define.ECurrencyType.None)
                {
                    skill.LevelUpCurrencies.Add(new LevelUpCurrency(skill.LevelUpCurrency3, skill.LevelUpCurrency3Count));
                }
                if (skill.LevelUpCurrency4 != Define.ECurrencyType.None)
                {
                    skill.LevelUpCurrencies.Add(new LevelUpCurrency(skill.LevelUpCurrency4, skill.LevelUpCurrency4Count));
                }
                if (skill.LevelUpCurrency5 != Define.ECurrencyType.None)
                {
                    skill.LevelUpCurrencies.Add(new LevelUpCurrency(skill.LevelUpCurrency5, skill.LevelUpCurrency5Count));
                }
            }

            return true;
        }
    }
    #endregion

    #region HeroGachaData
    [Serializable]
    public class HeroGachaData
    {
        public string GachaItem;
        public Define.ECurrencyType CurrencyType;
        public int CurrencyCount;
        public int Percent;
        public int Max;
    }

    public class HeroGachaDataLoader : ILoader<string, HeroGachaData>
    {
        public List<HeroGachaData> HeroGachaList = new List<HeroGachaData>();

        public Dictionary<string, HeroGachaData> MakeDict()
        {
            Dictionary<string, HeroGachaData> dict = new Dictionary<string, HeroGachaData>();
            foreach (var heroGacha in HeroGachaList)
                dict.Add(heroGacha.GachaItem, heroGacha);
            return dict;
        }

        public bool Validate()
        {
            for(int i = 1; i < HeroGachaList.Count; i++)
            {
                HeroGachaList[i].Percent += HeroGachaList[i - 1].Percent;
            }
            return true;
        }
    }
    #endregion

    #region CurrencyGacha
    [Serializable]
    public class CurrencyGachaData
    {
        public string GachaItem;
        public Define.ECurrencyType CurrencyType;
        public int CurrencyCount;
        public int Percent;
        public int Max;
    }

    public class CurrencyGachaDataLoader : ILoader<string, CurrencyGachaData>
    {
        public List<CurrencyGachaData> currencyGachaList = new List<CurrencyGachaData>();

        public Dictionary<string, CurrencyGachaData> MakeDict()
        {
            Dictionary<string, CurrencyGachaData> dict = new Dictionary<string, CurrencyGachaData>();
            foreach (var currencyGacha in currencyGachaList)
                dict.Add(currencyGacha.GachaItem, currencyGacha);
            return dict;
        }

        public bool Validate()
        {
            for (int i = 1; i < currencyGachaList.Count; i++)
            {
                currencyGachaList[i].Percent += currencyGachaList[i - 1].Percent;
            }
            return true;
        }
    }
    #endregion

    #region BuddyGachaRarityData
    [Serializable]
    public class BuddyGachaRarityData
    {
        public Define.ERarityType RarityType;
        public int Percent;
        public int Max;
    }

    public class BuddyGachaRarityDataLoader : ILoader<Define.ERarityType, BuddyGachaRarityData>
    {
        public List<BuddyGachaRarityData> rarityList = new List<BuddyGachaRarityData>();

        public Dictionary<Define.ERarityType, BuddyGachaRarityData> MakeDict()
        {
            Dictionary<Define.ERarityType, BuddyGachaRarityData> dict = new Dictionary<Define.ERarityType, BuddyGachaRarityData>();
            foreach (var rarity in rarityList)
                dict.Add(rarity.RarityType, rarity);
            return dict;
        }

        public bool Validate()
        {
            for (int i = 1; i < rarityList.Count; i++)
            {
                rarityList[i].Percent += rarityList[i - 1].Percent;
            }
            return true;
        }
    }
    #endregion

    #region BuddyGachaData
    [Serializable]
    public class BuddyGachaData
    {
        public string GachaItem;
        public string SpineNameKey;
        public Define.ERarityType Rarity;
        public Define.ECurrencyType CurrencyType;
        public int CurrencyCount;
        public int BuddyTemplateId;
    }

    public class BuddyGachaDataLoader : ILoader<string, BuddyGachaData>
    {
        public List<BuddyGachaData> buddyGachaList = new List<BuddyGachaData>();

        public Dictionary<string, BuddyGachaData> MakeDict()
        {
            Dictionary<string, BuddyGachaData> dict = new Dictionary<string, BuddyGachaData>();
            foreach (var buddy in buddyGachaList)
                dict.Add(buddy.GachaItem, buddy);
            return dict;
        }

        public bool Validate()
        {
            Managers.Data.commonBuddies = new List<string>();
            Managers.Data.rareBuddies = new List<string>();
            Managers.Data.epicBuddies = new List<string>();
            Managers.Data.uniqueBuddies = new List<string>();
            Managers.Data.legendBuddies = new List<string>();

            foreach(var buddy in buddyGachaList)
            {
                if(buddy.Rarity == Define.ERarityType.Common)
                { 
                    Managers.Data.commonBuddies.Add(buddy.GachaItem);
                }
                else if (buddy.Rarity == Define.ERarityType.Rare)
                {
                    Managers.Data.rareBuddies.Add(buddy.GachaItem);
                }
                else if (buddy.Rarity == Define.ERarityType.Epic)
                {
                    Managers.Data.epicBuddies.Add(buddy.GachaItem);
                }
                else if (buddy.Rarity == Define.ERarityType.Unique)
                {
                    Managers.Data.uniqueBuddies.Add(buddy.GachaItem);
                }
                else if (buddy.Rarity == Define.ERarityType.Legend)
                {
                    Managers.Data.legendBuddies.Add(buddy.GachaItem);
                }
            }

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
        public string NameTextId;
        public string DescriptionTextId;
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

    #region CurrencyData
    [Serializable]
    public class CurrencyTypeData
    {
        public Define.ECurrencyType CurrencyType;
        public string IconImage;
    }

    public class CurrencyTypeDataLoader : ILoader<Define.ECurrencyType, CurrencyTypeData>
    {
        public List<CurrencyTypeData> currencies = new List<CurrencyTypeData>();

        public Dictionary<Define.ECurrencyType, CurrencyTypeData> MakeDict()
        {
            Dictionary<Define.ECurrencyType, CurrencyTypeData> dict = new Dictionary<Define.ECurrencyType, CurrencyTypeData>();
            foreach (CurrencyTypeData currency in currencies)
                dict.Add(currency.CurrencyType, currency);
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region MissionData
    public class RewardCurrency
    {
        public int point;
        public Define.ECurrencyType currencyType;
        public int count;

        public RewardCurrency(int point, Define.ECurrencyType currencyType, int count)
        {
            this.point = point;
            this.currencyType = currencyType;
            this.count = count;
        }
    }

    [Serializable]
    public class MissionData
    {
        public int TemplateId;
        public string Name;
        public string NameTextId;
        public Define.EMissionType MissionType;
        public Define.EMissionGoal MissionGoal;
        public int MissionCount;
        public int MaxPoint;
        public int Point;
        public List<int> PointStep;
        public List<Define.ECurrencyType> RewardType;
        public List<int> RewardCount;

        [ExcludeFieldAttribute]
        public List<RewardCurrency> RewardCurrencies;
    }

    public class MissionDataLoader : ILoader<int, MissionData>
    {
        public List<MissionData> missions = new List<MissionData>();
        public Dictionary<int, MissionData> MakeDict()
        {
            Dictionary<int, MissionData> dict = new Dictionary<int, MissionData>();
            foreach (var mission in missions)
                dict.Add(mission.TemplateId, mission);
            return dict;
        }

        public bool Validate()
        {
            foreach (var mission in missions)
            {
                mission.RewardCurrencies = new List<RewardCurrency>();

                for(int i = 0; i < mission.PointStep.Count; i++)
                {
                    if(mission.RewardCount[i] == 0)
                        continue;

                    mission.RewardCurrencies.Add(new RewardCurrency(mission.PointStep[i], mission.RewardType[i], mission.RewardCount[i]));
                }
            }

            return true;
        }
    }
    #endregion

}