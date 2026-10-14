using UnityEngine;
using System.Collections.Generic;
using Spine;
using WorkerSpine;

namespace Tuantu.SkyboxCreature
{
    public class SkyboxWeapon : EquipmentScriptBase
    {
        private int soaring;
        private AgentModel agent;

        private WorkerAnimatorChanger workerAnimChanger;
        private UnitAnimatorController unitAnimController;
        private UnitModel target;
        private List<UnitModel> soaringSkyTargets = new List<UnitModel>();
        private int thurstAttNum;
        public MovableObjectNode movable => agent.GetMovableNode();
        public PassageObjectModel currentPassage => movable.GetPassage();
        private float soaringSkyRange;
        private bool isAttacking;
        private bool isSet;

        public override void OnEquip(UnitModel actor)
        {
            base.OnEquip(actor);
            agent = base.model.owner as AgentModel;
        }

        public override void OnStageStart()
        {
            soaringSkyRange = 14f;
            isAttacking = false;

            bool hasArmor = agent.HasEquipment(9200009);
            bool hasGift = agent.HasEquipment(9200010);

            if(hasArmor && hasGift)
                isSet = true;

            AgentUnit owner = agent.GetUnit();
            workerAnimChanger = owner.animChanger;
            unitAnimController = owner.GetComponent<UnitAnimatorController>();
            owner.animEventHandler.SetAttackEndEvent(delegate{});
            owner.animEventHandler.SetDamageEvent(delegate{});
        }

        public override void OnStageRelease()
        {
            base.OnStageRelease();
            isSet = false;
            isAttacking = true;
            model.metaInfo.range = 3f;
        }

        public void OnStart(TrackEntry e)
        {
            ((UnitModel)agent).tempAnim.PlayAttackAnimation(null);
            unitAnimController.SetBattle(false);
            agent.GetWorkerUnit().animChanger.state.Start -= OnStart;
        }

        public void OnEvent(TrackEntry entry, Event e)
        {
            int damageGuarantee = 1;
            if(e.Data.Name == "Damage_Basic")
            {
                DamageInfo originDamage = model.metaInfo.damageInfos[0];
                DamageInfo baseDamage = new DamageInfo(originDamage.type, (int)originDamage.min, (int)originDamage.max);
                if(isSet)
                {
                    baseDamage.min += 5;
                    baseDamage.max += 5;
                }
                target.TakeDamage(agent, baseDamage);
                soaring += 1;
            }
            if(e.Data.Name == "Damage_SkyThrust")
            {
                DamageInfo originDamage = model.metaInfo.damageInfos[1];
                DamageInfo baseDamage = new DamageInfo(originDamage.type, (int)originDamage.min, (int)originDamage.max);
                baseDamage.min += thurstAttNum * 0.3f;
                baseDamage.max += thurstAttNum * 0.3f;
                if(isSet)
                {
                    baseDamage.min *= soaring * 0.15f + damageGuarantee;
                    baseDamage.max *= soaring * 0.15f + damageGuarantee;
                }
                if(!isSet)
                {
                    baseDamage.min *= soaring * 0.1f + damageGuarantee;
                    baseDamage.max *= soaring * 0.1f + damageGuarantee;
                }

                target.TakeDamage(agent, baseDamage);
                thurstAttNum += 1;
                soaring += 1;
            }
            if(e.Data.Name == "Damage_SoaringSky")
            {
                DamageInfo originDamage = model.metaInfo.damageInfos[2];
                DamageInfo baseDamage = new DamageInfo(originDamage.type, (int)originDamage.min, (int)originDamage.max);
                if(isSet)
                {
                    baseDamage.min *= soaring * 0.35f + damageGuarantee;
                    baseDamage.max *= soaring * 0.35f + damageGuarantee;
                }
                if(!isSet)
                {
                    baseDamage.min *= soaring * 0.3f + damageGuarantee;
                    baseDamage.max *= soaring * 0.3f + damageGuarantee;
                }
                soaring = 0;

                foreach(UnitModel _target in soaringSkyTargets)
                {
                    _target.TakeDamage(agent, baseDamage);
                    soaring += 2;
                }
            }
            if(e.Data.Name == "Damage_AerialBash")
            {
                DamageInfo originDamage = model.metaInfo.damageInfos[3];
                DamageInfo baseDamage = new DamageInfo(originDamage.type, (int)originDamage.min, (int)originDamage.max);
                baseDamage.min *= soaring * 0.1f + damageGuarantee;
                baseDamage.max *= soaring * 0.1f + damageGuarantee;
                if(isSet)
                {
                    soaring += 3;
                }
                soaring += 5;
                target.TakeDamage(agent, baseDamage);
            }
            if(e.Data.Name == "Damage_AerialThrust")
            {
                DamageInfo originDamage = model.metaInfo.damageInfos[4];
                DamageInfo baseDamage = new DamageInfo(originDamage.type, (int)originDamage.min, (int)originDamage.max);
                float applySoaring = soaring * 0.1f;
                if(isSet)
                {
                    baseDamage.min *= applySoaring * 0.15f + damageGuarantee;
                    baseDamage.max *= applySoaring * 0.15f + damageGuarantee;
                }
                if(!isSet)
                {
                    baseDamage.min *= applySoaring * 0.1f + damageGuarantee;
                    baseDamage.max *= applySoaring * 0.1f + damageGuarantee;
                }

                target.TakeDamage(agent, baseDamage);
                soaring += 1;
            }
            if(e.Data.Name == "AttackEnd")
            {
                agent.GetWorkerUnit().animChanger.state.Event -= OnEvent;
                OnCancelWeapon(agent);
            }
        }

