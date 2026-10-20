using UnityEngine;
using Spine;
using Spine.Unity;

public class EclipsedOfAceAnim : CreatureAnimScript
{
    private EclipsedOfAce eclipsedOfAce;
    public new SkeletonAnimation animator;

    public bool starting = true;

    public void SetScript(EclipsedOfAce _eclipsedOfAce)
    {
        eclipsedOfAce = _eclipsedOfAce;
        animator = base.gameObject.GetComponent<SkeletonAnimation>();
        Idle();
    }

    public void OnMove()
    {
        animator.AnimationState.SetAnimation(0, "Walk", true);
    }

    public void Idle()
    {
        animator.AnimationState.SetAnimation(0, "Idle", true);
    }

    public void Attack()
    {
        TrackEntry entry = animator.AnimationState.SetAnimation(0, "Stab", false);
        entry.Event += Attack;
        entry.Complete += AttackEnd;
    }
    
    public void attack_bigsword()
    {
        TrackEntry entry = animator.AnimationState.SetAnimation(0, "Slam", false);
        entry.Event += attack_bigsword;
        entry.Complete += AttackEnd;
    }

    public void attack_beam()
    {
        TrackEntry entry = animator.AnimationState.SetAnimation(0, "Slam", false);
        entry.Event += attack_beam;
        entry.Complete += AttackEnd;
    }

    public void Attack(TrackEntry track, Event e)
    {
        if(e.Data.Name == "Damage" && eclipsedOfAce._target != null && eclipsedOfAce.InRange(eclipsedOfAce._target, 1.5f))
        {
            if(!eclipsedOfAce.InView(eclipsedOfAce._target))
                return;
            eclipsedOfAce._target.TakeDamage(eclipsedOfAce.model, new DamageInfo(RwbpType.B, 10, 15));
        }
    }

    public void attack_bigsword(TrackEntry track, Event e)
    {
        if(e.Data.Name == "Damage" && eclipsedOfAce._target != null && eclipsedOfAce.InRange(eclipsedOfAce._target, 1.5f))
        {
            if(!eclipsedOfAce.InView(eclipsedOfAce._target))
                return;
            eclipsedOfAce._target.TakeDamage(eclipsedOfAce.model, new DamageInfo(RwbpType.B, 40, 60));
        }
    }

    public void attack_beam(TrackEntry track, Event e)
    {
        if(e.Data.Name == "Damage" && eclipsedOfAce._target != null && eclipsedOfAce.InRange(eclipsedOfAce._target, 5f))
        {
            if(!eclipsedOfAce.InView(eclipsedOfAce._target))
                return;
            eclipsedOfAce._target.TakeDamage(eclipsedOfAce.model, new DamageInfo(RwbpType.B, 15, 20));
        }
    }

    private void AttackEnd(TrackEntry track)
    {
        eclipsedOfAce.attacking = false;
        eclipsedOfAce.moving = false;
        eclipsedOfAce.chasing = false;

        if(eclipsedOfAce._target != null && eclipsedOfAce._target is WorkerModel && ((WorkerModel)eclipsedOfAce._target).IsDead())
        {
            eclipsedOfAce.MakeChildCreature(eclipsedOfAce._target);
            eclipsedOfAce._target = null;
        }
    }
}