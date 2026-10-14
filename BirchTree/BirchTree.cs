using UnityEngine;

namespace Tuantu.BirchTreeCreature
{
    public class BirchTree : CreatureBase
    {
        public BirchTreeAnim anim;

        public override void OnEnterRoom(UseSkill skill)
        {
            base.OnEnterRoom(skill);
            AgentModel agent = skill.agent;

            if(agent.Wstat <= 1)
            {
                agent.Die();
            }
        }

        public override void OnFinishWork(UseSkill skill)
        {
            base.OnFinishWork(skill);
            AgentModel agent = skill.agent;
            
            if(skill.skillTypeInfo.rwbpType == RwbpType.P && agent.Pstat <= 2)
            {
                agent.Die();
            }
        }

        public override void OnViewInit(CreatureUnit unit)
        {
            // 애니메이션 설정
            try
            {
                base.OnViewInit(unit);
                anim = (BirchTreeAnim)unit.animTarget;
                anim.SetScript(this);
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex);
            }
        }
    }
}