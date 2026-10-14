using UnityEngine;

namespace Tuantu.RetroBrickCreature
{
    public class RetroBrick : CreatureBase
    {
        private RetroBrickAnim anim;
        public override void OnViewInit(CreatureUnit unit)
        {
            Debug.Log($"RetroBrick ID :: {model.metadataId}");
            base.OnViewInit(unit);
            anim = (RetroBrickAnim)unit.animTarget;
            anim.SetScript(this);
        }
    }
}