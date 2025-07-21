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

        // Stage
        OnStageWaveIndexChanged,
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

    #region Toast
    public enum EToastColor
    {
        Black,
        Red,
        Purple,
        Magenta,
        Blue,
        Green,
        Yellow,
        Orange
    }

    public enum EToastPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
    #endregion

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

    #region Data
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

    public enum EDifficultyLevel
    {
        None,
        Normal,
        Hard,
    }

    #endregion

    #region Stage

    public enum EStageState
    {
        None,
        Start,
        Battle,
        Move,
        Over,
        Clear,
    }

    #endregion

    #region Reward
    public enum ERewardType
    {
        None,
        StageClear,
    }

    public enum ECurrencyType
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

    public enum EUserInfoItem
    {
        Stamina,
        Dia,
        Gold,
    }

    #endregion



    #region Tag
    public const string HEROTAG = "Hero";
    public const string BUDDYTAG = "Buddy";
    #endregion

    #region HardCoding
    public const int HEROLINENUMBHER = 100;
    public const int MAX_STAMINA = 50;
    public const int GAME_PER_STAMINA = 3;

    public const string GREENBUTTON = "Btn_MainButton_Green";
    public const string REDBUTTON = "Btn_MainButton_Red";
    #endregion
}