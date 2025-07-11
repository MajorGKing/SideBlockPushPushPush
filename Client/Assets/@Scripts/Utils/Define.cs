using System;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class Define
{
    public const char MAP_TOOL_WALL = '0';
    public const char MAP_TOOL_NONE = '1';

    public enum EScene
    {
        Unknown,
        TitleScene,
        LoadingScene,
        LobbyScene,
        GameScene,
    }

    public enum ESound
    {
        Bgm,
        SubBgm,
        Effect,
        Max,
    }

    public enum ETouchEvent
    {
        PointerUp,
        PointerDown,
        Click,
        Pressed,
        BeginDrag,
        Drag,
        EndDrag,
    }

    public enum ELanguage
	{
        Korean,
        English,
        French,
        SimplifiedChinese,
        TraditionalChinese,
        Japanese,
	}

    public enum EEventType
	{
		None,

		OnClickAttackButton,
		OnClickAutoButton,

		InventoryChanged,
		CurrencyChanged,
		StatChanged,
		QuestUpdated,
		CollectionUpdated,
	}

	public enum ELayer
	{
		Default = 0,
		TransparentFX = 1,
		IgnoreRaycast = 2,
		Dummy1 = 3,
		Water = 4,
		UI = 5,
		Hero = 6,
		Monster = 7,
		Boss = 8,
		//
		Env = 11,
		Obstacle = 12,
		//
		Projectile = 20,
	}

    #region Object
    public enum EGameObjectType
    {
        None,
        Map,
        Hero,
        Buddy,
        Monster,
        SkillEffect,
        SkillCube,
        Projectile,
    }
    #endregion

    public enum ESkillType
    {
        Active,
        Passive,
    }

    public enum EUseSkillTargetType
    {
        Other,
        Self,
    }

    public enum ETargetFriendType
    {
        Enemy,
        Company,
        Hero,
    }

    public enum EEffectType
    {
        Damage,
        MagicDamage,
        MagicDamageLight,
        MagicDamageDark,
        MagicDamageFire,
        MagicDamageWater,
        MagicDamageEarth,
        MagicDamageWind,
    }

    public enum EDurationPolicy
    {
        Instant,
        Duration,
        All
    }


    #region Tag
    public const string HEROTAG = "Hero";
    public const string BUDDYTAG = "Buddy";
    #endregion

    #region HardCoding
    public const int HEROLINENUMBHER = 100;
    #endregion
}