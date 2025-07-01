using Spine;
using System.Collections.Generic;
using UnityEngine;
using static BuddyController;

public class HeroController : CreatureController
{
    public enum EHeroState
    {
        None,
        Idle,
        Attack,
        Reload,
    }

    private List<SpriteRenderer> _myBlocks;
    private GameScene _gameScene;

    private bool _doWork;
    private bool _auto = true;

    private bool _isWaitingAttack;

    [SerializeField]
    private EHeroState _currentState;
    public EHeroState currentState
    {
        get { return _currentState; }
        set { _currentState = value; }
    }

    protected override void Init()
    {
        base.Init();
    }

    public void SetInfo(int num, List<SpriteRenderer> blockSet, GameScene game)
    {
        _myBlocks = blockSet;
        _gameScene = game;

        // TODO 데이터 불러와서 스프라이트 세트 가저오기
    }

    public void SetStartAI(bool start)
    {
        _doWork = start;
        PlayAnimation(0, "idle", true);
        currentState = EHeroState.Idle;
        _isWaitingAttack = false;
    }

    private void Update()
    {
        if (_doWork == false)
            return;

        switch (currentState)
        {
            case EHeroState.Idle:
                UpdateIdle();
                break;
            case EHeroState.Attack:
                UpdateAttack();
                break;
            //case EHeroState.Reload:
            //    UpdateReload();
            //    break;
            default:
                break;
        }
    }


    private void UpdateIdle()
    {
        if(_auto == true)
        {
            if(_myBlocks[_myBlocks.Count - 1].sprite != null)
            {
                currentState = EHeroState.Attack;
            }
        }
    }

    private void UpdateAttack()
    {
        if(_isWaitingAttack == true) 
            return;

        _isWaitingAttack = true;
        PlayAnimation(0, "attack", false);
    }

    //private void UpdateReload()
    //{
    //}

    public void AddBlock(Sprite blockSprite)
    {
        foreach (var block in _myBlocks)
        {
            if(block.sprite == null)
            {
                block.sprite = blockSprite;
                // Scale it to fit 1x1 world units
                if (blockSprite != null)
                {
                    Vector2 spriteSize = blockSprite.bounds.size;
                    if (spriteSize.x != 0 && spriteSize.y != 0)
                    {
                        Vector3 scale = block.transform.localScale;
                        scale.x = 1f / spriteSize.x;
                        scale.y = 1f / spriteSize.y;
                        block.transform.localScale = scale;
                    }
                }
                return;
            }
        }
    }


    public override void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        Attack();
    }

    public override void OnAnimCompleteHandler(TrackEntry trackEntry)
    {
        if(currentState == EHeroState.Attack)
        {
            currentState = EHeroState.Idle;
            PlayAnimation(0, "idle", true);
            _isWaitingAttack = false;
        }
    }

    private void Attack()
    {
        _gameScene.HeroAttack();

        foreach (var block in _myBlocks)
        {
            block.sprite = null;
        }
    }


}
