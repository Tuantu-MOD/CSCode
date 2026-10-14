using Spine;
using Spine.Unity;
using System.Collections.Generic;

namespace Tuantu.SicklerCreature
{
    public class SicklerAnim : CreatureAnimScript
    {
        private Sickler sickler;
        private new SkeletonAnimation animator;

        public void SetScript(Sickler _sickler)
        {
            sickler = _sickler;
            animator = base.gameObject.GetComponent<SkeletonAnimation>();
            Idle();
        }

        public void PlayDead()
        {
            animator.AnimationState.SetAnimation(0, "Dead", false);
        }

        public void NormalMove()
        {
            animator.AnimationState.SetAnimation(0, "Walk-NoBreak", true);
        }

        public void BreakMove()
        {
            animator.AnimationState.SetAnimation(0, "Walk-Break", true);
        }

        public void Idle()
        {
            animator.AnimationState.SetAnimation(0, "Idle", true);
        }

        public void Attack1()
        {
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "Attack1", false);
            entry.Event += Damage;
            entry.Complete += DamageEnd;
        }

        public void Attack2()
        {
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "Attack2", false);
            entry.Event += Damage;
            entry.Complete += DamageEnd;
        }

        public void Heavy()
        {
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "AttackHeavy", false);
            entry.Event += Damage;
            entry.Complete += DamageEnd;
        }

        public void OnScream(List<UnitModel> targets)
        {
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "Scream", false);
            entry.Event += (TrackEntry ent, Event e) =>
            {
                if(e.Data.Name == "Damage")
                {
                    foreach(UnitModel item in targets)
                    {
                        item.TakeDamage(sickler.model, new DamageInfo(RwbpType.W, 3, 3));
                    }
                }
            };
            entry.Complete += (TrackEntry ent) =>
            {
                sickler.screaming = false;
                sickler.breaked = true;
                sickler.firstTargeting = false;
                sickler.AnyMove();
            };
        }
        public void Damage(TrackEntry entry, Event e)
        {
            if(e.Data.Name == "Damage" && sickler._target != null && sickler.InRange(sickler._target, sickler.range))
            {
                if(!sickler.InView(sickler._target))
                    return;
                sickler._target.TakeDamage((UnitModel)sickler.model, sickler.combo);
            }
            if(e.Data.Name == "HeavyDamage" && sickler._target != null && sickler.InRange(sickler._target, sickler.range))
            {
                if(!sickler.InView(sickler._target))
                    return;
                sickler._target.TakeDamage((UnitModel)sickler.model, sickler.heavy);
            }
        }

        public void DamageEnd(TrackEntry entry)
        {
            switch(sickler._target)
            {
                case WorkerModel worker when worker.IsDead():
                case CreatureModel creature when creature.state == CreatureState.SUPPRESSED:
                    sickler.KillUnit();
                    break;
            }
            sickler.attacking = false;
            sickler.screaming = false;
            sickler.chasing = false;
            sickler.moving = true;
        }
    }
}