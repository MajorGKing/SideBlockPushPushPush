using AccountDB;
using GameDB;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

public class LoginAccountPacketReq
{
	public string userId { get; set; } = String.Empty;
	public string token { get; set; } = string.Empty;
}

public class LoginAccountPacketRes
{
	public ProviderType providerType { get; set; }
	public bool success { get; set; } = false;
	public long accountDbId { get; set; }
	public string jwt { get; set; } = string.Empty;
}

public class PlayerPacketReq
{
    public string jwt { get; set; } = string.Empty;
}

public class PlayerPacketRes
{
    // 요청 성공 여부
    public bool Success { get; set; }

    // 응답 메시지 (선택 사항)
    public string Message { get; set; }

    // 플레이어의 핵심 데이터를 담는 클래스
    public PlayerData PlayerData { get; set; }
}

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
    public int HeroSaveDataDbId { get; set; }
    public int TemplateId { get; set; }
    public List<int> SkillTemplateIds { get; set; } = new List<int>();
    public bool IsSelected { get; set; }
    public int NowExp { get; set; }
    public int MaxExp { get; set; }
}

public class HeroListReq
{
    public string jwt { get; set; } = string.Empty;
}

public class HeroListRes
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<HeroDTO> Heroes { get; set; } = new List<HeroDTO>();
}

