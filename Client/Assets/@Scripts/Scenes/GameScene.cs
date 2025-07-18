using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

    private float _time = 180f;
    public float time
    {
        get { return _time; }
        set
        {
            if (_time != value)
            {
                _time = value;
                if(_gameSceneUI == null) return;

                _gameSceneUI.UpdateTime(_time);
            }
        }
    }

    private StageData _stageData;
    public StageData stageData => _stageData;
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

            Managers.Event.TriggerEvent(Define.EEventType.OnStageWaveIndexChanged);
        }
    }

    private IEnumerator _currentCoroutine = null;
    private Define.EStageState _stageState = Define.EStageState.None;
    public Define.EStageState StageState
    {
        get { return _stageState; }
        set
        {
            if (_stageState == value)
            {
                return;
            }

            _stageState = value;

            SwitchStageCoroutine();
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

        // 라인블록, 스톡블록 추가 TODO
        // 나중에 코드로 자동으로 할 수 있도록
        blockImages = new List<List<SpriteRenderer>> { blockImages1, blockImages2, blockImages3, blockImages4 };

        _monsterControllers = new List<MonsterController>();

        //var monster = Managers.Resource.Instantiate("Monster", monsterPosition[0]);
        //var monster = Managers.Object.SpawnCreatureObject<MonsterController>(monsterPosition[0], 1, 1);
        //monster.transform.position = monsterPosition[0].position;
        //_monsterControllers.Add(monster);

        _gameSceneUI = Managers.UI.ShowSceneUI<UI_GameScene>();
        _gameSceneUI.SetInfo(_isAuto, this);
    }

    // 매번 씬을 로드 하기 때문에 별도의 Init메소드를 만들지 않는다 -> 필요시 만들기
    protected override void Start()
    {
        base.Start();

        // 스테이지 데이터 설정
        _stageData = Managers.Data.StageDataDic[Managers.Game.stageTemplateId];

        if(_stageData == null )
        {
            Debug.LogError("Stage Data Null");
            return;
        }

        StageWaveIndex = 0;

        // 게임상 터치 입력 시작하도록
        Managers.Game.GameSceneStart(this);

        // 히어로 스폰
        SpawnHero();

        // 버디 스폰
        SpanwnBuddies();

        // state 변경
        StageState = Define.EStageState.Start;
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

    private void SpawnHero()
    {
        _heroController = Managers.Object.SpawnCreatureObject<HeroController>(heroPosition, 0);
        _heroController.SetBlocks(stockImages);
    }

    private void SpanwnBuddies()
    {
        _buddyControllers = new List<BuddyController>();
        for (int i = 0; i < 4; i++)
        {
            var buddy = Managers.Object.SpawnCreatureObject<BuddyController>(buddyPosition[i], i);
            _buddyControllers.Add(buddy);
        }
    }

    protected virtual void SpawnMonsterByWaveIndex(int waveIndex)
    {
        switch (waveIndex)
        {
            case 1:
                SpawnMonsters(stageData.FirstWaveMonsterList, stageData.FirstWaveMonsterLevelList);
                break;
            case 2:
                SpawnMonsters(stageData.SecondWaveMonsterList, stageData.SecondWaveMonsterLevelList);
                break;
            case 3:
                SpawnMonsters(stageData.BossWaveMonsterList, stageData.BossWaveMonsterLevelList);
                break;
            default:
                break;
        }
    }

    protected void SpawnMonsters(List<int> monsterList, List<int> monsterLevel)
    {
        int spawnIndex = 0;
        switch (monsterList.Count)
        {
            case 1:
                Managers.Object.SpawnCreatureObject<MonsterController>(monsterPosition[0], monsterList[0], monsterLevel[0]);
                break;
            case 2:
            case 3:
            case 4:
                foreach (int monsterIndex in monsterList)
                {
                    Managers.Object.SpawnCreatureObject<MonsterController>(monsterPosition[spawnIndex + 1], monsterList[spawnIndex], monsterLevel[spawnIndex]);
                    spawnIndex++;
                }
                break;

            default:
                break;
        }
    }

    private void SwitchStageCoroutine()
    {
        IEnumerator coroutine = GetStageCoroutineForState(StageState);
        if (coroutine == null || _currentCoroutine == coroutine)
        {
            return;
        }

        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }

        _currentCoroutine = coroutine;
        StartCoroutine(_currentCoroutine);
    }

    private IEnumerator GetStageCoroutineForState(Define.EStageState state)
    {
        switch (state)
        {
            case Define.EStageState.Start:
                return CoStartState();
            case Define.EStageState.Battle:
                return CoBattleState();
            case Define.EStageState.Move:
                return CoMoveState();
            case Define.EStageState.Over:
                return CoOverState();
            case Define.EStageState.Clear:
                return CoClearState();

            default:
                return null;
        }
    }

    // 맨처음 하는 일 몬스터를 스폰하고 게임 진행
    private IEnumerator CoStartState()
    {
        // 블럭 넘겨주기
        for (int i = 0; i < _buddyControllers.Count; i++)
        {
            _buddyControllers[i].SetBlocks(blockImages[i]);
        }

        // 버디 컨트롤서 시작
        foreach (var buddy in _buddyControllers)
        {
            buddy.SetStartAI(true);
            buddy.SetAuto(_isAuto);
        }

        // 히어로 컨트롤러 시작
        _heroController.SetStartAI(true);
        _heroController.SetAuto(_isAuto);

        // 웨이브 인덱스 변경 => 스폰 시작
        StageWaveIndex = 1;

        yield return new WaitUntil(() => Managers.Object.LivingMonsterList.Count > 0);
        StageState = Define.EStageState.Battle;

        yield return null;
    }

    // 게임 중 전투중인 경우 진행
    private IEnumerator CoBattleState()
    {
        // 모든 캐릭터 idle상태로

        // 배틀 시작
        while (true)
        {
            time -= Time.deltaTime;
            if(time < 0)
            {
                time = 0;
                StageState = Define.EStageState.Over;
                break;
            }
            
            if(Managers.Object.LivingMonsterList.Count == 0)
            {
                StageState = Define.EStageState.Move;
                break;
            }

            yield return null;
        }
    }

    // 웨이브 바뀌는 부분 처리
    protected virtual IEnumerator CoMoveState()
    {
        Managers.Object.SetAllBuddyState(BuddyController.EBuddyState.Wait);

        yield return new WaitForSeconds(3f);

        StageWaveIndex++;

        if (StageWaveIndex >= 4)
        {
            StageState = Define.EStageState.Clear;
            yield return null;
        }

        yield return new WaitUntil(() => Managers.Object.LivingMonsterList.Count > 0);
        StageState = Define.EStageState.Battle;

        Managers.Object.SetAllBuddyState(BuddyController.EBuddyState.Idle);

        yield return null;
    }

    // 게임 클리어
    protected virtual IEnumerator CoClearState()
    {
        var clear = Managers.UI.ShowPopupUI<UI_RewardPopup>();

        // TODO 추후 웹서버를 통해 받는다
        // TODO 추후 개인 => 웹서버에 리워드를 저장한다
        clear.SetInfo(Define.ERewardType.StageClear, Managers.Game.GetRewards());

        // 게임 클리어 세팅
        Managers.Game.ClearStage();

        yield return null;
    }

    // 게임 클리어 실패
    protected virtual IEnumerator CoOverState()
    {
        var clear = Managers.UI.ShowPopupUI<UI_FailPopup>();

        yield return new WaitForSeconds(5f);

        Managers.Scene.LoadScene(Define.EScene.LobbyScene);

        Managers.UI.ClosePopupUI(clear);


        yield return null;
    }
}