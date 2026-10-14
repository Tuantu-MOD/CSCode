using Spine.Unity;

namespace Tuantu.BirchTreeCreature
{
    public class BirchTreeAnim : CreatureAnimScript
    {
        private new SkeletonAnimation animator;
        private BirchTree tree;

        public void SetScript(BirchTree tree)
        {
            this.tree = tree;
            animator = base.gameObject.GetComponent<SkeletonAnimation>();
            Idle();
        }

        public void Idle()
        {
            animator.AnimationState.SetAnimation(0, "Idle", true);
        }
    }
}