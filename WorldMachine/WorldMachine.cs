using UnityEngine;
using Tuantu;
using System.Collections.Generic;

namespace Tuantu.WorldMachineCreature
{
    public class WorldMachine : CreatureBase, IObserver
    {
        private float totalAddHp;
        private float totalAddMental;
        private float totalAddEnergyMul;
        private bool overloadFlag;
        private bool energyMuled;
        private bool hpspAdded;
        private WorldMachineAnim anim;
        private Sefira sefira;

        public override void OnStageStart()
        {
            base.OnStageStart();
            overloadFlag = false;
            sefira = model.GetCurrentNode().GetAttachedPassage().GetSefira();
            SetObserver(true);
            CheckQliphoth();
        }

        public override void OnStageEnd()
        {
            base.OnStageEnd();
            SetObserver(false);
        }

        public override void OnViewInit(CreatureUnit unit)
        {
            base.OnViewInit(unit);
            anim = (WorldMachineAnim)unit.animTarget;
            anim.SetScript(this);
        }

        public override void OnWorkCoolTimeEnd(CreatureFeelingState oldState)
        {
            base.OnWorkCoolTimeEnd(oldState);
            switch(oldState)
            {
                case CreatureFeelingState.BAD:
                    model.SubQliphothCounter();
                    break;
                case CreatureFeelingState.GOOD:
                    model.AddQliphothCounter();
                    break;
                case CreatureFeelingState.NORM:
                    EnergyModel.instance.SubEnergy(35f);
                    if(Prob(25))
                        model.SubQliphothCounter();
                    break;
            }
            CheckQliphoth();
        }

        public override void OnEnterRoom(UseSkill skill)
        {
            base.OnEnterRoom(skill);
            overloadFlag = false;
            AgentModel agent = skill.agent;
            if(agent.Wstat < 4)
                ActivateRandomAbno(out CreatureModel selected);
        }

        public override void ActivateQliphothCounter()
        {
            base.ActivateQliphothCounter();
            Harmony_Patch.AddRecoverHPMental(-totalAddHp, -totalAddMental);
            totalAddHp = 0;
            totalAddMental = 0;
            Harmony_Patch.energyMul -= totalAddEnergyMul;
            totalAddEnergyMul = 0;

            ActivateRandomAbno(out CreatureModel selected);

            if(selected != null)
            {
                int count = (int)(selected.observeInfo.cubeNum * 0.15f);
                selected.SubCreatureSuccessCube(count);
            }
            anim.ActiveSkill();
        }

        private void ActivateRandomAbno(out CreatureModel selected)
        {
            CreatureModel[] creatureArr = CreatureManager.instance.GetCreatureList();
            List<CreatureModel> targets = new List<CreatureModel>();
            foreach(CreatureModel target in creatureArr)
            {
                if(target.script.HasRoomCounter() && target != model)
                    targets.Add(target);
            }
            if(targets.Count == 0)
            {
                selected = null;
                return;
            }
            int randInd = Random.Range(0, targets.Count);
            
            selected = targets[randInd];
            selected.SetQliphothCounter(0);
        }

        public void CheckQliphoth()
        {
            int currentQliphoth = model.qliphothCounter;
            switch(currentQliphoth)
            {
                case 5:
                    if(!hpspAdded)
                    {
                        Harmony_Patch.AddRecoverHPMental(10, 5);
                        totalAddHp += 10;
                        totalAddMental += 5;
                        hpspAdded = true;
                    }
                    break;
                case 4:
                    Harmony_Patch.AddRecoverHPMental(-totalAddHp, -totalAddMental);
                    totalAddHp = 0;
                    totalAddMental = 0;
                    hpspAdded = false;
                    if(!energyMuled)
                    {
                        Harmony_Patch.energyMul += 0.05f;
                        totalAddEnergyMul += 0.05f;
                        energyMuled = true;
                    }
                    break;
                case 3:
                    Harmony_Patch.AddRecoverHPMental(-totalAddHp, -totalAddMental);
                    totalAddHp = 0;
                    totalAddMental = 0;
                    hpspAdded = false;
                    Harmony_Patch.energyMul -= totalAddEnergyMul;
                    totalAddEnergyMul = 0;
                    energyMuled = false;
                    break;
                case 2:
                    if(!hpspAdded)
                    {
                        Harmony_Patch.AddRecoverHPMental(-10, -5);
                        totalAddHp -= 10;
                        totalAddMental -= 5;
                        hpspAdded = true;
                    }
                    Harmony_Patch.energyMul -= totalAddEnergyMul;
                    totalAddEnergyMul = 0;
                    energyMuled = false;
                    break;
                case 1:
                    if(!energyMuled)
                    {
                        Harmony_Patch.energyMul += -0.05f;
                        totalAddEnergyMul += -0.05f;
                        energyMuled = true;
                    }
                    break;
            }
        }
        
        private void SetObserver(bool active)
        {
            if (active)
                Notice.instance.Observe(NoticeName.OnQliphothOverloadLevelChanged, this);
            else
                Notice.instance.Remove(NoticeName.OnQliphothOverloadLevelChanged, this);
        }

        public void OnNotice(string notice, params object[] param)
        {
            if(notice == NoticeName.OnQliphothOverloadLevelChanged)
            {
                if(param != null && param.Length > 0)
                {
                    int num = (int)param[0];
                    if(num != 1)
                        OverloadChange();
                }
            }
        }

        private void OverloadChange()
        {
            if(overloadFlag)
            {
                overloadFlag = false;
                model.SubQliphothCounter();
                CheckQliphoth();
                return;
            }
            overloadFlag = !overloadFlag;
        }
    }
}