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
}