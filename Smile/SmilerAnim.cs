using UnityEngine;
using Spine;
using Spine.Unity;

namespace Tuantu.SmileCreature
{
    public class SmilerAnim : CreatureAnimScript
    {
        private Smiler smiler;
        public new SkeletonAnimation animator;

        public bool starting = true;

        public void SetScript(Smiler _smiler)
        {
            smiler = _smiler;
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
            if(e.Data.Name == "Damage" && smiler._target != null && smiler.InRange(smiler._target, 1.5f))
            {
                if(!smiler.InView(smiler._target))
                    return;
                smiler._target.TakeDamage(smiler.model, new DamageInfo(RwbpType.R, 4, 6));
            }
        }

        private void AttackEnd(TrackEntry track)
        {
            smiler.attacking = false;
            smiler.moving = false;
            smiler.chasing = false;

            if(smiler._target != null && smiler._target is WorkerModel && ((WorkerModel)smiler._target).IsDead())
            {
                smiler.MakeChildCreature(smiler._target);
                smiler._target = null;
            }
        }
    }
}