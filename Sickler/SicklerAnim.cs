using Spine;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace Tuantu.SicklerCreature
{
    public class SicklerAnim : CreatureAnimScript
    {
        private Sickler sickler;
        private SoundEffectPlayer idlePlayer;
        private SoundEffectPlayer escapePlayer;
        private new SkeletonAnimation animator;

        public void SetScript(Sickler _sickler)
        {
            sickler = _sickler;
            animator = base.gameObject.GetComponent<SkeletonAnimation>();
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
            if(escapePlayer != null)
                Destroy(escapePlayer.gameObject);
            animator.AnimationState.SetAnimation(0, "Idle", true);
            idlePlayer = SoundEffectPlayer.Play_Mod("Tuantu_MOD", "Sickler_Idle", animator.transform, 1f, 1f, AudioRolloffMode.Logarithmic);
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
            SoundEffectPlayer.PlayOnce_Mod("Tuantu_MOD", "Sickler_FirstScream", (Vector2)sickler.model.GetCurrentViewPosition(), 1f);
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
                if(idlePlayer != null)
                    Destroy(idlePlayer.gameObject);
                escapePlayer = SoundEffectPlayer.Play_Mod("Tuantu_MOD", "Sickler_EscapeScream", animator.transform, 1f, 1f, AudioRolloffMode.Logarithmic);
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