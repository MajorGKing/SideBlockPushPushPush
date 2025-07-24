using Spine;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;

public class BuddyController : AllyController
{
    public enum EBuddyState
    {
        None,
        Idle,
        Attack,
        Reload,
        Wait,
    }

    private List<int> _nowBlockList;
    //private GameScene _gameScene;

    public int templateId;

    private List<BuddySkill> _skillList;
    
    [SerializeField]
    private float _coolTime;
    [SerializeField]
    private float _currentCoolTime;
    public float currentCoolTime
    {
        get { return _currentCoolTime; }
        set
        {
            if( _currentCoolTime == value )
                return;

            _currentCoolTime = value;

            if( _coolTime <= 0 )
            {
                _coolTime = 0;
            }

            _battleBarUI.SetInfo(_coolTime - currentCoolTime, _coolTime);
        }
    }

    [SerializeField]
    private float _reloadTime;
    [SerializeField]
    private float _currentReloadTime;
    public float currentReloadTime
    {
        get { return _currentReloadTime; }
        set
        {
            if(_currentReloadTime == value )
                return;
            
            _currentReloadTime = value;

            if(_currentReloadTime <= 0 )
            {
                _currentReloadTime = 0;
            }

            _battleBarUI.SetInfo(_reloadTime - currentReloadTime, _reloadTime);
        }
    }

    //public List<Sprite> blockImages;

    private UI_BattleBarWorldSpace _battleBarUI;

    [SerializeField]
    private EBuddyState _currentBuddyState = EBuddyState.None;
    public EBuddyState currentBuddyState
    {
        get { return _currentBuddyState; }
        set 
        {
            _currentBuddyState = value;

            //OnChangedState();
        }
    }

    //private void OnChangedState()
    //{
    //    UpdateAnimation();
    //}

    protected override void Init()
    {
        base.Init();

        _battleBarUI = GetComponentInChildren<UI_BattleBarWorldSpace>();

        GameObjectType = Define.EGameObjectType.Buddy;
    }

    // TODO 이름 변경 필요
    // TODO 이후 번호 받아 갱신하는거 필요
    public void SetInfo(int num)//, List<SpriteRenderer> blockSet)//, GameScene game)
    {
        templateId = num;

        _skillList = new List<BuddySkill> { };
        _nowBlockList = new List<int>();

        // TODO 데이터 불러와서 스프라이트 세트 가저오기
        // TODO Buddy Data 만들기
        if (templateId == 0)
        {
            skeletonAnimation.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>("spi_buddy_tom_SkeletonData");
            skeletonAnimation.Initialize(true);

            _skillList.Add(new BuddySkill(this, 5));
            _skillList.Add(new BuddySkill(this, 5));
            _skillList.Add(new BuddySkill(this, 6));

            AnimationBindEventInit();
        }
        else if (templateId == 1)
        {
            skeletonAnimation.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>("spi_buddy_mari_SkeletonData");
            skeletonAnimation.Initialize(true);

            _skillList.Add(new BuddySkill(this, 7));

            AnimationBindEventInit();
        }
        else if (templateId == 2)
        {
            skeletonAnimation.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>("spi_buddy_ellie_SkeletonData");
            skeletonAnimation.Initialize(true);

            _skillList.Add(new BuddySkill(this, 3));
            _skillList.Add(new BuddySkill(this, 3));
            _skillList.Add(new BuddySkill(this, 3));
            _skillList.Add(new BuddySkill(this, 3));
            _skillList.Add(new BuddySkill(this, 4));

            AnimationBindEventInit();
        }
        else if (templateId == 3)
        {
            skeletonAnimation.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>("spi_buddy_duck_SkeletonData");
            skeletonAnimation.Initialize(true);

            _skillList.Add(new BuddySkill(this, 1));
            _skillList.Add(new BuddySkill(this, 2));

            AnimationBindEventInit();
        }
    }

    public override void SetStartAI(bool start)
    {
        _doWork = start;
        currentBuddyState = EBuddyState.Idle;
        currentCoolTime = _coolTime;
        _isWaitingAttack = false;

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
            case EBuddyState.Wait:
                UpdateWait();
                break;
            default:
                break;
        }
    }

    private void UpdateIdle()
    {
        if (currentCoolTime > 0)
        {
            currentCoolTime -= Time.deltaTime;
            return;
        }

        if (_auto == true)
        {
            currentBuddyState = EBuddyState.Attack;
        }
    }

    private void UpdateAttack()
    {
        if (currentCoolTime > 0)
            return;

        if (_isWaitingAttack == true)
            return;

        PlayAnimation(0, _skillList[_nowBlockList[0]].skillData.AnimName, false);
        _isWaitingAttack = true;
    }

    private void UpdateReload()
    {
        if(currentReloadTime > 0)
        {
            currentReloadTime -= Time.deltaTime;

            return;
        }


        _battleBarUI.SetInfo(1f, 1f);
        ReloadBlocks();

        currentBuddyState = EBuddyState.Idle;
    }

    private void UpdateWait()
    {
        currentCoolTime = 0;

        if (skeletonAnimation.AnimationState.GetCurrent(0).Animation.Name == ANIMATION_IDLE)
            return;

        PlayAnimation(0, ANIMATION_IDLE, true);
        _isWaitingAttack = false;
    }

    public override void DoAttack()
    {
        if(currentBuddyState == EBuddyState.Idle && currentCoolTime <= 0)
        {
            currentBuddyState = EBuddyState.Attack;
        }
    }

    public override void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        Attack();
    }

    public override void OnAnimCompleteHandler(TrackEntry trackEntry)
    {
        if(currentBuddyState == EBuddyState.Attack)
        {
            if (trackEntry.Animation.Name != ANIMATION_ATTACK)
                return;

            _isWaitingAttack = false;
            if (_myBlocks[0].sprite == null)
            {
                PlayAnimation(0, ANIMATION_MOVE, true);
                currentBuddyState = EBuddyState.Reload;
                currentReloadTime = _reloadTime;
            }
            else
            {
                PlayAnimation(0, ANIMATION_IDLE, true);
                currentBuddyState = EBuddyState.Idle;
                currentCoolTime = _skillList[_nowBlockList[0]].skillData.Cooltime;
                _coolTime = _skillList[_nowBlockList[0]].skillData.Cooltime;
                _nowBlockList.RemoveAt(0);
            }
        }
    }

    private void ReloadBlocks()
    {
        _nowBlockList.Clear();

        foreach (var block in _myBlocks)
        {
            int randomIndex = Random.Range(0, _skillList.Count);
            Sprite selectedSprite = Managers.Resource.Load<Sprite>(_skillList[randomIndex].skillData.IconImageKey);
            block.sprite = selectedSprite;

            _nowBlockList.Add(randomIndex);

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
        PlayAnimation(0, ANIMATION_IDLE, true);
    }

    private void Attack()
    {
        if (_myBlocks[0].sprite == null)
            return;

        Sprite firstBlock = _myBlocks[0].sprite;
        int blockId = _nowBlockList[0];

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

        _skillList[blockId].UseSkill();
    }

    //private void UpdateAnimation()
    //{
    //    switch(currentBuddyState)
    //    {
    //        case EBuddyState.Idle:
    //            PlayAnimation(0, ANIMATION_IDLE, true);
    //            break;
    //        case EBuddyState.Attack:
    //            PlayAnimation(0, ANIMATION_ATTACK, false);
    //            break;
    //        case EBuddyState.Reload:
    //            PlayAnimation(0, ANIMATION_MOVE, true);
    //            break;

    //        default:
    //            break;
    //    }
    //}
}
