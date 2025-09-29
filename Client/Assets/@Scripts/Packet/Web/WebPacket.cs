using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WebPacket
{
	using System;
    using System.Diagnostics.CodeAnalysis;
    using static Define;

    public enum EProviderType
    {
        None = 0,
        Guest = 1,
        Google = 2,
        Facebook = 3,
    }

    [Serializable]
	public class JwtToken
	{
		public long sub;
		public long iat;
		public long exp;
	}

    [Serializable]
    public class LoginAccountPacketReq
    {
        public string userId;
        public string token;
    }

    [Serializable]
    public class LoginAccountPacketRes
    {
        public EProviderType providerType { get; set; }
        public bool success;
        public long accountDbId;
        public string jwt;
    }

    [Serializable]
    // Player 테이블의 데이터를 담는 DTO(Data Transfer Object)
    public class PlayerData
    {
        public int PlayerDbId { get; set; }
        public int UserLevel { get; set; }
        public string UserName { get; set; }
        public int Stamina { get; set; }
        public bool BGMOn { get; set; }
        public bool EffectSoundOn { get; set; }
        public DateTime LastMissionTime { get; set; }
        public int CurrentStage { get; set; }
    }

    [Serializable]
    public class PlayerPacketReq
    {
        public string jwt { get; set; } = string.Empty;
    }

    [Serializable]
    public class PlayerPacketRes
    {
        // 요청 성공 여부
        public bool Success { get; set; }

        // 응답 메시지 (선택 사항)
        public string Message { get; set; }

        // 플레이어의 핵심 데이터를 담는 클래스
        public PlayerData PlayerData { get; set; }
    }

    public enum CurrencyType
    {
        Gold,
        Dia,
        BlueGem,
        GreenGem,
        YellowGem,
        StoneArmor,
        StoneBelt,
        StoneBoots,
        StoneGloves,
        StoneRing,
        StoneWeapon,
        Exp,
        ScrollArmor,
        ScrollBelt,
        ScrollBoots,
        ScrollGloves,
        ScrollRing,
        ScrollWeapon,
    }

    public class CurrencyData
    {
        public int PlayerDbId { get; set; }
        public int Gold { get; set; }
        public int Dia { get; set; }
        public int BlueGem { get; set; }
        public int GreenGem { get; set; }
        public int YellowGem { get; set; }
        public int StoneArmor { get; set; }
        public int StoneBelt { get; set; }
        public int StoneBoots { get; set; }
        public int StoneGloves { get; set; }
        public int StoneRing { get; set; }
        public int StoneWeapon { get; set; }
        public int Exp { get; set; }
        public int ScrollArmor { get; set; }
        public int ScrollBelt { get; set; }
        public int ScrollBoots { get; set; }
        public int ScrollGloves { get; set; }
        public int ScrollRing { get; set; }
        public int ScrollWeapon { get; set; }
    }

    public class CurrencyAllReq
    {
        public string jwt { get; set; } = string.Empty;
    }

    public class CurrencyAllRes
    {
        // 요청 성공 여부
        public bool Success { get; set; }
        [AllowNull]
        public CurrencyData currencyData { get; set; }
    }

    public class CurrencyAddReq
    {
        public string jwt { get; set; } = string.Empty;
        public CurrencyType CurrencyType { get; set; }
        public int Amount { get; set; }
    }

    public class HeroDTO
    {
        public int TemplateId { get; set; }
        public List<int> SkillTemplateIds { get; set; } = new List<int>();
        public bool IsSelected { get; set; }
        public int NowExp { get; set; }
        public int MaxExp { get; set; }
    }

    public class HeroListReq
    {
        public string Jwt { get; set; } = string.Empty;
    }

    public class HeroListRes
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<HeroDTO> Heroes { get; set; } = new List<HeroDTO>();
    }

    public class HeroNowChangeReq
    {
        public string Jwt { get; set; } = string.Empty;
        public int TemplateId { get; set; }
    }

    public class HeroLevelUpReq
    {
        public string Jwt { get; set; } = string.Empty;
        public int TemplateId { get; set; } // 현재 선택된 영웅 TemplateId
    }

    public class HeroSkillLevelUpReq
    {
        public string Jwt { get; set; } = string.Empty;
        public int HeroTemplateId { get; set; } // 현재 선택된 영웅 TemplateId
        public int HeroSkillTemplateId { get; set; } // 레벨업할 영웅 스킬 TemplateId
    }

    public class BuddyDTO
    {
        public int TemplateId { get; set; }
        public List<int> SkillTemplateId { get; set; } = new List<int>();
        public int SelectedNumber { get; set; }
    }

    public class BuddyListReq
    {
        public string Jwt { get; set; } = string.Empty;
    }

    public class BuddyListRes
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<BuddyDTO> Buddies { get; set; } = new List<BuddyDTO>();
    }

    public class BuddySelectedRemoveReq
    {
        public string Jwt { get; set; } = string.Empty;
        public int TemplateId { get; set; } // 삭제 buddy TemplateId
    }

    public class BuddySelectedAddReq
    {
        public string Jwt { get; set; } = string.Empty;
        public int TemplateId { get; set; } // 추가 buddy TemplateId
    }

    public class BuddyLevelUpReq
    {
        public string Jwt { get; set; } = string.Empty;
        public int TemplateId { get; set; } // 현재 선택된 동료 TemplateId
    }

    public class BuddySkillLevelUpReq
    {
        public string Jwt { get; set; } = string.Empty;
        public int BuddyTemplateId { get; set; } // 현재 선택된 동료 TemplateId
        public int BuddySkillTemplateId { get; set; } // 레벨업 할 동료 스킬 TemplateId
    }

    public class HeroGachaReward
    {
        public CurrencyType Type { get; set; }
        public int Count { get; set; }
    }

    public class ShopHeroGachaReq
    {
        public string Jwt { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ShopHeroGachaRes
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<HeroGachaReward> Rewards { get; set; } = new List<HeroGachaReward>();
    }

    public class BuddyGachaReward
    {
        public string BuddyName { get; set; } = string.Empty;// 새로 획득한 동료 이름Id
        public bool IsDuplicate { get; set; } // 이미 획득한 둥료인가?
    }

    public class ShopBuddyGachaReq
    {
        public string Jwt { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ShopBuddyGachaRes
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<BuddyGachaReward> Rewards { get; set; } = new List<BuddyGachaReward>();
    }

    public class CurrencyGachaReward
    {
        public CurrencyType Type { get; set; }
        public int Count { get; set; }
    }

    public class ShopCurrencyGachaReq
    {
        public string Jwt { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ShopCurrencyGachaRes
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<CurrencyGachaReward> Rewards { get; set; } = new List<CurrencyGachaReward>();
    }

    public class StageClearDTO
    {
        public int TemplateId { get; set; }
        public bool IsEnable { get; set; }
        public bool IsClear { get; set; }
    }

    public class StageClearListReq
    {
        public string Jwt { get; set; } = string.Empty;
    }

    public class StageClearListRes
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<StageClearDTO> Stages { get; set; } = new List<StageClearDTO>();
    }

    public class StageClearNowTemplateIdSetReq
    {
        public string Jwt { get; set; } = string.Empty;
        public int TemplatedId { get; set; }
    }

    public class StageClearNowTemplateIdSetRes
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TemplatedId { get; set; }
    }

    public class StageStartDataReq
    {
        public string Jwt { get; set; } = string.Empty;
    }

    public class StageStartDataRes
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public List<MonsterSnapshot> FirstWave { get; set; } = new List<MonsterSnapshot>();
        public List<MonsterSnapshot> SecondWave { get; set; } = new List<MonsterSnapshot>();
        public List<MonsterSnapshot> BossWave { get; set; } = new List<MonsterSnapshot>();
        public HeroSnapshot Hero { get; set; } = new HeroSnapshot();
        public List<BuddySnapshot> Buddies { get; set; } = new List<BuddySnapshot>();
    }

    public class MonsterSnapshot
    {
        public int TemplateId { get; set; }
        public int Level { get; set; }
        public int MaxHp { get; set; }
        public int NormalDefence { get; set; }
        public int MagicDefence { get; set; }
    }

    public class HeroSnapshot
    {
        public int TemplateId { get; set; }
        public int Level { get; set; }
        public int Attack { get; set; }
        public int MagicAttack { get; set; }
        public List<SkillSnapshot> Skills { get; set; } = new List<SkillSnapshot>();
    }

    public class BuddySnapshot
    {
        public int TemplateId { get; set; }
        public int Level { get; set; }
        public int Attack { get; set; }
        public int MagicAttack { get; set; }
        public float Reload { get; set; }
        public List<SkillSnapshot> Skills { get; set; } = new List<SkillSnapshot>();
    }

    public class SkillSnapshot
    {
        public int TemplateId { get; set; }
        public int SkillLevel { get; set; }
        public Define.ESkillType SkillType { get; set; }
        public float Cooltime { get; set; } // BuddySkill only
        public float AnimSpeed { get; set; }
        public Define.EUseSkillTargetType UseSkillTargetType { get; set; }
        public int GatherTargetCounts { get; set; }
        public int GatherTargetType { get; set; }
        public Define.ETargetFriendType TargetFriendType { get; set; }

        // Icon(s) for UI display
        public List<string>? IconImageKeys { get; set; } = null; // Hero
        public string? IconImageKey { get; set; } = null;         // Buddy

        // One effect per skill
        public EffectSnapshot Effect { get; set; } = new EffectSnapshot();
    }

    public class EffectSnapshot
    {
        public int TemplateId { get; set; }
        public Define.EEffectType EffectType { get; set; }
        public Define.EDurationPolicy DurationPolicy { get; set; }
        public float Duration { get; set; }
        public float DamageValue { get; set; }
        public int StatType { get; set; }
        public float AddValue { get; set; }
        public int LifeStealValue { get; set; }
        public int StunValue { get; set; }
    }

    public class RewardDTO
    {
        public Define.ECurrencyType RewardType { get; set; }
        public int RewardAmount { get; set; }
        public bool IsFirst { get; set; }
    }

    public class StageRewardReq
    {
        public string Jwt { get; set; } = string.Empty;
    }

    public class StageRewardRes
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<RewardDTO> Rewards { get; set; } = new List<RewardDTO>();
    }

    public class SetNextStageReq
    {
        public string Jwt { get; set; } = string.Empty;
    }

    public class SetNextStageRes
    {
        public bool Success { get; set; }
        public bool CanChange { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StageTemplateId { get; set; }
    }

    public class SetBackStageReq
    {
        public string Jwt { get; set; } = string.Empty;
    }

    public class SetBackStageRes
    {
        public bool Success { get; set; }
        public bool CanChange { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StageTemplateId { get; set; }
    }

    public class SetHardNormalStageReq
    {
        public string Jwt { get; set; } = string.Empty;
    }

    public class SetHardNormalStageRes
    {
        public bool Success { get; set; }
        public bool CanChange { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StageTemplateId { get; set; }
    }

    public class MissionDTO
    {
        public int TemplateId { get; set; }
        public int StackedPoint { get; set; }
        public EMissionState MissionState { get; set; }
        public int GetRewardCount { get; set; }
    }

    public class GetMissionListReq
    {
        public string Jwt { get; set; } = string.Empty;
    }

    public class GetMissionListRes
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<MissionDTO> Missions { get; set; } = new List<MissionDTO>();
    }

    public class GetNormalMissionRewardReq
    {
        public string Jwt { get; set; } = string.Empty;
        public int TemplatedId { get; set; }
    }

    public class GetMissionRewardReq
    {
        public string Jwt { get; set; } = string.Empty;
        public int TemplatedId { get; set; }
    }

    public class GetMissionRewardRes
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<RewardDTO> Rewards { get; set; } = new List<RewardDTO>();
    }
}