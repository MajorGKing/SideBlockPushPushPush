using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameScene : BaseScene
{
    public Sprite sprite_sheet;

    private HeroController _heroController;
    private List<BuddyController> _buddyControllers;

    public List<SpriteRenderer> blockImages1;
    public List<SpriteRenderer> blockImages2;
    public List<SpriteRenderer> blockImages3;
    public List<SpriteRenderer> blockImages4;
    public List<SpriteRenderer> stockImages;

    private List<List<SpriteRenderer>> blockImages;

    public List<Transform> monsterPosition;

    private List<MonsterController> _monsterControllers;

    private UI_GameScene _gameSceneUI;
    private bool _isAuto;

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
        _heroController = GameObject.FindGameObjectWithTag(Define.HEROTAG).GetComponent<HeroController>();
        var buddies = GameObject.FindGameObjectsWithTag(Define.BUDDYTAG);

        _buddyControllers = Enumerable.Repeat<BuddyController>(null, 4).ToList();
        foreach (var buddy in buddies)
        {
            string name = buddy.transform.parent.name;
            char lastChar = name[name.Length - 1];
            if (char.IsDigit(lastChar))
            {
                int index = (int)char.GetNumericValue(lastChar);
                _buddyControllers[index - 1] = buddy.GetComponent<BuddyController>();
            }
        }

        // 라인블록, 스톡블록 추가 TODO
        // 나중에 코드로 자동으로 할 수 있도록
        blockImages = new List<List<SpriteRenderer>> { blockImages1, blockImages2, blockImages3, blockImages4 };


        for(int i = 0; i < _buddyControllers.Count; i++)
        {
            _buddyControllers[i].SetInfo(i, blockImages[i], this);
        }

        _heroController.SetInfo(0, stockImages, this);

        _monsterControllers = new List<MonsterController>();

        var monster = Managers.Resource.Instantiate("Monster", monsterPosition[0]);
        monster.transform.position = monsterPosition[0].position;
        var mc = monster.transform.GetComponent<MonsterController>();
        _monsterControllers.Add(mc);
        _monsterControllers[0].SetInfo(this);

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

    public void BuddyAttack(Sprite block)
    {
        _heroController.AddBlock(block);
        _monsterControllers[0].OnDamage(0, 10);
    }

    public void HeroAttack()
    {
        _monsterControllers[0].OnDamage(1, 30);
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
    }
}