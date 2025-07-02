using Spine;
using Spine.Unity;
using UnityEngine;

public class CreatureController : BaseController
{
    private const string ANIMATION_IDLE = "idle";
    private const string ANIMATION_MOVE = "move";
    private const string ANIMATION_HIT = "damage";
    private const string ANIMATION_DEAD = "dead";

    public SkeletonAnimation SkeletonAnimation { get; private set; }

    protected override void Init()
    {
        SkeletonAnimation = GetComponentInChildren<SkeletonAnimation>();

        // Bind Event
        SkeletonAnimation.AnimationState.Event -= OnAnimEventHandler;
        SkeletonAnimation.AnimationState.Event += OnAnimEventHandler;
        SkeletonAnimation.AnimationState.Complete -= OnAnimCompleteHandler;
        SkeletonAnimation.AnimationState.Complete += OnAnimCompleteHandler;
    }


    public void PlayAnimation(int trackIndex, string animName, bool loop)
    {
        if (SkeletonAnimation == null)
        {
            return;
        }

        SkeletonAnimation.AnimationState.SetAnimation(trackIndex, animName, loop);
    }

    public virtual void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        // ���⼭ ���� �� �� ó���� �ڵ� �ۼ�

    }

    public virtual void OnAnimCompleteHandler(TrackEntry trackEntry)
    {
        // �ִϸ��̼� ������ �� �� ����
        //if (AIObjectState == EAIObjectState.Attack || AIObjectState == EAIObjectState.Hit)
        //{
        //    IsAttackComplete = AIObjectState == EAIObjectState.Attack ? true : false;
        //    PlayAnimation(0, ANIMATION_IDLE, true);
        //}
    }

    public virtual void DoAttack()
    {

    }
}
