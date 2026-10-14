using UnityEngine;
using System;
using System.Collections.Generic;
using Spine.Unity;
using Spine;
using LobotomyBaseMod;
using Tuantu;

namespace Tuantu.WishrootCreature
{
    public class WishrootWeapon : EquipmentScriptBase
    {
        private int norStack;
        private int speStack;
        private int attStack;

        private bool normal;
        private bool special;
        private bool transform;
        private bool noradd;

        private AgentModel worker;
        private UnitModel _target;
        private UnitModel _actor;
        
        private Spine.AnimationState state;

        public override void OnStageStart()
        {
            base.OnStageStart();

            AgentUnit owner = worker.GetUnit();
            owner.animEventHandler.SetAttackEndEvent(delegate{});
            owner.animEventHandler.SetDamageEvent(delegate{});

            norStack = 0;
            speStack = 0;
            attStack = 0;
            normal = false;
            special = false;
            transform = false;
        }

        public override void OnEquip(UnitModel actor)
        {
            base.OnEquip(actor);
            worker = base.model.owner as AgentModel;
        }

        public override void OnCancelWeapon(UnitModel actor)
        {
            if(normal)
            {
                norStack -= 1;
                if(norStack < 0)
                    norStack = 0;
            }
            if(special)
            {
                speStack -= 1;
                if(speStack < 0)
                    speStack = 0;
                norStack = 3;
            }
            if(transform)
            {
                speStack = 3;
                norStack = 3;
            }
        }

        public override WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
        {
            List<DamageInfo> list = new List<DamageInfo>();
            string animname = string.Empty;

            noradd = false;

            _target = target;
            _actor = actor;

            AgentUnit agent = worker.GetUnit();
            agent.animChanger.ChangeAnimator_Mod(new KeyValuePairSS(Harmony_Patch.packageid, "DangoCreatureWeapon_Wishroot"));
            agent.animChanger.GetAnimator().UseState = true;
            this.state = agent.animChanger.state;

            if(this.state == null)
            {
                Debug.Log("state null");
                (base.model as WeaponModel).OnEndAttackCycle();
                base.OnAttackEnd(actor, target);
                SetNextAttackType();
            }
            if(normal)
            {
                Debug.Log("normal");
                animname = base.model.metaInfo.animationNames[0];
                TrackEntry entry = state.SetAnimation(0, "AttackNormal", false);

                entry.Event += Damage;
                entry.Complete += DamageEnd;
            }
            else if(special)
            {
                Debug.Log("special");
                list.Add(new DamageInfo(RwbpType.A, 0, 0));
                animname = base.model.metaInfo.animationNames[1];
                TrackEntry entry = state.SetAnimation(0, "AttackSpecial", false);

                entry.Event += Damage;
                entry.Complete += DamageEnd;
                speStack++;
                norStack = 0;
            }
            else if(transform)
            {
                if(attStack >= 0 && attStack <= 4)
                {
                    list.Add(base.model.metaInfo.damageInfos[2].Copy());
                    list.Add(base.model.metaInfo.damageInfos[2].Copy());
                    list.Add(base.model.metaInfo.damageInfos[2].Copy());
                    animname = base.model.metaInfo.animationNames[2];
                    TrackEntry entry = state.SetAnimation(0, "AttackTrans1", false);
                    entry.Event += Damage;
                    entry.Complete += DamageEnd;
                    speStack = 0;
                    norStack = 0;
                }
                else if(attStack >= 5)
                {
                    list.Add(base.model.metaInfo.damageInfos[3].Copy());
                    list.Add(base.model.metaInfo.damageInfos[3].Copy());
                    animname = base.model.metaInfo.animationNames[3];
                    TrackEntry entry = state.SetAnimation(0, "AttackTrans2", false);
                    entry.Event += Damage;
                    entry.Complete += DamageEnd;
                    speStack = 0;
                    norStack = 0;
                }
            }
            return new WeaponDamageInfo(null, list.ToArray());
        }

        public void DamageEnd(TrackEntry entry)
        {
            (base.model as WeaponModel).OnEndAttackCycle();
            base.OnAttackEnd(_actor, _target);
            noradd = false;
            SetNextAttackType();
        }

