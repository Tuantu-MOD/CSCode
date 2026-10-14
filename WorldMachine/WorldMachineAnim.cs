using Spine;
using Spine.Unity;
using UnityEngine;

namespace Tuantu.WorldMachineCreature
{
    public class WorldMachineAnim : CreatureAnimScript
    {
        private WorldMachine machine;
        private new SkeletonAnimation animator;

        public void SetScript(WorldMachine _machine)
        {
            machine = _machine;
            animator = gameObject.GetComponent<SkeletonAnimation>();
            Idle();
        }

        private void Idle()
        {
            animator.AnimationState.SetAnimation(0, "Idle", true);
        }

        public void ActiveSkill()
        {
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "Active", false);
            entry.Complete += (TrackEntry ent) =>
            {
                Idle();
                machine.model.SetQliphothCounter(3);
                machine.CheckQliphoth();
            };
        }
    }
}