        public override void OnCancelWeapon(UnitModel actor)
        {
            base.OnCancelWeapon(actor);
            agent.GetWorkerUnit().animChanger.state.Event -= OnEvent;
            isAttacking = false;
        }

        public override void OnPrepareWeapon(UnitModel actor)
        {
            base.OnPrepareWeapon(actor);
            isAttacking = false;
            if(soaring >= 20)
            {
                model.metaInfo.range = 5f;
            }
            else
            {
                model.metaInfo.range = 3f;
            }
        }

        public override WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel _target)
        {
            if(isAttacking)
                return null;
            List<DamageInfo> list = new List<DamageInfo>();
            string animname = string.Empty;
            thurstAttNum = 0;
            isAttacking = true;

            agent.GetWorkerUnit().animChanger.state.Start += OnStart;
            agent.GetWorkerUnit().animChanger.state.Event += OnEvent;
            target = _target;
            int prob = Random.Range(0, 11);

            if(soaring >= 20)
            {
                soaringSkyTargets = GetTargets();
                list.Add(new DamageInfo(RwbpType.A, 0, 0));
                animname = base.model.metaInfo.animationNames[2];
                agent.GetUnit().animChanger.state.SetAnimation(0, "Attack_SoaringSky", false);
            }
            
            else if(prob > 7 && soaring >= 10) // 찌르기(8, 9, 10)
            {
                if(soaring >= 15) // 강한 찌르기
                {
                    list.Add(new DamageInfo(RwbpType.A, 0, 0));
                    animname = base.model.metaInfo.animationNames[4];
                    agent.GetUnit().animChanger.state.SetAnimation(0, "Attack_AerialThrust", false);
                }
                else
                {
                    list.Add(new DamageInfo(RwbpType.A, 0, 0));
                    animname = base.model.metaInfo.animationNames[1];
                    agent.GetUnit().animChanger.state.SetAnimation(0, "Attack_SkyThrust", false);
                }
            }
            else if(prob > 4 && prob < 8) // 방패(5, 6, 7)
            {
                list.Add(new DamageInfo(RwbpType.A, 0, 0));
                animname = base.model.metaInfo.animationNames[3];
                agent.GetUnit().animChanger.state.SetAnimation(0, "Attack_AerialBash", false);
            }
            else // 일반(1, 2, 3, 4, 활공 부족할 시)
            {
                list.Add(new DamageInfo(RwbpType.A, 0, 0));
                animname = base.model.metaInfo.animationNames[0];
                agent.GetUnit().animChanger.state.SetAnimation(0, "Attack_Basic", false);
            }
            return new WeaponDamageInfo(animname, list.ToArray());
        }

        private List<UnitModel> GetTargets()
        {
            List<UnitModel> targetList = new List<UnitModel>();
            if (currentPassage != null)
            {
                MovableObjectNode[] enteredTargets = currentPassage.GetEnteredTargets(base.model.owner.GetMovableNode());
                foreach (MovableObjectNode movableObjectNode in enteredTargets)
                {
                    UnitModel unit = movableObjectNode.GetUnit();
                    if (IsHostile(unit) && GetDis(unit))
                    {
                        targetList.Add(unit);
                    }
                }
            }
            return targetList;
        }

        private bool IsHostile(UnitModel target)
        {
            if (!target.IsAttackTargetable())
            {
                return false;
            }
            WorkerModel workerModel = base.model.owner as WorkerModel;
            if (target == workerModel)
            {
                return false;
            }
            if (base.model.owner.IsHostile(target))
            {
                return true;
            }
            if (workerModel != null && workerModel.IsPanic())
            {
                return true;
            }
            if (target is CreatureModel)
            {
                return true;
            }
            return false;
        }

        public override void OnAttackEnd(UnitModel actor, UnitModel target)
        {
            
        }

        private bool GetDis(UnitModel tar)
        {
            // 목표 x 좌표
            float x = tar.GetCurrentViewPosition().x;
            // 내 x 좌표
            float x2 = agent.GetCurrentViewPosition().x;
            // 변경할 수(기본값)
            float num = 1f;
            if(currentPassage != null)
                num = currentPassage.scaleFactor; // 축소, 확대 같은거
            float dis = Mathf.Abs(x - x2) / num; // 계산
            if(dis <= soaringSkyRange)
            {
                return true;
            }
            return false;
        }
    }
}