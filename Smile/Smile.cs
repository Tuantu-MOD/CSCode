using System;
using UnityEngine;
using System.Collections.Generic;

namespace Tuantu.SmileCreature
{
    public class Smile : CreatureBase
    {
        private static class ChildInfo
        {
            public static void GetChildInfo(out string script, out string prefab)
            {
                script = "Smiler";
                prefab = "Custom/SmilerAnim";
            }
        }

        private SmileAnim anim;
        private Timer attacktimer = new Timer();
        public bool attacking = false;
        public bool chasing = false;
        public bool moving = false;
        public UnitModel _target;

        public override void ParamInit()
        {
            base.ParamInit();
            model.ResetQliphothCounter();
        }

        public override void OnViewInit(CreatureUnit unit)
        {
            base.OnViewInit(unit);
            anim = (SmileAnim)unit.animTarget;
            anim.SetScript(this);
        }

        public override void OnWorkCoolTimeEnd(CreatureFeelingState oldState)
        {
            base.OnWorkCoolTimeEnd(oldState);
            switch (oldState)
            {
                case CreatureFeelingState.BAD:
                    model.SubQliphothCounter();
                    break;
                case CreatureFeelingState.NORM:
                    model.SubQliphothCounter();
                    break;
                case CreatureFeelingState.GOOD:
                    model.AddQliphothCounter();
                    break;
            }
        }

        public override void ActivateQliphothCounter()
        {
            base.ActivateQliphothCounter();
            Escape();
        }

        public override void Escape()
        {
            base.Escape();
            model.Escape();
            base.UniqueEscape();
            AnyMove();
        }

        public override bool OnAfterSuppressed()
        {
            anim.PlayDead();
            return true;
        }

