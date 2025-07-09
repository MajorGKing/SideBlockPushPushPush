using Data;
using Spine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeroController : AllyController
{
    public enum EHeroState
    {
        None,
        Idle,
        Attack,
        Reload,
    }

    //private List<SpriteRenderer> _myBlocks;
    //private GameScene _gameScene;

    [SerializeField]
    private EHeroState _currentState;
    public EHeroState currentState
    {
        get { return _currentState; }
        set { _currentState = value; }
    }

    //public List<BuddySkillData> buddySkillDatas;
    public List<HeroSkill> skillData;
    private HeroSkill normalSkill;

    protected override void Init()
    {
        base.Init();

        GameObjectType = Define.EGameObjectType.Hero;
        //buddySkillDatas = new List<BuddySkillData>();
        skillData = new List<HeroSkill>();
    }

    public void SetInfo(int templateID)//, List<SpriteRenderer> blockSet, GameScene game)
    {
        //_myBlocks = blockSet;
        //_gameScene = game;

        // TODO ������ �ҷ��ͼ� ��������Ʈ ��Ʈ ��������
        skillData.Add(new HeroSkill(this, 1));
        skillData.Add(new HeroSkill(this, 2));
        skillData.Add(new HeroSkill(this, 3));

        normalSkill = skillData
            .Where(skill => skill.skillData.IconImageKey != null && skill.skillData.IconImageKey.Count == 0)
            .First();
    }

    public override void SetStartAI(bool start)
    {
        _doWork = start;
        PlayAnimation(0, ANIMATION_IDLE, true);
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
        PlayAnimation(0, ANIMATION_ATTACK, false);
    }

    //private void UpdateReload()
    //{
    //}

    public void AddBlock(BuddySkillData skillData)
    {
        foreach (var block in _myBlocks)
        {
            if(block.sprite == null)
            {
                var blockSprite = Managers.Resource.Load<Sprite>(skillData.IconImageKey);
                // Scale it to fit 1x1 world units
                if (blockSprite != null)
                {
                    block.sprite = blockSprite;

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

    public override void DoAttack()
    {
        if(currentState == EHeroState.Idle)
        {
            currentState = EHeroState.Attack;
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
            PlayAnimation(0, ANIMATION_IDLE, true);
            _isWaitingAttack = false;
        }
    }

    // TODO �ҷ� ���ڿ� ���� ���� �ʿ�
    private void Attack()
    {
        // 1. ����� ��������Ʈ �̸� �����ϱ�
        List<string> spriteNames = _myBlocks
                                        .Where(block => block.sprite != null)
                                        .Select(block => block.sprite.name)
                                        .ToList();

        // 2. ���� ��ų ����Ʈ ��
        var activeSkills = skillData
            .Where(skill => skill.skillData.IconImageKey != null && skill.skillData.IconImageKey.Count > 0)
            .ToList();

        // 3. ���� ���� ��ų ã��(���� ��ġ�� �� ��ų�� ã��)
        HeroSkill bestSkill = null;
        int longestMatch = 0;

        foreach (var skill in activeSkills)
        {
            if (IsSubsequence(skill.skillData.IconImageKey, spriteNames))
            {
                if (skill.skillData.IconImageKey.Count > longestMatch)
                {
                    bestSkill = skill;
                    longestMatch = skill.skillData.IconImageKey.Count;
                }
            }
        }

        // 4. longestMatch == 0 �̸� ��ֽ�ų�� �ƴϸ� bestSkill�۵�
        if(longestMatch == 0 && normalSkill != null)
        {
            normalSkill.UseSkill();
        }
        else
        {
            bestSkill.UseSkill();
        }    

        // ��� �̹��� ����
        foreach (var block in _myBlocks)
        {
            if(block.sprite != null)
            {
                block.sprite = null;
            }
        }
    }

    #region Helper
    private bool IsSubsequence(List<string> pattern, List<string> sequence)
    {
        int patternIndex = 0;
        int sequenceIndex = 0;

        while (patternIndex < pattern.Count && sequenceIndex < sequence.Count)
        {
            if (pattern[patternIndex] == sequence[sequenceIndex])
                patternIndex++;
            sequenceIndex++;
        }

        return patternIndex == pattern.Count;
    }
    #endregion
}
