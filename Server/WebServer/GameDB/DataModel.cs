using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Permissions;

namespace GameDB
{
    public enum EMissionState
    {
        None,
        Progress,
        Rewardable,
        Finish,
    }

    /// <summary>
    /// 플레이어의 핵심 데이터를 저장하는 테이블입니다.
    /// </summary>
    [Table("Player")]
    public class PlayerDb
    {
        /// <summary>
        /// 플레이어의 고유 ID. 이 테이블의 기본 키(Primary Key)입니다.
        /// </summary>
        [Key]
        public int PlayerDbId { get; set; }

        /// <summary>
        /// 로그인 또는 기기 식별을 위한 고유 ID.
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string UniqueId { get; set; }

        /// <summary>
        /// 플레이어의 레벨입니다.
        /// </summary>
        public int UserLevel { get; set; } = 1;

        /// <summary>
        /// 플레이어의 닉네임입니다.
        /// </summary>
        public string UserName { get; set; } = "Player";

        /// <summary>
        /// 플레이어가 보유한 스태미나입니다.
        /// </summary>
        public int Stamina { get; set; } = 50;

        /// <summary>
        /// BGM 설정 상태입니다.
        /// </summary>
        public bool BGMOn { get; set; } = true;

        /// <summary>
        /// 효과음 설정 상태입니다.
        /// </summary>
        public bool EffectSoundOn { get; set; } = true;

        /// <summary>
        /// 마지막으로 미션을 진행한 시간입니다.
        /// </summary>
        public DateTime LastMissionTime { get; set; }

        // 네비게이션 프로퍼티 (1:1 관계)
        /// <summary>
        /// 플레이어의 화폐 정보. CurrencyDb 테이블과 1:1 관계를 맺습니다.
        /// </summary>
        public CurrencyDb Currency { get; set; }

        // 네비게이션 프로퍼티 (1:N 관계)
        /// <summary>
        /// 플레이어가 소유한 영웅들의 컬렉션. HeroSaveDataDb 테이블과 1:N 관계를 맺습니다.
        /// </summary>
        public ICollection<HeroSaveDataDb> Heroes { get; set; } = new List<HeroSaveDataDb>();

        /// <summary>
        /// 플레이어가 소유한 버디들의 컬렉션. BuddySaveDataDb 테이블과 1:N 관계를 맺습니다.
        /// </summary>
        public ICollection<BuddySaveDataDb> Buddies { get; set; } = new List<BuddySaveDataDb>();

        /// <summary>
        /// 플레이어가 클리어한 스테이지 목록의 컬렉션. StageClearDb 테이블과 1:N 관계를 맺습니다.
        /// </summary>
        public ICollection<StageClearDb> StageClears { get; set; } = new List<StageClearDb>();

        /// <summary>
        /// 플레이어가 진행 중인 미션 목록의 컬렉션. MissionSaveDataDb 테이블과 1:N 관계를 맺습니다.
        /// </summary>
        public ICollection<MissionSaveDataDb> Missions { get; set; } = new List<MissionSaveDataDb>();

        /// <summary>
        /// 플레이어가 진행 중인 업적 목록의 컬렉션. AchievementSaveDataDb 테이블과 1:N 관계를 맺습니다.
        /// </summary>
        public ICollection<AchievementSaveDataDb> Achievements { get; set; } = new List<AchievementSaveDataDb>();

        /// <summary>
        /// 플레이어가 이미 클리어한 업적 목록의 컬렉션. AchievementClearListDb 테이블과 1:N 관계를 맺습니다.
        /// </summary>
        public ICollection<AchievementClearListDb> AchievementClearList { get; set; } = new List<AchievementClearListDb>();
    }

