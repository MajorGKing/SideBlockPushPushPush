using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class GameData
{
    public int UserLevel = 1;
    public string UserName = "Player";

    public int[] Currencies = new int[Enum.GetNames(typeof(Define.ECurrencyType)).Length];

    public int Stamina = Define.MAX_STAMINA;

    public List<HeroController> Heroes = new List<HeroController>();
    public List<BuddyController> Buddies = new List<BuddyController>();

    public int CurrentStageTemplateId = 1;
    public Dictionary<int, StageClear> StageClears = new Dictionary<int, StageClear>();

    public bool BGMOn = true;
    public bool EffectSoundOn = true;
}

[Serializable]
public class StageClear
{
    public int TemplateId;
    public bool isEnable;
    public bool isClear;
}

public class GameManager
{
    string _path;

    #region GameData
    private GameData _gameData = new GameData();

    public int GetCurrency(Define.ECurrencyType currencyType)
    {
        return _gameData.Currencies[(int)currencyType];
    }

    public void SetCurrency(Define.ECurrencyType currencyType, int value)
    {
        _gameData.Currencies[(int)currencyType] = value;
        SaveGame();
        OnCurrenciesChagned?.Invoke();
    }

    public void AddCurrency(Define.ECurrencyType currencyType, int value)
    {
        _gameData.Currencies[(int)currencyType] += value;
        SaveGame();
        OnCurrenciesChagned?.Invoke();
    }

    public int Stamina
    {
        get { return _gameData.Stamina; }
        set
        {
            _gameData.Stamina = value;
            SaveGame();
            OnCurrenciesChagned?.Invoke();
        }
    }

    #endregion

    #region Action
    public event Action OnCurrenciesChagned;
    public event Action OnCurrentStageChanged;
    #endregion

    private GameScene _scene;
    private bool _nowGameScene = false;

    private HeroController _hero;
    public HeroController hero
    {
        get { return _hero; }
    }
    private List<BuddyController> _buddies;
    public List<BuddyController> buddies
    {
        get { return _buddies; }
    }

    private int _stageTemplateId;
    public int stageTemplateId
    {
        get { return _stageTemplateId; }
        set 
        {
            if (value == 0)
                return;

            if (_gameData.StageClears.ContainsKey(value) == false)
                return;

            if (_gameData.StageClears[value].isEnable == false)
                return;

            _stageTemplateId = value;
            _gameData.CurrentStageTemplateId = value;
            OnCurrentStageChanged?.Invoke();
            SaveGame();
        }
    }

    //private int _world;
    //public int world
    //{
    //    get { return _world; }
    //    private set { _world = value; }
    //}
    //private int _stage;
    //public int stage
    //{
    //    get { return _stage; }
    //    private set { _stage = value; }
    //}
    //private Define.EDifficultyLevel _difficultyLevel;
    //public Define.EDifficultyLevel difficultyLevel
    //{
    //    get { return _difficultyLevel; }
    //    private set { _difficultyLevel = value; }
    //}

    public void Init()
    {
        _path = Application.persistentDataPath + "/SaveData.json";
        //world = 1;
        //stage = 1;
        //difficultyLevel = Define.EDifficultyLevel.Normal;

        if (LoadGame())
            return;


        StageClear stage = new StageClear();
        stage.TemplateId = 1;
        stage.isEnable = true;
        stage.isClear = false;
        _gameData.StageClears.Add(1, stage);


        PlayerPrefs.SetInt("ISFIRST", 0);
        //PlayerPrefs.Save();

        stageTemplateId = _gameData.CurrentStageTemplateId;//GetLastClearedNormalStageTemplateId(defaultValue: 1);
    }

    public void Update()
    {
        if (_scene == null)
            return;

        if (_nowGameScene == false)
            return;

        // 입력 처리
        UpdateInput();
    }

    public void GameSceneStart(GameScene scene)
    {
        _scene = scene;
        _nowGameScene = true;
    }

    public void GameSceneEnd()
    {
        _scene = null;
        _nowGameScene = false;
    }