        public void Damage(TrackEntry entry, Event e)
        {
            if(e.Data.Name == "Spike_A")
            {
                GiveDamage();
            }
            if(e.Data.Name == "Damage")
            {
                if(normal)
                {
                    if(InView(_target))
                    {
                        _target.TakeDamage(_actor, base.model.metaInfo.damageInfos[0].Copy());
                    }
                    if(!noradd)
                    {
                        norStack++;
                        noradd = true;
                    }
                }
                else if(transform)
                {
                    if(attStack >= 0 && attStack <= 4)
                    {
                        _target.TakeDamage(_actor, base.model.metaInfo.damageInfos[2].Copy());
                    }
                    else if(attStack >= 5)
                    {
                        _target.TakeDamage(_actor, base.model.metaInfo.damageInfos[3].Copy());
                    }
                }
            }
        }

        private void MakeSpike(UnitModel target)
        {
            if (!Add_On.instance.EffectList.TryGetValue("Wishroot_Spike", out var value))
            {
                return;
            }
            GameObject effect = SkeletonAnimation.NewSkeletonAnimationGameObject(value).gameObject;
            SkeletonAnimation animator = effect.GetComponent<SkeletonAnimation>();
            effect.AddComponent<CustomEffect>();
            CustomEffect component = effect.GetComponent<CustomEffect>();
            Vector3 viewPos = target.GetCurrentViewPosition();
            Vector3 pos = new Vector3(viewPos.x, viewPos.y -1f, viewPos.z -5f);
            float currentScale = ((WorkerModel)base.model.owner).GetMovableNode().currentScale;
            effect.transform.SetParent(EffectLayer.currentLayer.transform);
            effect.transform.position = pos;
            effect.transform.localScale = new Vector3(currentScale, currentScale, 2f);
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "Attack", false);
            entry.Event += (trackEntry, e) => {
                if(e.Data.Name == "Damage")
                {
                    target.TakeDamage(base.model.metaInfo.damageInfos[1].Copy());
                    attStack++;
                }
            };
            entry.Complete += (trackEntry) => {
                UnityEngine.Object.Destroy(effect);
            };
        }

        private bool InView(UnitModel target)
        {
            // 내 x
            float mx = _actor.GetCurrentViewPosition().x;
            // 목표 x
            float tx = target.GetCurrentViewPosition().x;

            // 왼쪽 바라보고 목표 x가 더 오른쪽이면
            if(_actor.GetMovableNode().GetDirection() == UnitDirection.LEFT && mx < tx)
                return false;
            // 오른쪽 바라보고 목표 x가 더 왼쪽이면
            if(_actor.GetMovableNode().GetDirection() == UnitDirection.RIGHT && mx > tx)
                return false;
            return true;
        }

        public void GiveDamage()
        {
            if(special)
            {
                List<UnitModel> targets = GetTargets();
                foreach(UnitModel item in targets)
                {
                    MakeSpike(item);
                }
                return;
            }
            Debug.Log("Error : not special but try Spike");
        }

        public override void OnPrepareWeapon(UnitModel actor)
        {
            base.OnPrepareWeapon(actor);
            SetNextAttackType();
        }

        private void SetNextAttackType()
        {
            if(norStack < 3)
            {
                normal = true;
                special = false;
                transform = false;
                base.model.metaInfo.range = 4f;
            }
            else if(norStack >= 3 && speStack < 3)
            {
                normal = false;
                special = true;
                transform = false;
                base.model.metaInfo.range = 100f;
            }
            else if(speStack >= 3)
            {
                normal = false;
                special = false;
                transform = true;
                if(attStack >= 0 && attStack <= 4)
                    base.model.metaInfo.range = 10f;
                else
                    base.model.metaInfo.range = 4f;
            }
            _actor = null;
            _target = null;
        }

        private List<UnitModel> GetTargets()
        {
            List<UnitModel> targetList = new List<UnitModel>();
            PassageObjectModel currentPassage = base.model.owner.GetMovableNode().currentPassage;
            if (currentPassage != null)
            {
                MovableObjectNode[] enteredTargets = currentPassage.GetEnteredTargets(base.model.owner.GetMovableNode());
                foreach (MovableObjectNode movableObjectNode in enteredTargets)
                {
                    UnitModel unit = movableObjectNode.GetUnit();
                    if (IsHostile(unit))
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
    }
}