    /// <summary>
    /// 플레이어가 보유한 모든 화폐를 한 행에 저장하는 테이블입니다.
    /// Player 테이블과 1:1 관계를 맺습니다.
    /// </summary>
    [Table("Currency")]
    public class CurrencyDb
    {
        /// <summary>
        /// 이 테이블의 기본 키이자 PlayerDb 테이블의 외래 키(Foreign Key)입니다.
        /// PlayerDb의 PlayerDbId와 동일한 값을 가집니다.
        /// </summary>
        [Key]
        [ForeignKey("Player")]
        public int PlayerDbId { get; set; }

        // ECurrencyType에 정의된 모든 화폐를 컬럼으로 직접 포함
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

        // 네비게이션 프로퍼티
        /// <summary>
        /// 이 화폐 정보를 소유한 플레이어 객체에 접근할 수 있습니다.
        /// </summary>
        public PlayerDb Player { get; set; }
    }

    /// <summary>
    /// 플레이어가 소유한 영웅의 데이터를 저장하는 테이블입니다.
    /// Player 테이블과 1:N 관계를 맺습니다.
    /// </summary>
    [Table("HeroSaveData")]
    public class HeroSaveDataDb
    {
        /// <summary>
        /// 영웅 데이터의 고유 ID. 이 테이블의 기본 키(Primary Key)입니다.
        /// </summary>
        [Key]
        public int HeroSaveDataDbId { get; set; }

        /// <summary>
        /// 영웅의 종류를 식별하는 고유 ID입니다.
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// 이 영웅이 선택되었는지 여부입니다.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 현재 경험치입니다.
        /// </summary>
        public int NowExp { get; set; }

        /// <summary>
        /// 최대 경험치입니다.
        /// </summary>
        public int MaxExp { get; set; }

        /// <summary>
        /// 이 영웅을 소유한 플레이어의 ID. 외래 키(Foreign Key)입니다.
        /// </summary>
        public int PlayerDbId { get; set; }

        /// <summary>
        /// 이 영웅을 소유한 PlayerDb 객체에 접근할 수 있습니다.
        /// </summary>
        public PlayerDb Player { get; set; }
    }

    /// <summary>
    /// 플레이어가 소유한 버디의 데이터를 저장하는 테이블입니다.
    /// Player 테이블과 1:N 관계를 맺습니다.
    /// </summary>
    [Table("BuddySaveData")]
    public class BuddySaveDataDb
    {
        /// <summary>
        /// 버디 데이터의 고유 ID. 이 테이블의 기본 키(Primary Key)입니다.
        /// </summary>
        [Key]
        public int BuddySaveDataDbId { get; set; }

        /// <summary>
        /// 버디의 종류를 식별하는 고유 ID입니다.
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// 이 버디가 선택되었는지 여부입니다.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 이 버디를 소유한 플레이어의 ID. 외래 키(Foreign Key)입니다.
        /// </summary>
        public int PlayerDbId { get; set; }

        /// <summary>
        /// 이 버디를 소유한 PlayerDb 객체에 접근할 수 있습니다.
        /// </summary>
        public PlayerDb Player { get; set; }
    }

    /// <summary>
    /// 플레이어가 클리어한 스테이지 정보를 저장하는 테이블입니다.
    /// Player 테이블과 1:N 관계를 맺습니다.
    /// </summary>
    [Table("StageClear")]
    public class StageClearDb
    {
        /// <summary>
        /// 스테이지 클리어 데이터의 고유 ID. 이 테이블의 기본 키(Primary Key)입니다.
        /// </summary>
        [Key]
        public int StageClearDbId { get; set; }

        /// <summary>
        /// 스테이지의 고유 ID입니다.
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// 이 스테이지가 활성화되었는지 여부입니다.
        /// </summary>
        public bool isEnable { get; set; }

        /// <summary>
        /// 이 스테이지가 클리어되었는지 여부입니다.
        /// </summary>
        public bool isClear { get; set; }

        /// <summary>
        /// 이 데이터를 소유한 플레이어의 ID. 외래 키(Foreign Key)입니다.
        /// </summary>
        public int PlayerDbId { get; set; }

