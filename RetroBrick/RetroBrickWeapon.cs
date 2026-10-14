using System.Collections.Generic;
using UnityEngine;

namespace Tuantu.RetroBrickCreature
{
    public class RetroBrickWeapon : EquipmentScriptBase
    {
        public override WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
        {
            List<DamageInfo> list = new List<DamageInfo>();
            string animname = string.Empty;

            list.Add(base.model.metaInfo.damageInfos[0].Copy());
            animname = base.model.metaInfo.animationNames[0];

            return new WeaponDamageInfo(animname, list.ToArray());    
        }
    }
}