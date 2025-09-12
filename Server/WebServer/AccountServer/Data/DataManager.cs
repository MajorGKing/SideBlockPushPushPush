namespace Server.Data
{
    using System.Collections.Generic;
    using AccountServer;

    public interface IValidate
    {
        bool Validate();
    }

    public interface ILoader<Key, Value> : IValidate
    {
        Dictionary<Key, Value> MakeDict();
    }

    public class DataManager
    {
        private static HashSet<IValidate> _loaders = new HashSet<IValidate>();

        public static Dictionary<string, Data.TextData> TextDict { get; private set; } = new Dictionary<string, Data.TextData>();
        public static Dictionary<int, Data.HeroSkillData> HeroSkillDataDic { get; set; } = new Dictionary<int, Data.HeroSkillData>();
        public static Dictionary<int, Data.BuddySkillData> BuddySkillDataDic { get; set; } = new Dictionary<int, Data.BuddySkillData>();
        public static Dictionary<int, Data.EffectData> EffectDataDic { get; set; } = new Dictionary<int, Data.EffectData>();
        public static Dictionary<int, Data.StageData> StageDataDic { get; set; } = new Dictionary<int, Data.StageData>();
        public static Dictionary<int, Data.MonsterData> MonsterDataDic { get; set; } = new Dictionary<int, Data.MonsterData>();
        public static Dictionary<int, Data.ProgressionTypeData> ProgressionTypeDataDic { get; set; } = new Dictionary<int, Data.ProgressionTypeData>();
        public static Dictionary<Define.ECurrencyType, Data.CurrencyTypeData> CurrencyTypeDataDic { get; set; } = new Dictionary<Define.ECurrencyType, Data.CurrencyTypeData>();
        public static Dictionary<int, Data.BuddyData> BuddyDataDic { get; set; } = new Dictionary<int, Data.BuddyData>();
        public static Dictionary<int, Data.HeroData> HeroDataDic { get; set; } = new Dictionary<int, Data.HeroData>();
        public static Dictionary<string, Data.HeroGachaData> HeroGachaDataDic { get; set; } = new Dictionary<string, Data.HeroGachaData>();
        public static Dictionary<string, Data.CurrencyGachaData> CurrencyGachaDataDic { get; set; } = new Dictionary<string, Data.CurrencyGachaData>();
        public static Dictionary<Define.ERarityType, Data.BuddyGachaRarityData> BuddyGachaRarityDataDic { get; set; } = new Dictionary<Define.ERarityType, Data.BuddyGachaRarityData>();
        public static Dictionary<string, Data.BuddyGachaData> BuddyGachaDataDic { get; set; } = new Dictionary<string, Data.BuddyGachaData>();
        public static Dictionary<int, Data.MissionData> MissionDataDic { get; set; } = new Dictionary<int, Data.MissionData>();
        public static Dictionary<int, Data.AchievementData> AchievementDataDic { get; set; } = new Dictionary<int, Data.AchievementData>();

        // BuddyList
        public static List<string> commonBuddies;
        public static List<string> rareBuddies;
        public static List<string> epicBuddies;
        public static List<string> uniqueBuddies;
        public static List<string> legendBuddies;


        public static void LoadData()
        {
            TextDict = LoadJson<Data.TextDataLoader, string, Data.TextData>("TextData").MakeDict();
            HeroSkillDataDic = LoadJson<Data.HeroSkillDataLoader, int, Data.HeroSkillData>("HeroSkillData").MakeDict();
            BuddySkillDataDic = LoadJson<Data.BuddySkillDataLoader, int, Data.BuddySkillData>("BuddySkillData").MakeDict();
            EffectDataDic = LoadJson<Data.EffectDataLoader, int, Data.EffectData>("EffectData").MakeDict();
            StageDataDic = LoadJson<Data.StageDataLoader, int, Data.StageData>("StageData").MakeDict();
            MonsterDataDic = LoadJson<Data.MonsterDataLoader, int, Data.MonsterData>("MonsterData").MakeDict();
            ProgressionTypeDataDic = LoadJson<Data.ProgressionTypeDataLoader, int, Data.ProgressionTypeData>("ProgressionTypeData").MakeDict();
            CurrencyTypeDataDic = LoadJson<Data.CurrencyTypeDataLoader, Define.ECurrencyType, Data.CurrencyTypeData>("CurrencyTypeData").MakeDict();
            BuddyDataDic = LoadJson<Data.BuddyDataLoader, int, Data.BuddyData>("BuddyData").MakeDict();
            HeroDataDic = LoadJson<Data.HeroDataLoader, int, Data.HeroData>("HeroData").MakeDict();
            HeroGachaDataDic = LoadJson<Data.HeroGachaDataLoader, string, Data.HeroGachaData>("HeroGachaData").MakeDict();
            CurrencyGachaDataDic = LoadJson<Data.CurrencyGachaDataLoader, string, Data.CurrencyGachaData>("CurrencyGachaData").MakeDict();
            BuddyGachaRarityDataDic = LoadJson<Data.BuddyGachaRarityDataLoader, Define.ERarityType, Data.BuddyGachaRarityData>("BuddyGachaRarityData").MakeDict();
            BuddyGachaDataDic = LoadJson<Data.BuddyGachaDataLoader, string, Data.BuddyGachaData>("BuddyGachaData").MakeDict();
            MissionDataDic = LoadJson<Data.MissionDataLoader, int, Data.MissionData>("MissionData").MakeDict();
            AchievementDataDic = LoadJson<Data.AchievementDataLoader, int, Data.AchievementData>("AchievementData").MakeDict();
            Validate();

        }

        private static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
        {
            string text = File.ReadAllText($"{ConfigManager.Config.dataPath}/JsonData/{path}.json");    
            Loader loader = Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(text);
            _loaders.Add(loader);
            return loader;
        }

        private static bool Validate()
        {
            bool success = true;

            foreach (var loader in _loaders)
            {
                if (loader.Validate() == false)
                    success = false;
            }

            _loaders.Clear();

            return success;
        }

    }

}
