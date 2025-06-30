using Spine;
using System.Collections.Generic;
using UnityEngine;

public class BuddyController : CreatureController
{
    public enum EBuddyState
    {
        None,
        Idle,
        Attack,
        Reload,
    }

    private List<SpriteRenderer> _myBlocks;
    private int _buddyNumber;
    private GameScene _gameScene;

    private bool _doWork;
    private bool _auto = true;

    [SerializeField]
    private float _coolTime;
    [SerializeField]
    private float _currentCoolTime;

    [SerializeField]
    private float _reloadTime;
    [SerializeField]
    private float _currentReloadTime;

    public List<Sprite> blockImages;

    [SerializeField]
    private EBuddyState _currentBuddyState = EBuddyState.None;
    public EBuddyState currentBuddyState
    {
        get { return _currentBuddyState; }
        set { _currentBuddyState = value; }
    }
    

    protected override void Init()
    {
        base.Init();


    }

    // TODO 이름 변경 필요
    // TODO 이후 번호 받아 갱신하는거 필요
    public void SetInfo(int num, List<SpriteRenderer> blockSet, GameScene game)
    {
        _buddyNumber = num;
        _myBlocks = blockSet;
        _gameScene = game;

        // TODO 데이터 불러와서 스프라이트 세트 가저오기
    }

    public void SetStartAI(bool start)
    {
        _doWork = start;
        currentBuddyState = EBuddyState.Idle;
        _currentCoolTime = _coolTime;

        ReloadBlocks();
    }

    private void Update()
    {
        if (_doWork == false)
            return;

        switch(currentBuddyState)
        {
            case EBuddyState.Idle:
                UpdateIdle();
                break;
            case EBuddyState.Attack:
                UpdateAttack(); 
                break;
            case EBuddyState.Reload:
                UpdateReload();
                break;
            default:
                break;
        }
    }

    private void UpdateIdle()
    {
        if (_currentCoolTime >= 0)
        {
            _currentCoolTime -= Time.deltaTime;
            return;
        }

        if (_auto == true)
        {
            currentBuddyState = EBuddyState.Attack;
        }
    }

    private void UpdateAttack()
    {
        if (_currentCoolTime > 0)
        {
            return;
        }

        PlayAnimation(0, "attack", false);

        _currentCoolTime = _coolTime;
    }

    private void UpdateReload()
    {
        if(_currentReloadTime > 0)
        {
            _currentReloadTime -= Time.deltaTime;
            return;
        }

        ReloadBlocks();

        currentBuddyState = EBuddyState.Idle;
    }

    public override void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        Attack();
    }

    public override void OnAnimCompleteHandler(TrackEntry trackEntry)
    {
        if(currentBuddyState == EBuddyState.Attack)
        {
            if (_myBlocks[0].sprite == null)
            {
                PlayAnimation(0, "move", true);
                currentBuddyState = EBuddyState.Reload;
                _currentReloadTime = _reloadTime;
            }
            else
            {
                PlayAnimation(0, "idle", true);
                currentBuddyState = EBuddyState.Idle;
                _currentCoolTime = _coolTime;
            }
        }
    }

    private void ReloadBlocks()
    {
        foreach (var block in _myBlocks)
        {
            int randomIndex = Random.Range(0, blockImages.Count);
            Sprite selectedSprite = blockImages[randomIndex];
            block.sprite = selectedSprite;

            // Resize to fit 1x1 world units
            Vector2 spriteSize = selectedSprite.bounds.size; // World units size of sprite

            if (spriteSize.x != 0 && spriteSize.y != 0)
            {
                // Compute required scale to make it 1x1
                Vector3 scale = block.transform.localScale;
                scale.x = 1f / spriteSize.x;
                scale.y = 1f / spriteSize.y;
                block.transform.localScale = scale;
            }
        }
    }

    private void Attack()
    {
        if (_myBlocks[0].sprite == null)
            return;

        Sprite firstBlock = _myBlocks[0].sprite;

        // Shift sprites forward
        for (int i = 1; i < _myBlocks.Count; i++)
        {
            Sprite nextSprite = _myBlocks[i].sprite;
            _myBlocks[i - 1].sprite = nextSprite;

            // Re-scale to fit 1x1 world units
            if (nextSprite != null)
            {
                Vector2 size = nextSprite.bounds.size;
                if (size.x != 0 && size.y != 0)
                {
                    Vector3 scale = _myBlocks[i - 1].transform.localScale;
                    scale.x = 1f / size.x;
                    scale.y = 1f / size.y;
                    _myBlocks[i - 1].transform.localScale = scale;
                }
            }
        }

        // Clear the last block
        SpriteRenderer lastBlock = _myBlocks[_myBlocks.Count - 1];
        lastBlock.sprite = null;
        lastBlock.transform.localScale = Vector3.one; // Reset scale just in case

        _gameScene.BuddyAttack(firstBlock);
    }

}
