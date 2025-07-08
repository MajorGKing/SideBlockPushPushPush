using Spine;
using Spine.Unity;
using UnityEngine;

public class CreatureController : BaseController
{
    protected const string ANIMATION_IDLE = "idle";
    protected const string ANIMATION_ATTACK = "attack";
    protected const string ANIMATION_MOVE = "move";
    protected const string ANIMATION_DIE = "dead";

    public SkeletonAnimation skeletonAnimation { get; private set; }

    protected override void Init()
    {
        skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
        AnimationBindEventInit();
    }

    protected void AnimationBindEventInit()
    {
        // Bind Event
        skeletonAnimation.AnimationState.Event -= OnAnimEventHandler;
        skeletonAnimation.AnimationState.Event += OnAnimEventHandler;
        skeletonAnimation.AnimationState.Complete -= OnAnimCompleteHandler;
        skeletonAnimation.AnimationState.Complete += OnAnimCompleteHandler;
    }


    public void PlayAnimation(int trackIndex, string animName, bool loop)
    {
        if (skeletonAnimation == null)
        {
            return;
        }

        skeletonAnimation.AnimationState.SetAnimation(trackIndex, animName, loop);
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