        public override void UniqueEscape()
        {
            try
            {
                base.UniqueEscape();
                model.CheckNearWorkerEncounting();

                if(attacking)
                    return;

                // 목표 유효성 검사 (죽었거나 다른 노드로 이동했으면 해제)
                if(_target != null)
                {
                    if(_target.hp <= 0 || _target.GetMovableNode().currentPassage != model.GetMovableNode().currentPassage)
                    {
                        _target = null;
                        chasing = false;
                        if(moving)
                            StopMoment();
                    }
                }

                // 아직 사거리 밖이거나 목표가 없으면, 매 틱 더 가까운 적이 있는지 재탐색
                // (사거리 안에서는 재탐색하지 않아 공격 시작 직전에 타겟이 바뀌는 것을 방지)
                if(_target == null || !InRange(_target, 1f))
                {
                    UnitModel nearest = FindNearestEnemy();
                    if(nearest != null && nearest != _target)
                    {
                        if(_target == null || GetDis(nearest) < GetDis(_target))
                        {
                            _target = nearest;
                            chasing = false;
                        }
                    }
                }

                if(_target != null)
                {
                    // 사거리 안이면 공격
                    if(InRange(_target, 1f))
                    {
                        if(moving)
                            StopMoment();

                        if(!attacktimer.started)
                        {
                            StartAttack();
                        }
                        return;
                    }

                    // 사거리 밖이면 추적
                    if(!chasing || !moving)
                    {
                        model.MoveToMovable(_target.GetMovableNode());
                        model.GetCurrentCommand().SetEndCommand(EndMove);
                        anim.OnMove();
                        chasing = true;
                        moving = true;
                    }
                    return;
                }

                // 목표가 없으면 순찰
                if(!moving)
                {
                    AnyMove();
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.Log(ex);
            }
        }

        // 현재 통로에서 가장 가까운 적(워커)을 찾는다.
        // 공격 쿨타임(attacktimer)과 무관하게 언제든 호출 가능하도록 분리함.
        private UnitModel FindNearestEnemy()
        {
            MovableObjectNode node = model.GetMovableNode();
            if(node == null || node.currentPassage == null)
                return null;

            List<UnitModel> enemy = new List<UnitModel>();
            MovableObjectNode[] enteredTargets = node.currentPassage.GetEnteredTargets(node);
            foreach(MovableObjectNode movablenode in enteredTargets)
            {
                UnitModel unit = movablenode.GetUnit();
                if(unit == null || unit.hp <= 0f)
                    continue;

                if(unit is WorkerModel worker && !worker.IsDead())
                {
                    enemy.Add(worker);
                }
            }

            if(enemy.Count == 0)
                return null;

            return DistanceUnitUtil.SelectNearestUnit(model, enemy);
        }

        public void AnyMove()
        {
            model.ClearCommand();
            model.MoveToNode(MapGraph.instance.GetCreatureRoamingPoint());
            model.GetCurrentCommand().SetEndCommand(EndMove);
            anim.OnMove();
            moving = true;
        }

        private void StartAttack()
        {
            if(_target != null && !attacking)
            {
                StopMoment();
                attacktimer.StartTimer(3f);
                attacking = true;
                anim.Attack();
            }
        }

        private void StopMoment()
        {
            anim.Idle();
            moving = false;
            movable.StopMoving();
            if(model.GetCurrentCommand() is MoveCreatureCommand)
                model.ClearCommand();
        }
        
        private void EndMove()
        {
            moving = false;
        }

        public bool InRange(UnitModel tar, float range)
        {
            float dis = GetDis(tar);
            return range >= dis;
        }
        private float GetDis(UnitModel tar)
        {
            float x = tar.GetCurrentViewPosition().x;
            float x2 = model.GetCurrentViewPosition().x;
            float num = 1f;
            if(currentPassage != null)
                num = currentPassage.scaleFactor;
            return Math.Abs(x - x2) / num;
        }

        public bool InView(UnitModel target)
        {
            float mx = model.GetCurrentViewPosition().x;
            float tx = target.GetCurrentViewPosition().x;

            if(model.GetDirection() == UnitDirection.LEFT && mx < tx)
                return false;
            if(model.GetDirection() == UnitDirection.RIGHT && mx > tx)
                return false;
            return true;
        }

        public override void OnReturn()
        {
            anim.Idle();
            anim.SetScript(this);
            model.ResetQliphothCounter();
            attacking = false;
            moving = false;
            chasing = false;
            attacktimer = new Timer();
            _target = null;
        }

        public override void OnFixedUpdate(CreatureModel creature)
        {
            base.OnFixedUpdate(creature);
            attacktimer.RunTimer();
        }

        public override ChildCreatureModel MakeChildCreature(UnitModel origin)
        {
            Debug.Log("Starg child gen :: smiler");
            AgentModel agentmodel = null;
            OfficerModel officermodel = null;
            bool isoff = false;
            bool isagen = false;
            if(origin is AgentModel)
            {
                Debug.Log("is agent");
                agentmodel = origin as AgentModel;
                isagen = true;
            }
            else if(origin is OfficerModel)
            {
                Debug.Log("is officer");
                officermodel = origin as OfficerModel;
                isoff = true;
            }
            if(isagen || isoff)
            {
                string script = string.Empty;
                string prefab = string.Empty;
                ChildInfo.GetChildInfo(out script, out prefab);
                long instID = model.AddChildCreatureModel(script, prefab);
                ChildCreatureModel childcreature = model.GetChildCreatureModel(instID);
                childcreature.RiskLevel = RiskLevel.TETH;
                childcreature.script.movable.SetDirection(origin.GetMovableNode().GetDirection());
                childcreature.GetMovableNode().Assign(origin.GetMovableNode());
                childcreature.Unit.init = true;

                if(isagen)
                {
                    agentmodel.GetWorkerUnit().gameObject.SetActive(false);
                    if(!agentmodel.IsDead())
                        agentmodel.Die();
                }
                if(isoff)
                {
                    officermodel.GetWorkerUnit().gameObject.SetActive(false);
                    if(!officermodel.IsDead())
                        officermodel.Die();
                }

                childcreature.Escape();
                return childcreature;
            }
            return base.MakeChildCreature(origin);
        }
    }
}