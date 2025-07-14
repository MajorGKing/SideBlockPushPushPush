using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager
{
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
        protected set { _stageTemplateId = value; }
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
        //world = 1;
        //stage = 1;
        //difficultyLevel = Define.EDifficultyLevel.Normal;
        stageTemplateId = 1;
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

}