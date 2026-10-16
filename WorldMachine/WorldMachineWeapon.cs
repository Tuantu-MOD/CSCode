using System.Collections.Generic;

namespace Tuantu.WorldMachineCreature
{
    public class WorldMachineWeapon : EquipmentScriptBase
    {
        public override WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
        {            
            SoundEffectPlayer.PlayOnce_Mod("Tuantu_MOD", "Sniper_Aim/Reload", actor.GetCurrentViewPosition());
            List<DamageInfo> list = new List<DamageInfo>();
            string animname = string.Empty;

            list.Add(model.metaInfo.damageInfos[0].Copy());
            animname = model.metaInfo.animationNames[0];

            return new WeaponDamageInfo(animname, list.ToArray());    
        }

        public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
            SoundEffectPlayer.PlayOnce_Mod("Tuantu_MOD", "Sniper_Shoot", actor.GetCurrentViewPosition());
            return true;
        }
    }
}