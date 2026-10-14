using System.Collections.Generic;
using UnityEngine;
using Spine;
using WorkerSpine;

namespace Tuantu.SicklerCreature
{
    public class SicklerWeapon : EquipmentScriptBase
    {
        private int attnum;
        public int killnum;
        public bool spawn;
        private long instanceId;
        private AgentModel agent;
        private Timer harmTimer = new Timer();

        private WorkerAnimatorChanger workerAnimChanger;
        private UnitAnimatorController unitAnimController;
        private ShadowHunterModel agentHunter;

        public override void OnEquip(UnitModel actor)
        {
            base.OnEquip(actor);
            agent = base.model.owner as AgentModel;
        }


        public override void OnPrepareWeapon(UnitModel actor)
        {
            actor.hp -= (float)actor.maxHp * 0.3f;
            SoundEffectPlayer.PlayOnce_Mod("Tuantu_MOD", "SicklerWeapon_equip", (Vector2)actor.GetCurrentViewPosition());
            if(!spawn || agentHunter == null)
            {
                MakeHunter();
                spawn = true;
                return;
            }
            harmTimer.StartTimer(30f);
            agent.GetUnit().PrepareWeapon();
            agent.GetUnit().Attack(-1, 0);
            unitAnimController.SetBattle(false);
            agent.GetUnit().animEventHandler.SetAnimEvent(null);
            agent.GetUnit().animChanger.GetAnimator().UseState = true;
            agent.GetUnit().animChanger.state.SetAnimation(0, "Walk_OneHand", true);
        }

        public override void OnStageStart()
        {
            AgentUnit owner = agent.GetUnit();
            workerAnimChanger = owner.animChanger;
            unitAnimController = owner.GetComponent<UnitAnimatorController>();
            owner.animEventHandler.SetAttackEndEvent(delegate{});
            owner.animEventHandler.SetDamageEvent(delegate{});

            attnum = 0;
            killnum = 0;
            spawn = false;
            instanceId = 10000L;
        }

        public override void OnStageRelease()
        {
            base.OnStageRelease();
            if(agentHunter != null)
                agentHunter.OnDestroy();
        }

        public override void OnFixedUpdate()
        {
            harmTimer.RunTimer();
        }

        public void OnStart(TrackEntry e)
        {
            e.Event += OnEvent;
            ((UnitModel)agent).tempAnim.PlayAttackAnimation(null);
            unitAnimController.SetBattle(false);
            agent.GetWorkerUnit().animChanger.state.Start -= OnStart;
        }

        private void SpawnHunter()
        {
            ShadowHunterModel hunter = ShadowHunterModel.MakeCreature(instanceId, agent, this);
            agentHunter = hunter;
            hunter.hasGift = ((UnitModel)agent).HasEquipment(9200007);
            agent.hp -= (float)agent.maxHp * 0.2f;
            if(agent.hp <= 0f)
                agent.hp = 1f;
        }

        public void OnEvent(TrackEntry entry, Event e)
        {
            if(e.Data.Name == "Haunt")
            {
                SoundEffectPlayer.PlayOnce_Mod("Tuantu_MOD","SicklerWeapon_Spawn", (Vector2)agent.GetCurrentViewPosition());
                SpawnHunter();
            }
        }

        public override WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
        {
            List<DamageInfo> list = new List<DamageInfo>();
            string animname = string.Empty;

            agent.GetWorkerUnit().animChanger.state.Start += OnStart;

            if(attnum == 0)
            {
                list.Add(base.model.metaInfo.damageInfos[0].Copy());
                animname = base.model.metaInfo.animationNames[killnum];
            }

            else if(attnum == 1)
            {
                animname = base.model.metaInfo.animationNames[1];
                list.Add(base.model.metaInfo.damageInfos[killnum].Copy());                
            }

            else if(attnum == 2)
            {
                animname = base.model.metaInfo.animationNames[2];
                DamageInfo damage = base.model.metaInfo.damageInfos[killnum].Copy();
                damage.min /= 2;
                damage.max /= 2;
                list.Add(damage);
                list.Add(damage);
            }
            
            return new WeaponDamageInfo(animname, list.ToArray());
        }

        public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
            SoundEffectPlayer.PlayOnce_Mod("Tuantu_MOD","SicklerWeapon_Swing", (Vector2)actor.GetCurrentViewPosition());
            return true;
        }

        public override void OnAttackEnd(UnitModel actor, UnitModel target)
        {
            attnum += 1;
            if(attnum > 2)
                attnum = 0;
            if(target.hp <= 0)
                OnKillMainTarget(actor, target);
        }

        public override void OnKillMainTarget(UnitModel actor, UnitModel target)
        {
            killnum += 1;
            if(killnum > 5)
                killnum = 5;
            if(actor is WorkerModel worker)
                worker.RecoverHP(agent.maxHp * 0.3f);
        }

        public void OnKillByHunter()
        {
            killnum += 1;
            if(killnum > 5)
                killnum = 5;
        }

        private void MakeHunter()
        {
            agent.GetUnit().PrepareWeapon();
            agent.GetUnit().Attack(-1, 0);
            unitAnimController.SetBattle(false);
            agent.GetUnit().animEventHandler.SetAnimEvent(null);
            agent.GetUnit().animChanger.GetAnimator().UseState = true;
            TrackEntry entry = agent.GetUnit().animChanger.state.SetAnimation(0, "Haunt", false);
            entry.Event += OnEvent;
        }
    }
}