        /// <summary>
        /// 이 데이터를 소유한 PlayerDb 객체에 접근할 수 있습니다.
        /// </summary>
        public PlayerDb Player { get; set; }
    }

    /// <summary>
    /// 플레이어가 진행 중인 미션 정보를 저장하는 테이블입니다.
    /// Player 테이블과 1:N 관계를 맺습니다.
    /// </summary>
    [Table("MissionSaveData")]
    public class MissionSaveDataDb
    {
        /// <summary>
        /// 미션 데이터의 고유 ID. 이 테이블의 기본 키(Primary Key)입니다.
        /// </summary>
        [Key]
        public int MissionSaveDataDbId { get; set; }

        /// <summary>
        /// 미션의 종류를 식별하는 고유 ID입니다.
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// 현재까지 미션 목표를 달성한 포인트입니다.
        /// </summary>
        public int StackedPoint { get; set; }

        /// <summary>
        /// 미션의 현재 상태입니다 (진행 중, 보상 가능 등).
        /// </summary>
        public EMissionState MissionState { get; set; }

        /// <summary>
        /// 이 미션을 소유한 플레이어의 ID. 외래 키(Foreign Key)입니다.
        /// </summary>
        public int PlayerDbId { get; set; }

        /// <summary>
        /// 이 미션을 소유한 PlayerDb 객체에 접근할 수 있습니다.
        /// </summary>
        public PlayerDb Player { get; set; }
    }

    /// <summary>
    /// 플레이어가 진행 중인 업적 정보를 저장하는 테이블입니다.
    /// Player 테이블과 1:N 관계를 맺습니다.
    /// </summary>
    [Table("AchievementSaveData")]
    public class AchievementSaveDataDb
    {
        /// <summary>
        /// 업적 데이터의 고유 ID. 이 테이블의 기본 키(Primary Key)입니다.
        /// </summary>
        [Key]
        public int AchievementSaveDataDbId { get; set; }

        /// <summary>
        /// 업적의 종류를 식별하는 고유 ID입니다.
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// 현재까지 업적 목표를 달성한 포인트입니다.
        /// </summary>
        public int StackedPoint { get; set; }

        /// <summary>
        /// 업적의 현재 상태입니다 (진행 중, 보상 가능 등).
        /// </summary>
        public EMissionState MissionState { get; set; }

        /// <summary>
        /// 원본 업적의 ID입니다. 연관된 업적이 있을 경우 사용됩니다.
        /// </summary>
        public int OriginalTemplateId { get; set; }

        /// <summary>
        /// 이 업적이 클리어되었는지 여부입니다.
        /// </summary>
        public bool IsCleared { get; set; }

        /// <summary>
        /// 이 업적을 소유한 플레이어의 ID. 외래 키(Foreign Key)입니다.
        /// </summary>
        public int PlayerDbId { get; set; }

        /// <summary>
        /// 이 업적을 소유한 PlayerDb 객체에 접근할 수 있습니다.
        /// </summary>
        public PlayerDb Player { get; set; }
    }

    /// <summary>
    /// 플레이어가 이미 클리어한 업적의 ID 목록을 저장하는 테이블입니다.
    /// Player 테이블과 1:N 관계를 맺습니다.
    /// </summary>
    [Table("AchievementClearList")]
    public class AchievementClearListDb
    {
        /// <summary>
        /// 클리어한 업적 데이터의 고유 ID. 이 테이블의 기본 키(Primary Key)입니다.
        /// </summary>
        [Key]
        public int AchievementClearListDbId { get; set; }

        /// <summary>
        /// 클리어한 업적의 고유 ID입니다.
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// 이 업적을 클리어한 플레이어의 ID. 외래 키(Foreign Key)입니다.
        /// </summary>
        public int PlayerDbId { get; set; }

        /// <summary>
        /// 이 데이터를 소유한 PlayerDb 객체에 접근할 수 있습니다.
        /// </summary>
        public PlayerDb Player { get; set; }
    }

}
