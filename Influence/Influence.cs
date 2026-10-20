using System;
using UnityEngine;
using System.Collections.Generic;

public class Influence : CreatureBase
{

    private static class ChildInfo
    {
        public static void GetChildInfo(out string script, out string prefab)
        {
            script = "Influence_zerotwo";
            prefab = "Custom/Influence_zerotwoAnim";
        }
    }
    private InfluenceAnim anim;
    private Timer attacktimer = new Timer();
    public bool attacking = false;
    public bool chasing = false;
    private RwbpType type;
    private Sefira _currentSefira;
    public Sefira CurrentSefira => _currentSefira;
    private RwbpType ptype = RwbpType.W;
    private float damageInterveral = 10f;
    public bool moving = false;
    private float normQliphothSubChance = 0.5f;
    public UnitModel _target;


    private void RandomType()
    {
        int wtype = UnityEngine.Random.Range(1,5);
        switch (wtype)
        {
            case 1:
                type = RwbpType.R;
                break;
            case 2:
                type = RwbpType.W;
                break;
            case 3:
                type = RwbpType.B;
                break;
            case 4:
                type = RwbpType.P;
                break;
        }
        type = RwbpType.W;
    }
    public override void ParamInit()
    {
        base.ParamInit();
        _currentSefira = model.GetCurrentNode().GetAttachedPassage().GetSefira();
        model.ResetQliphothCounter();
        RandomType();
    }

    public override void OnViewInit(CreatureUnit unit)
    {
        base.OnViewInit(unit);
        anim = (InfluenceAnim)unit.animTarget;
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
                if(UnityEngine.Random.value < normQliphothSubChance)
                {
                    model.SubQliphothCounter();
                }
                break;
        }
        if(AllocatedAgent != null && AllocatedAgent.prudenceLevel < 5)
        {
            model.SubQliphothCounter();
        }
        RandomType();
    }


    public override void OnFinishWork(UseSkill skill)
    {
        base.OnFinishWork(skill);
        if(ptype == RwbpType.W)
        {
            UnityEngine.Debug.Log("do the W work");
            AgentModel agent = skill.agent;
            agent.AddUnitBuf(new InBuf(this));
        }
    }
    public override void ActivateQliphothCounter()
    {
        base.ActivateQliphothCounter();
        Escape();

        try
        {
            SpawnEclipsedOfAce();
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.Log("SpawnEclipsedOfAce failed: " + ex);
        }
    }

    public override void Escape()
    {
        base.Escape();
        _currentSefira = model.sefira;
        if (_currentSefira == null)
        {
            UnityEngine.Debug.LogWarning("Influence.Escape: CurrentSefira null");
            return;
        }
        MapNode centerNode = CurrentSefira.sefiraPassage.centerNode;
        model.SetCurrentNode(centerNode);
        model.Escape();
        anim.Escape_Idle();
    }



    public override bool OnAfterSuppressed()
    {
        anim.PlayDead();
        return true;
    }

    public override void UniqueEscape()
    {
        anim.Escape_Idle();
    }

    public override void OnFixedUpdate(CreatureModel creature)
    {
        base.OnFixedUpdate(creature);
        attacktimer.RunTimer();

        if (!attacktimer.started)
        {
            attacktimer.StartTimer(damageInterveral);
            DamageAllWorkersInRoom();
        }
    }

    private void DamageAllWorkersInRoom()
    {
        MovableObjectNode node = model.GetMovableNode();
        if (node == null || node.currentPassage == null)
            return;

        MovableObjectNode[] enteredTargets = node.currentPassage.GetEnteredTargets(node);
        foreach (MovableObjectNode movablenode in enteredTargets)
        {
            UnitModel unit = movablenode.GetUnit();
            if (unit == null || unit.hp <= 0f)
                continue;

            if (unit is WorkerModel worker && !worker.IsDead())
            {
                worker.TakeDamage(model, new DamageInfo(RwbpType.W, 2, 6));
            }
        }
    }

    private void SpawnEclipsedOfAce()
    {
        string script = "EclipsedOfAce";
        string prefab = "Custom/EclipsedOfAceAnim";

        long instID = model.AddChildCreatureModel(script, prefab);
        ChildCreatureModel childcreature = model.GetChildCreatureModel(instID);
        childcreature.RiskLevel = RiskLevel.ALEPH;

        childcreature.script.movable.SetDirection(model.GetMovableNode().GetDirection());
        childcreature.GetMovableNode().Assign(model.GetMovableNode());
        childcreature.Unit.init = true;

        childcreature.Escape();
    }

    public override ChildCreatureModel MakeChildCreature(UnitModel origin)
    {
        UnityEngine.Debug.Log("summon zerotwo");
        AgentModel agentmodel = null;
        OfficerModel officermodel = null;
        bool isoff = false;
        bool isagen = false;
        if(origin is AgentModel)
        {
            agentmodel = origin as AgentModel;
            isagen = true;
        }
        else if(origin is OfficerModel)
        {
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
            childcreature.RiskLevel = RiskLevel.WAW;
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