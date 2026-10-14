namespace Tuantu.SkyboxCreature
{
    public class SkyboxBuf : UnitBuf
    {
        private const float factor = 1.2f;
        public RwbpType targetType;

        public SkyboxBuf(RwbpType type)
        {
            targetType = type;
        }

        public override float OnTakeDamage(UnitModel attacker, DamageInfo damageInfo)
        {
            if(damageInfo.type == targetType)
            {
                return factor;
            }
            return 1;
        }
    }
}