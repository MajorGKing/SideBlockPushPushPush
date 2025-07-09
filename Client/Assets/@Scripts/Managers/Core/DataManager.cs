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

    public Dictionary<int, Data.HeroSkillData> HeroSkillDataDic { get; set; } = new Dictionary<int, Data.HeroSkillData>();
    public Dictionary<int, Data.BuddySkillData> BuddySkillDataDic { get; set; } = new Dictionary<int, Data.BuddySkillData>();
    public Dictionary<int, Data.EffectData> EffectDataDic { get; set; } = new Dictionary<int, Data.EffectData>();

    public void Init()
    {
        HeroSkillDataDic = LoadJson<Data.HeroSkillDataLoader, int, Data.HeroSkillData>("HeroSkillData").MakeDict();
        BuddySkillDataDic = LoadJson<Data.BuddySkillDataLoader, int, Data.BuddySkillData>("BuddySkillData").MakeDict();
        EffectDataDic = LoadJson<Data.EffectDataLoader, int, Data.EffectData>("EffectData").MakeDict();

        
        Validate();
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
