using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using System;

public class EclipsedOfAce : CreatureBase
{
    public string cname = "에이스의 일식";
    public Influence gosyujinsama;
    private Timer attacktimer = new Timer();
    private EclipsedOfAceAnim animscript;
    public bool attacking = false;
    public bool chasing = false;   
    private int attack = 0;
    public bool moving = false;
    public UnitModel _target;

    public override void SetModel(CreatureModel model)
    {
        this.model = model;
        gosyujinsama = ((ChildCreatureModel)model).parent.script as Influence;
        model.movementScale *= UnityEngine.Random.Range(0.7f, 1.3f);
    }

    public override bool OnAfterSuppressed()
    {
        return false;
    }

    public override void OnViewInit(CreatureUnit unit)
    {
        base.OnViewInit(unit);
        animscript = (EclipsedOfAceAnim)unit.animTarget;
        animscript.SetScript(this);
    }

    public override void Escape()
    {
        base.Escape();
        model.Escape();
        base.UniqueEscape();
        AnyMove();
    }

    public void AnyMove()
    {
        model.ClearCommand();
        MapNode roamNode = MapGraph.instance.GetCreatureRoamingPoint();
        if (roamNode == null)
        {
            UnityEngine.Debug.LogWarning("AnyMove: roamNode null");
            return;
        }
        model.MoveToNode(roamNode);
        var cmd = model.GetCurrentCommand();
        if (cmd == null)
        {
            UnityEngine.Debug.LogWarning("AnyMove: currentCommand null");
            return;
        }
        cmd.SetEndCommand(EndMove);
        if (animscript == null)
        {
            UnityEngine.Debug.LogWarning("AnyMove: animscript null");
            return;
        }
        animscript.OnMove();
        moving = true;
    }

    public void EndMove()
    {
        moving = false;
    }

    public bool InRange(UnitModel tar , float range)
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

    public override void UniqueEscape()
    {
        try
        {
            base.UniqueEscape();
            model.CheckNearWorkerEncounting();

            if(attacking)
                return;

            // 목표 유효성 검사
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

            // 사거리 밖이거나 목표가 없으면 매 틱 더 가까운 적을 재탐색
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
                    animscript.OnMove();
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
            UnityEngine.Debug.LogError(ex);
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

    private void StartAttack()
    {
        if(_target != null && !attacking)
        {
            if(attack == 0)
            {
                StopMoment();
                attacktimer.StartTimer(3f);
                attacking = true;
                animscript.Attack();
                attack += 1;
            }
            else if(attack == 1)
            {
                StopMoment();
                attacktimer.StartTimer(6f);
                attacking = true;
                animscript.attack_bigsword();
                attack += 1;
            }
            else if(attack == 2)
            {
                StopMoment();
                attacktimer.StartTimer(6f);
                attacking = true;
                animscript.attack_beam();
                attack = 0;
            }
        }
    }

    private void StopMoment()
    {
        animscript.Idle();
        moving = false;
        movable.StopMoving();
        if(model.GetCurrentCommand() is MoveCreatureCommand)
            model.ClearCommand();
    }

    public override void OnReturn()
    {
        animscript.Idle();
        animscript.SetScript(this);
        attacking = false;
        moving = false;
        chasing = false;
        attack = 0;
        attacktimer = new Timer();
        _target = null;
    }

    public override void OnFixedUpdate(CreatureModel creature)
    {
        base.OnFixedUpdate(creature);
        attacktimer.RunTimer();
    }

    public override string GetName()
    {
        return cname;
    }
}