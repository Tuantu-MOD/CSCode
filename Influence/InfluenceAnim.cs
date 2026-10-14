using UnityEngine;
using Spine;
using Spine.Unity;

public class InfluenceAnim : CreatureAnimScript
{
    private Influence influence;
    private new SkeletonAnimation animator;

    public void SetScript(Influence _influence)
    {
        influence = _influence;
        animator = base.gameObject.GetComponent<SkeletonAnimation>();
        Idle();
    }

    public void Idle()
    {
        animator.AnimationState.SetAnimation(0, "Idle", true);
    }

    public void Escape_Idle()
    {
        animator.AnimationState.SetAnimation(0, "Escape_Idle", true);
    }

    public void PlayDead()
    {
        animator.AnimationState.SetAnimation(0, "Dead", false);
    }
}