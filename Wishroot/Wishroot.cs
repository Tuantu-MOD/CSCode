
using UnityEngine;
using WorkerSpine;
using System.Collections.Generic;
using Harmony;

namespace Tuantu.WishrootCreature
{
    public class Wishroot : CreatureBase
    {
        public static class ChildInfo
        {
            public static void GetChildInfo(out string script, out string prefab)
            {
                script = "ParasiticWish";
                prefab = "Custom/ParasiteAnim";
            }
        }

        private class UpgradeBuf : UnitStatBuf
        {
            private WorkerPrimaryStat stat;
            private Timer hittimer;
            private UnitModel agent;
            private Wishroot root;

            public override void FixedUpdate()
            {
                hittimer.RunTimer(); // 타이머 시간 가게
                if(!hittimer.started)
                {
                    agent.TakeDamage(new DamageInfo(RwbpType.P, 10, 10));
                    hittimer.StartTimer(15f);
                }
            }

            public override void OnUnitDie()
            {
                AgentModel target = (AgentModel)agent;

                var bufList = AccessTools.Field(typeof(UnitModel), "_bufList").GetValue(target) as List<UnitBuf>;

                foreach(UnitBuf buf in bufList)
                {
                    if(buf is ParasiteBuf)
                    {
                        return;
                    }
                }

                target.AddUnitBuf(new ParasiteBuf(root, -1f));
            }

            public UpgradeBuf(WorkerPrimaryStat s, Wishroot _root)
                :base(0f, UnitBufType.ADD_SUPERARMOR)
            {
                stat = s;
                root = _root;
            }

            public override void Init(UnitModel model)
            {
                base.Init(model);
                agent = model;
                hittimer = new Timer();
                hittimer.StartTimer(10f);
                primaryStat.hp = stat.hp;
                primaryStat.mental = stat.mental;
                primaryStat.work = stat.work;
                primaryStat.battle = stat.battle;
                if (!(model is WorkerModel))
                {
                    return;
                }
                WorkerModel workerModel = model as WorkerModel;
                WorkerUnit workerUnit = workerModel.GetWorkerUnit();
                if (workerUnit != null)
                {
                    StatAdditionEffect.MakeEffect(RwbpType.R, workerUnit.animRoot);
                    StatAdditionEffect.MakeEffect(RwbpType.W, workerUnit.animRoot);
                    StatAdditionEffect.MakeEffect(RwbpType.B, workerUnit.animRoot);
                    StatAdditionEffect.MakeEffect(RwbpType.P, workerUnit.animRoot);
                }
            }
        }

        private WishrootAnim anim;
        private AgentModel agent;
        private bool eating;
        private List<long> eaten = new List<long>();

        public override void OnStageStart()
        {
            base.OnStageStart();
            eaten.Clear();
            ParamInit();
        }

        public override void ParamInit()
        {
            base.ParamInit();
            agent = null;
            eating = false;
        }

        public override void OnViewInit(CreatureUnit unit)
        {
            try
            {
                base.OnViewInit(unit);
                anim = (WishrootAnim)unit.animTarget;
                anim.SetScript(this);
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex);
            }
        }

        public override void OnFixedUpdate(CreatureModel creature)
        {
            if(agent != null && !eating && agent.GetCurrentNode() == model.GetCustomNode())
            {
                eating = true;
                ActiveSkill();
            }
        }

        public override void OnReleaseWork(UseSkill skill)
        {
            base.OnReleaseWork(skill);
            RwbpType work = skill.skillTypeInfo.rwbpType;
            
            if(skill.agent.IsDead() || skill.agent.IsPanic())
                return;

            if(work == RwbpType.R || skill.GetCurrentFeelingState() == CreatureFeelingState.BAD)
            {
                agent = skill.agent;
                EatApple();
            }
        }

        private void EatApple()
        {
            agent.StopPanic();
            agent.LoseControl();
            agent.StopStun();
            agent.workerAnimator.SetBool("Work", false);
            agent.MoveToNode(model.GetCustomNode());
        }

        private void ActiveSkill()
        {
            agent.GetUnit().animChanger.ChangeAnimatorWithUniqueFace(WorkerSpine.AnimatorName.WellcheersAgentCTRL, false);
            agent.GetUnit().animEventHandler.SetAnimEvent(OnAnimCalled);
            agent.workerAnimator.SetInteger("Color", 1);
        }

        private void OnAnimCalled(int i)
        {
            if (agent != null)
            {
                switch (i)
                {
                case 0:
                    MakeDrinkSound();
                    break;
                case 1:
                    if(eaten.Contains(agent.instanceId))
                    {
                        Transform();
                    }
                    else
                    {
                        UpgradeStat();
                    }
                    break;
                }
            }
        }

        private void Transform()
        {
            if(agent != null)
            {
                if(!agent.IsDead())
                {
                    agent.AddUnitBuf(new ParasiteBuf(this, 10f));
                    agent.GetControl();
                }
            }
        }

        private void UpgradeStat()
        {
            if(agent != null)
            {
                int buffstat = 15;
                
                WorkerPrimaryStat stat = new WorkerPrimaryStat();

                stat.hp = buffstat;
                stat.mental = buffstat;
                stat.work = buffstat;
                stat.battle = buffstat;

                agent.AddUnitBuf(new UpgradeBuf(stat, this));

                agent.RecoverHP((float)agent.maxHp);
                agent.RecoverMental((float)agent.maxMental);

                agent.GetUnit().animChanger.ChangeAnimator();
                agent.GetControl();

                eaten.Add(agent.instanceId);

                ParamInit();
            }
        }

        private void MakeDrinkSound()
        {
            if (agent != null)
            {
                base.Unit.PlaySound("Drink2");
            }
        }

        public override ChildCreatureModel MakeChildCreature(UnitModel origin)
        {
            if(origin is AgentModel)
            {
                AgentModel agentmodel = origin as AgentModel;
                string script = string.Empty;
                string prefab = string.Empty;
                ChildInfo.GetChildInfo(out script, out prefab);
                long instID = model.AddChildCreatureModel(script, prefab);
                ChildCreatureModel childcreature = model.GetChildCreatureModel(instID);
                childcreature.RiskLevel = RiskLevel.WAW;
                childcreature.script.movable.SetDirection(origin.GetMovableNode().GetDirection());
                childcreature.GetMovableNode().Assign(origin.GetMovableNode());
                childcreature.Unit.init = true;
                agentmodel.GetWorkerUnit().gameObject.SetActive(false);
                if(!agentmodel.IsDead())
                    agentmodel.Die();
                
                childcreature.Escape();
                ((ParasiticWish)childcreature.script).name = agentmodel.GetUnitName();
                return childcreature;
            }
            return base.MakeChildCreature(origin);
        }
    }
}