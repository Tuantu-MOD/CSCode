using UnityEngine;
using Spine;
using Spine.Unity;

namespace Tuantu.SmileCreature
{
    public class SmileAnim : CreatureAnimScript
    {
        private Smile smile;
        private new SkeletonAnimation animator;

        public void SetScript(Smile _smile)
        {
            smile = _smile;
            animator = base.gameObject.GetComponent<SkeletonAnimation>();
            Idle();
        }

        public void Idle()
        {
            animator.AnimationState.SetAnimation(0, "Idle", true);
        }

        public void OnMove()
        {
            animator.AnimationState.SetAnimation(0, "Move", true);
        }

        public void Attack()
        {
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "Attack", false);
            entry.Event += Attack;
            entry.Complete += AttackEnd;
        }

        public void Attack(TrackEntry track, Event e)
        {
            if(e.Data.Name == "Attack" && smile._target != null && smile.InRange(smile._target, 1.5f))
            {
                if(!smile.InView(smile._target))
                    return;
                smile._target.TakeDamage(smile.model, new DamageInfo(RwbpType.R, 6, 8));
            }
        }

        public void PlayDead()
        {
            animator.AnimationState.SetAnimation(0, "Dead", false);
        }

        private void AttackEnd(TrackEntry track)
        {
            smile.attacking = false;
            smile.moving = false;
            smile.chasing = false;

            if(smile._target != null && smile._target is WorkerModel && ((WorkerModel)smile._target).IsDead())
            {
                smile.MakeChildCreature(smile._target);
                smile._target = null;
            }
        }
    }
}