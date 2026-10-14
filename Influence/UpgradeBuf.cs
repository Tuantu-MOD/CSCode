public class UpgradeBuf : UnitStatBuf
{
    private WorkerPrimaryStat stat;
    private Timer hittimer;
    private UnitModel agent;
    private Influence root;

    public UpgradeBuf(WorkerPrimaryStat s, Influence _root)
        : base(0f, UnitBufType.YOU_MUST_HAPPY)
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

    public override void FixedUpdate()
    {
        hittimer.RunTimer();
        if (!hittimer.started)
        {
            agent.TakeDamage(new DamageInfo(RwbpType.P, 10, 10));
            hittimer.StartTimer(10f);
        }
    }

    public override void OnUnitDie()
    {
        AgentModel target = (AgentModel)agent;
        target.RemoveUnitBuf(new InBuf(root));
    }
}
