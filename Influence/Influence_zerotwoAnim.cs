using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using System;

public class Influence_zerotwoAnim : CreatureAnimScript
{
    private Influence_zerotwo influence_zerotwo;
    public new SkeletonAnimation animator;

    public bool starting = true;

    public void SetScript(Influence_zerotwo _influence_zerotwo)
    {
        influence_zerotwo = _influence_zerotwo;
        animator = base.gameObject.GetComponent<SkeletonAnimation>();
        Idle();
    }

    public void OnMove()
    {
        animator.AnimationState.SetAnimation(0, "Walk_Default_01", true);
    }

    public void Idle()
    {
        animator.AnimationState.SetAnimation(0, "Standing_1", true);
    }

    public void Attack()
    {
        TrackEntry entry = animator.AnimationState.SetAnimation(0, "Attack", false);
        entry.Event += Attack;
        entry.Complete += AttackEnd;
    }

    public void Attack(TrackEntry track, Event e)
    {
        if(e.Data.Name == "Damage" && influence_zerotwo._target != null && influence_zerotwo.InRange(influence_zerotwo._target, 1.5f))
        {
            if(!influence_zerotwo.InView(influence_zerotwo._target))
                return;
            influence_zerotwo._target.TakeDamage(influence_zerotwo.model, new DamageInfo(RwbpType.B, 12, 18));
        }
    }

    private void AttackEnd(TrackEntry track)
    {
        influence_zerotwo.attacking = false;
        influence_zerotwo.moving = false;
        influence_zerotwo.chasing = false;

        if(influence_zerotwo._target != null && influence_zerotwo._target is WorkerModel && ((WorkerModel)influence_zerotwo._target).IsDead())
        {
            influence_zerotwo.MakeChildCreature(influence_zerotwo._target);
            influence_zerotwo._target = null;
        }
    }
}