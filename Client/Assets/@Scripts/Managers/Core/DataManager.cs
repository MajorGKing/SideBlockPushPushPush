using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

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
    private HashSet<IValidate> _loaders = new HashSet<IValidate>();

    public Dictionary<string, Data.TextData> TextDict { get; private set; } = new Dictionary<string, Data.TextData>();
    public Dictionary<int, Data.HeroSkillData> HeroSkillDataDic { get; set; } = new Dictionary<int, Data.HeroSkillData>();
    public Dictionary<int, Data.BuddySkillData> BuddySkillDataDic { get; set; } = new Dictionary<int, Data.BuddySkillData>();
    public Dictionary<int, Data.EffectData> EffectDataDic { get; set; } = new Dictionary<int, Data.EffectData>();
    public Dictionary<int, Data.StageData> StageDataDic { get; set; } = new Dictionary<int, Data.StageData>();
    public Dictionary<int, Data.MonsterData> MonsterDataDic { get; set; } = new Dictionary<int, Data.MonsterData>();
    public Dictionary<int, Data.ProgressionTypeData> ProgressionTypeDataDic { get; set; } = new Dictionary<int, Data.ProgressionTypeData>();
    public Dictionary<Define.ECurrencyType, Data.CurrencyTypeData> CurrencyTypeDataDic { get; set; } = new Dictionary<Define.ECurrencyType, Data.CurrencyTypeData>();
    public Dictionary<int, Data.BuddyData> BuddyDataDic { get; set; } = new Dictionary<int, Data.BuddyData>();
    public Dictionary<int, Data.HeroData> HeroDataDic { get; set; } = new Dictionary<int, Data.HeroData>();
    public Dictionary<string, Data.HeroGachaData> HeroGachaDataDic { get; set; } = new Dictionary<string, Data.HeroGachaData>();
    public Dictionary<string, Data.CurrencyGachaData> CurrencyGachaDataDic { get; set; } = new Dictionary<string, Data.CurrencyGachaData>();




    public void Init()
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
        Validate();

        Debug.Log("Data Load End");
    }

    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
		TextAsset textAsset = Managers.Resource.Load<TextAsset>($"{path}");
        Loader loader = JsonConvert.DeserializeObject<Loader>(textAsset.text);
        _loaders.Add(loader);
        return loader;
	}

    private bool Validate()
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
