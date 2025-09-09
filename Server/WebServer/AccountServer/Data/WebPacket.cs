using AccountDB;

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