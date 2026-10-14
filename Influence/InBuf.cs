using System.Diagnostics;
using UnityEngine;

public class InBuf : UnitBuf
{
    private Timer eclipsTickTimer = new Timer();
    private const float _bufRemainTime = 30f;
    private GameObject effect;
    private AgentModel agent;
    private Influence script; 

    public InBuf(Influence script)
    {
        remainTime = _bufRemainTime;
        this.script = script;
    }
    public override void Init(UnitModel model)
	{
		base.Init(model);  
		agent = model as AgentModel;
		eclipsTickTimer.StartTimer(1f);
		effect = agent.MakeCreatureEffectHead(script.model, addlist: false);
	}

    public override void FixedUpdate()
    {
        if (agent != null && !agent.IsDead())
        {
            UnityEngine.Debug.Log("effect buf");
            base.FixedUpdate();
            eclipsTickTimer.RunTimer();

            if (!eclipsTickTimer.started)
            {
                eclipsTickTimer.StartTimer(1f);

                if(agent.level <= 4 && !agent.HasUnitBuf(UnitBufType.DEATH_ANGEL_BETRAYER))
                {
                    agent.mental = 0f;
                    agent.Panic();
                }
                else
                {
                    float buffstat = agent.maxMental * 0.6f;
                    WorkerPrimaryStat stat = new WorkerPrimaryStat();
                    
                    stat.mental = (int)(-1f * buffstat);
                    
                    agent.AddUnitBuf(new UpgradeBuf(stat, script));
                    agent.RecoverMental((float)agent.maxMental);
                }
                if (agent.mental <= 0)
                {
                    UnityEngine.Debug.Log("bufagent mental zero");
                    agent.Die();
                    agent.GetWorkerUnit().gameObject.SetActive(false);
                    script.MakeChildCreature(agent);
                    OnUnitDie();
                    agent = null;
                }
            }
        }
    }

    public override void OnUnitDie()
    {
        base.OnUnitDie();
        DestoryEffect();
    }
    
    private void DestoryEffect()
    {
        if(effect != null)
        {
            Object.Destroy(effect);
            effect = null;
        }
    }
    public override void Destroy()
    {
        base.Destroy();
        DestoryEffect();
    }
}