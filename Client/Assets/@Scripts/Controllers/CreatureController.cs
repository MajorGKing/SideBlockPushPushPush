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
        // 여기서 공격 할 때 처리할 코드 작성

    }

    public virtual void OnAnimCompleteHandler(TrackEntry trackEntry)
    {
        // 애니메이션 끝났을 때 할 동작
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
