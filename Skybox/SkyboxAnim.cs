using Spine.Unity;

namespace Tuantu.SkyboxCreature
{
    public class SkyboxAnim : CreatureAnimScript
    {
        private Skybox skybox;
        private new SkeletonAnimation animator;
        public void SetScript(Skybox _skybox)
        {
            skybox = _skybox;
            animator = base.gameObject.GetComponent<SkeletonAnimation>();
            Idle(RwbpType.N);
        }

        public void Idle(RwbpType type)
        {
            switch(type)
            {
                case RwbpType.N:
                    animator.AnimationState.SetAnimation(0, "Idle", true);
                    break;
                case RwbpType.R:
                    animator.AnimationState.SetAnimation(0, "R_Idle", true);
                    break;
                case RwbpType.W:
                    animator.AnimationState.SetAnimation(0, "W_Idle", true);
                    break;
                case RwbpType.B:
                    animator.AnimationState.SetAnimation(0, "B_Idle", true);
                    break;
                case RwbpType.P:
                    animator.AnimationState.SetAnimation(0, "P_Idle", true);
                    break;
            }
        }
    }
}