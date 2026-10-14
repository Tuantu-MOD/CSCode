
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Tuantu.WishrootCreature
{
    public class WishrootAnim : CreatureAnimScript
    {
        private Wishroot script;
        private new SkeletonAnimation animator;

        public void SetScript(Wishroot _script)
        {
            script = _script;
            animator = base.gameObject.GetComponent<SkeletonAnimation>();
            animator.AnimationState.SetAnimation(0, "Idle", true);
        }
    }
}