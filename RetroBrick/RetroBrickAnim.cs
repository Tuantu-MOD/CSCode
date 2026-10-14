using UnityEngine;
using Spine.Unity;

namespace Tuantu.RetroBrickCreature
{
    public class RetroBrickAnim : CreatureAnimScript
    {
        private RetroBrick brick;
        private new SkeletonAnimation animator;
        public void SetScript(RetroBrick _brick)
        {
            brick = _brick;
            animator = base.gameObject.GetComponent<SkeletonAnimation>();
            Idle();
        }

        private void Idle()
        {
            animator.AnimationState.SetAnimation(0, "Idle", true);
        }
    }
}