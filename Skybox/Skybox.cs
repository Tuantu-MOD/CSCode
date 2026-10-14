using UnityEngine;

namespace Tuantu.SkyboxCreature
{
    public class Skybox : CreatureBase
    {
        private SkyboxAnim anim;
        private Sefira sefira;
        private bool isActived;

        public override void ParamInit()
        {
            base.ParamInit();
            model.ResetQliphothCounter();
        }

        public override void OnViewInit(CreatureUnit unit)
        {
            base.OnViewInit(unit);
            anim = (SkyboxAnim)unit.animTarget;
            anim.SetScript(this);
        }

        public override void OnStageStart()
        {
            base.OnStageStart();
            sefira = model.GetCurrentNode().GetAttachedPassage().GetSefira();
        }

        public override void OnReleaseWork(UseSkill skill)
        {
            base.OnReleaseWork(skill);
            if(isActived)
            {
                foreach(CreatureModel creature in sefira.creatureList)
                {
                    if(creature != model)
                    {
                        creature.SubQliphothCounter();
                    }
                }
            }
            if(skill.agent.IsDead())
            {
                model.AddQliphothCounter();
                isActived = false;
            }
        }

        public override void OnWorkCoolTimeEnd(CreatureFeelingState oldState)
        {
            base.OnWorkCoolTimeEnd(oldState);
            int prob = 0;
            switch(oldState)
            {
                case CreatureFeelingState.GOOD:
                    prob = 20;
                    break;
                case CreatureFeelingState.BAD:
                    prob = 80;
                    break;
            }
            if(Prob(prob))
            {
                model.SubQliphothCounter();
                QliphothSubed();
            }
        }

        public override void ActivateQliphothCounter()
        {
            base.ActivateQliphothCounter();
            isActived = true;
        }

        private void QliphothSubed()
        {
            int randN = Random.Range(0, 4);
            switch(randN)
            {
                case 0:
                    Amplify(RwbpType.R);
                    break;
                case 1:
                    Amplify(RwbpType.W);
                    break;
                case 2:
                    Amplify(RwbpType.B);
                    break;
                case 3:
                    Amplify(RwbpType.P);
                    break;
            }
        }

        private void Amplify(RwbpType type)
        {
            Debug.Log("Amplify");
            anim.Idle(type);
            foreach(AgentModel agent in AgentManager.instance.GetAgentList())
            {
                agent.AddUnitBuf(new SkyboxBuf(type));
            }
            foreach(OfficerModel officer in OfficerManager.instance.GetOfficerList())
            {
                officer.AddUnitBuf(new SkyboxBuf(type));
            }
        }
    }
}