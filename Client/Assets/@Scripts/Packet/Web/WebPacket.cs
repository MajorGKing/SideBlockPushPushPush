using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WebPacket
{
	using System;

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
}