using UnityEngine;
using Spine.Unity;
using Spine;

namespace Tuantu.SicklerCreature
{
    public class ShadowHunterAnim : CreatureAnimScript
    {
        private ShadowHunter shadowHunter;
        private new SkeletonAnimation animator;

        public void SetScript(ShadowHunter _shadowHunter)
        {
            shadowHunter = _shadowHunter;
            animator = GetComponent<SkeletonAnimation>();
            animator.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }

        public void Haunt()
        {
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "Haunt", false);
            entry.Complete += (TrackEntry entry2) =>
            {
                shadowHunter.haunting = false;
                shadowHunter.moving = true;
            };
        }

        public void Idle()
        {
            animator.AnimationState.SetAnimation(0, "Idle", true);
        }

        public void OnMove()
        {
            animator.AnimationState.SetAnimation(0, "Walk", true);
        }

        public void Attack1()
        {
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "Attack1", false);
            entry.MixDuration = 0f;
            entry.Event += OnEvent;
            entry.Complete += OnEnd;
        }

        public void Attack2()
        {
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "Attack2", false);
            entry.MixDuration = 0f;
            entry.Event += OnEvent;
            entry.Complete += OnEnd;
        }

        public void Heavy()
        {
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "HeavyAttack", false);
            entry.MixDuration = 0f;
            entry.Event += OnEvent;
            entry.Complete += OnEnd;
        }

        public void OnEvent(TrackEntry entry, Event e)
        {
            if(e.Data.Name == "Damage" && shadowHunter._target != null && shadowHunter.InRange(shadowHunter._target, shadowHunter.range))
            {
                if(!shadowHunter.InView(shadowHunter._target))
                    return;
                shadowHunter._target.TakeDamage(((UnitModel)shadowHunter.model), shadowHunter.att1dmg);
            }
            else if(e.Data.Name == "Damage2" && shadowHunter._target != null && shadowHunter.InRange(shadowHunter._target, shadowHunter.range))
            {
                if(!shadowHunter.InView(shadowHunter._target))
                    return;
                shadowHunter._target.TakeDamage(((UnitModel)shadowHunter.model), shadowHunter.att2dmg);
            }
            else if(e.Data.Name == "HeavyDamage" && shadowHunter._target != null && shadowHunter.InRange(shadowHunter._target, shadowHunter.range))
            {
                if(!shadowHunter.InView(shadowHunter._target))
                    return;
                shadowHunter._target.TakeDamage(((UnitModel)shadowHunter.model), shadowHunter.heavydmg);
            }
        }

        public void PlayDead()
        {
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "Dead", false);
            entry.Complete += (TrackEntry ent) =>
            {
                ((ShadowHunterModel)shadowHunter.model).OnDestroy();
            };
        }

        public void OnEnd(TrackEntry entry)
        {
            switch(shadowHunter._target)
            {
                case WorkerModel worker when worker.IsDead():
                case CreatureModel creature when creature.state == CreatureState.SUPPRESSED:
                    shadowHunter.KillUnit();
                    break;
            }
            shadowHunter.attacking = false;
            shadowHunter.chasing = false;
            shadowHunter.moving = true;
        }
    }
}