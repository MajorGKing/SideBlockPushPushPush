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
    }

    protected override void Start()
    {
        base.Start();

        foreach (var buddy in _buddyControllers)
        {
            buddy.SetStartAI(true);
        }
        _heroController.SetStartAI(true) ;
    }

    public void LineTouched(int lineNumber)
    {
        Debug.Log($"Line Number {lineNumber}");
    }

    public void BuddyAttack(Sprite block)
    {
        _heroController.AddBlock(block);
    }
    
    public override void Clear()
    {
        Managers.Game.GameSceneEnd();
    }
}