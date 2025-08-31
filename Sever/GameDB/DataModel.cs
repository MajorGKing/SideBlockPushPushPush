using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDB
{
	[Table("Player")]
	public class PlayerDb
	{
		// 기본 키 (Primary Key)
		[Key]
		public int PlayerDbId { get; set; }

		// GameData 클래스의 기본 속성들
		public int UserLevel { get; set; } = 1;
		public string UserName { get; set; } = "Player";
		public int Stamina { get; set; } = Define.MAX_STAMINA;
		public bool BGMOn { get; set; } = true;
		public bool EffectSoundOn { get; set; } = true;
		public DateTime LastMissionTime { get; set; }

		// 네비게이션 프로퍼티 (1:1 관계)
		// PlayerDb는 CurrencyDb 테이블의 기본 키(PlayerDbId)와 연결됩니다.
		public CurrencyDb Currency { get; set; }

		// 네비게이션 프로퍼티 (1:N 관계)
		// Player 한 명은 여러 개의 HeroSaveData를 가질 수 있습니다.
		public ICollection<HeroSaveDataDb> Heroes { get; set; } = new List<HeroSaveDataDb>();

		// Player 한 명은 여러 개의 BuddySaveData를 가질 수 있습니다.
		public ICollection<BuddySaveDataDb> Buddies { get; set; } = new List<BuddySaveDataDb>();

		// Player 한 명은 여러 개의 StageClear 데이터를 가질 수 있습니다.
		public ICollection<StageClearDb> StageClears { get; set; } = new List<StageClearDb>();

		// Player 한 명은 여러 개의 MissionSaveData를 가질 수 있습니다.
		public ICollection<MissionSaveDataDb> Missions { get; set; } = new List<MissionSaveDataDb>();

		// Player 한 명은 여러 개의 AchievementSaveData를 가질 수 있습니다.
		public ICollection<AchievementSaveDataDb> Achievements { get; set; } = new List<AchievementSaveDataDb>();

		// Player 한 명은 여러 개의 AchievementClearList 데이터를 가질 수 있습니다.
		public ICollection<AchievementClearListDb> AchievementClearList { get; set; } = new List<AchievementClearListDb>();
	}

	[Table("Currency")]
	public class CurrencyDb
	{
		// 기본 키 (Primary Key)이자 외래 키 (Foreign Key)
		// PlayerDb 테이블의 기본 키와 동일한 이름을 사용하며, 1:1 관계를 명시합니다.
		[Key]
		[ForeignKey("Player")]
		public int PlayerDbId { get; set; }

		// ECurrencyType에 정의된 모든 화폐를 컬럼으로 직접 포함합니다.
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

		// 네비게이션 프로퍼티: 1:1 관계의 상대방
		public PlayerDb Player { get; set; }
	}

	[Table("HeroSaveData")]
	public class HeroSaveDataDb
	{
		// 기본 키 (Primary Key)
		// EF Core 컨벤션에 따라 '클래스명' + 'Id'로 명명하여 자동으로 기본 키로 인식됩니다.
		[Key]
		public int HeroSaveDataDbId { get; set; }

		// HeroSaveData 클래스의 속성들
		public int TemplateId { get; set; }
		public bool IsSelected { get; set; }
		public int NowExp { get; set; }
		public int MaxExp { get; set; }

		// 외래 키 (Foreign Key)
		// PlayerDb 테이블의 기본 키인 PlayerDbId를 참조합니다.
		public int PlayerDbId { get; set; }

		// 네비게이션 프로퍼티: 1:N 관계의 상대방
		// 이 속성을 통해 PlayerDb 객체에 접근할 수 있습니다.
		public PlayerDb Player { get; set; }
	}

	[Table("BuddySaveData")]
	public class BuddySaveDataDb
	{
		// 기본 키 (Primary Key)
		// EF Core 컨벤션에 따라 '클래스명' + 'Id'로 명명하여 자동으로 기본 키로 인식됩니다.
		[Key]
		public int BuddySaveDataDbId { get; set; }

		// BuddySaveData 클래스의 속성들
		public int TemplateId { get; set; }
		public bool IsSelected { get; set; }

		// 외래 키 (Foreign Key)
		// PlayerDb 테이블의 기본 키인 PlayerDbId를 참조합니다.
		public int PlayerDbId { get; set; }

		// 네비게이션 프로퍼티: 1:N 관계의 상대방
		// 이 속성을 통해 PlayerDb 객체에 접근할 수 있습니다.
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
		public Define.EMissionState MissionState { get; set; }

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
		public Define.EMissionState MissionState { get; set; }

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
