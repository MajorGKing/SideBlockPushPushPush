using Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class GameScene : BaseScene
{
    private HeroController _heroController;
    private List<BuddyController> _buddyControllers;

    public List<SpriteRenderer> blockImages1;
    public List<SpriteRenderer> blockImages2;
    public List<SpriteRenderer> blockImages3;
    public List<SpriteRenderer> blockImages4;
    public List<SpriteRenderer> stockImages;

    private List<List<SpriteRenderer>> blockImages;

    public List<Transform> monsterPosition;
    public List<Transform> buddyPosition;
    public Transform heroPosition;
    

    private List<MonsterController> _monsterControllers;

    private UI_GameScene _gameSceneUI;
    private bool _isAuto;

    private StageData _stageData;
    public StageData StageData => _stageData;
    private int _stageWaveIndex = 0;
    public int StageWaveIndex
    {
        get { return _stageWaveIndex; }
        set
        {
            if (_stageWaveIndex == value)
            {
                return;
            }

            _stageWaveIndex = value;

            Managers.Object.RemoveAllMonsters();
            SpawnMonsterByWaveIndex(_stageWaveIndex);

            Managers.Event.TriggerEvent(EEventType.OnStageWaveIndexChanged);
        }
    }


    protected override void Awake()
    {
        base.Awake();

#if UNITY_EDITOR
        gameObject.AddComponent<CaptureScreenShot>();
#endif

        Debug.Log("@>> GameScene Init()");
        SceneType = Define.EScene.GameScene;

        // 게임상 터치 입력 시작하도록
        Managers.Game.GameSceneStart(this);

        // Hero, Buddy 추가
        _heroController = Managers.Object.SpawnCreatureObject<HeroController>(heroPosition, 0);
        _heroController.SetBlocks(stockImages);


        _buddyControllers = new List<BuddyController>();
        for (int i = 0; i < 4; i++)
        {
            var buddy = Managers.Object.SpawnCreatureObject<BuddyController>(buddyPosition[i], i);
            _buddyControllers.Add(buddy);
        }
        

        // 라인블록, 스톡블록 추가 TODO
        // 나중에 코드로 자동으로 할 수 있도록
        blockImages = new List<List<SpriteRenderer>> { blockImages1, blockImages2, blockImages3, blockImages4 };

        for(int i = 0; i < _buddyControllers.Count; i++)
        {
            _buddyControllers[i].SetBlocks(blockImages[i]);
        }

        _monsterControllers = new List<MonsterController>();

        //var monster = Managers.Resource.Instantiate("Monster", monsterPosition[0]);
        var monster = Managers.Object.SpawnCreatureObject<MonsterController>(monsterPosition[0], 0);
        monster.transform.position = monsterPosition[0].position;
        _monsterControllers.Add(monster);

        _gameSceneUI = Managers.UI.ShowSceneUI<UI_GameScene>();
        _gameSceneUI.SetInfo(_isAuto, this);
    }

    protected override void Start()
    {
        base.Start();

        foreach (var buddy in _buddyControllers)
        {
            buddy.SetStartAI(true);
            buddy.SetAuto(_isAuto);
        }
        _heroController.SetStartAI(true);
        _heroController.SetAuto(_isAuto);
    }

    public void LineTouched(int lineNumber)
    {
        Debug.Log($"Line Number {lineNumber}");

        if(lineNumber == Define.HEROLINENUMBHER)
        {
            _heroController.DoAttack();
        }
        else
        {
            _buddyControllers[lineNumber].DoAttack();
        }
    }

    public void SetAuto()
    {
        _isAuto = !_isAuto;

        foreach (var buddy in _buddyControllers)
        {
            buddy.SetAuto(_isAuto);
        }

        _heroController.SetAuto(_isAuto);

        _gameSceneUI.SetAutoUI(_isAuto);
    }
    
    public override void Clear()
    {
        Managers.Game.GameSceneEnd();
        Managers.Object.Clear();
    }

    protected virtual void SpawnMonsterByWaveIndex(int waveIndex)
    {
        switch (waveIndex)
        {
            case 1:
                SpawnMonsters(_stageData.FirstWaveMonsterList, _stageData.FirstWaveMonsterLevelList);
                break;
            case 2:
                SpawnMonsters(_stageData.SecondWaveMonsterList, _stageData.SecondWaveMonsterLevelList);
                break;
            case 3:
                SpawnMonsters(_stageData.BossWaveMonsterList, _stageData.BossWaveMonsterLevelList);
                break;
            default:
                break;
        }
    }

    protected void SpawnMonsters(List<int> monsterList, List<int> monsterLevel)
    {
        int spawnIndex = 0;
    }
}