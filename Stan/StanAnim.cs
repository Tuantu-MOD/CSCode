
using System.IO;
using Spine;
using Spine.Unity;
using UnityEngine;
using System.Reflection;

namespace Tuantu.StanCreature
{
    public class StanAnim : CreatureAnimScript
    {
        private Stan stan;
        private new SkeletonAnimation animator;

        public void SetScript(Stan _stan)
        {
            try
            {
                stan = _stan;
                animator = base.gameObject.GetComponent<SkeletonAnimation>();
                animator.gameObject.SetActive(true);
                Idle();
            }
            catch(System.Exception ex)
            {
                Debug.Log(ex);
            }
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
            if(e.Data.Name == "Attack" && stan._target != null && stan.InRange(stan._target, 1.5f))
            {
                if(!stan.InView(stan._target))
                    return;
                stan._target.TakeDamage(stan.model, new DamageInfo(RwbpType.R, 6, 8));
            }
        }

        public void PlayDead()
        {
            animator.AnimationState.SetAnimation(0, "Dead", false);
        }

        private void AttackEnd(TrackEntry track)
        {
            stan.attacking = false;
            stan.chasing = false;
            stan._target = null;
        }
    }
}