    private void UpdateInput()
    {
        if (IsPointerOverUIObject(Input.mousePosition))
            return;

        if (Input.GetMouseButtonDown(0))
        {
            
        }
        else if (Input.GetMouseButtonUp(0))
        {
            //Debug.Log("Touch Position: " + Input.mousePosition);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null)
            {
                if (hit.transform.TryGetComponent<LineTouchController>(out LineTouchController lineTouch))
                {
                    var lineNum = lineTouch.LineTouched();
                    _scene.LineTouched(lineNum);
                }
            }
        }
    }

    public bool IsPointerOverUIObject(Vector2 touchPos)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = touchPos;
        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0;
    }

    //public int GetLastClearedNormalStageTemplateId(int defaultValue = 1)
    //{
    //    for (int i = _gameData.StageClears.Count - 1; i >= 0; i--)
    //    {
    //        var stageClear = _gameData.StageClears[i];
    //        if (!stageClear.isClear)
    //            continue;

    //        if (Managers.Data.StageDataDic.TryGetValue(stageClear.TemplateId, out var stageData))
    //        {
    //            if (stageData.DifficultyLevel == Define.EDifficultyLevel.Normal)
    //            {
    //                return stageClear.TemplateId;
    //            }
    //        }
    //    }

    //    return defaultValue;
    //}

    #region Reward
    public List<Reward> GetRewards()
    {
        List<Reward> rewards = new List<Reward>();
        var stageData = Managers.Data.StageDataDic[stageTemplateId];

        int enumCount = Enum.GetNames(typeof(Define.ECurrencyType)).Length;
        List<int> currencyCounts = new List<int>(new int[enumCount]);

        System.Random _random = new System.Random();

        for(int i = 0; i < stageData.RewardTimes; i++)
        {
            int totalWeight = 0;
            foreach (int weight in stageData.RewardPercent)
                totalWeight += weight;

            int rand = _random.Next(0, totalWeight);
            int cumulative = 0;

            for (int j = 0; j < stageData.RewardPercent.Count; j++)
            {
                cumulative += stageData.RewardPercent[j];
                if (rand < cumulative)
                {
                    Define.ECurrencyType currencyType = stageData.RewardType[j];
                    int rewardCount = stageData.RewardCount[j];
                    currencyCounts[(int)currencyType] += rewardCount;
                    break;
                }
            }
        }

        for(int i = 0; i < currencyCounts.Count; i++)
        {
            if(currencyCounts[i] == 0)
                continue;

            rewards.Add(new Reward((Define.ECurrencyType)i, currencyCounts[i]));
        }

        if (_gameData.StageClears[stageTemplateId].isClear == false)
        {
            for(int i = 0; i < stageData.RewardFirstType.Count; i++)
            {
                rewards.Add(new Reward(stageData.RewardFirstType[i], stageData.RewardFirstCount[i], true));
            }
        }

        return rewards;
    }
    #endregion

    public void ClearStage()
    {
        _gameData.StageClears[stageTemplateId].isClear = true;
        if(_gameData.StageClears.ContainsKey(Managers.Data.StageDataDic[stageTemplateId].NextaStageId) == false)
        {
            var newStage = new StageClear();
            newStage.TemplateId = Managers.Data.StageDataDic[stageTemplateId].NextaStageId;
            newStage.isClear = false;
            newStage.isEnable = true;

            _gameData.StageClears.Add(newStage.TemplateId, newStage);
            stageTemplateId = newStage.TemplateId;
        }

        _gameData.CurrentStageTemplateId = stageTemplateId;

        SaveGame();
    }

    #region SaveLoad
    public void SaveGame()
    {
        string jsonStr = JsonConvert.SerializeObject(_gameData);
        File.WriteAllText(_path, jsonStr);

        Debug.Log("Save Sucess");
    }

    public bool LoadGame()
    {
        if (PlayerPrefs.GetInt("ISFIRST", 1) == 1)
        {
            string path = Application.persistentDataPath + "/SaveData.json";
            if (File.Exists(path))
                File.Delete(path);
            return false;
        }

        if (File.Exists(_path) == false)
            return false;

        string fileStr = File.ReadAllText(_path);
        GameData data = JsonConvert.DeserializeObject<GameData>(fileStr);
        if (data != null)
            _gameData = data;

        //IsLoaded = true;

        stageTemplateId = _gameData.CurrentStageTemplateId;

        Debug.Log("Loading Sucess");
        return true;
    }
    #endregion

}