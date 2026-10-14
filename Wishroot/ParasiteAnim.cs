
using System.IO;
using Spine;
using Spine.Unity;
using UnityEngine;
using System.Reflection;

namespace Tuantu.WishrootCreature
{
    public class ParasiteAnim : CreatureAnimScript
    {
        private ParasiticWish parasite;
        private new SkeletonAnimation animator;

        public void SetScript(ParasiticWish _parasite)
        {
            try
            {
                parasite = _parasite;
                animator = base.gameObject.GetComponent<SkeletonAnimation>();
                animator.gameObject.SetActive(true);
            }
            catch(System.Exception ex)
            {
                Debug.Log(ex);
            }
        }

        public void OnMove()
        {
            animator.AnimationState.SetAnimation(0, "Panic_Berserk_Walk", true);
        }

        public void Attack1()
        {
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "Attack1", false);
            entry.Event += Attack;
            entry.Complete += AttackEnd;
        }

        public void Attack2()
        {
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "Attack2", false);
            entry.Event += Attack;
            entry.Complete += AttackEnd;
        }

        public void Attack3()
        {
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "Attack3", false);
            entry.Event += Attack;
            entry.Complete += AttackEnd;
        }

        public void Attack(TrackEntry track, Event e)
        {
            if(e.Data.Name == "Damage" && parasite._target != null && parasite.InRange(parasite._target, 2.3f))
            {
                if(!parasite.InView(parasite._target))
                    return;
                parasite._target.TakeDamage(parasite.model, new DamageInfo(RwbpType.P, 5, 9));
            }
        }

        public void PlayDead()
        {
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "Dead", false);
            entry.Complete += DeadEnd;
        }

        private void DeadEnd(TrackEntry track)
        {
            Destroy(this.gameObject);
        }

        private void AttackEnd(TrackEntry track)
        {
            parasite.attacking = false;
            parasite.chasing = false;
            parasite.moving = true;
        }
    }
}