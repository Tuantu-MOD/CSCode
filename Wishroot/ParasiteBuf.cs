
using Spine;
using UnityEngine;

namespace Tuantu.WishrootCreature
{
    public class ParasiteBuf : UnitBuf
    {
        private Wishroot root;
        private AgentModel agent;

        public ParasiteBuf(Wishroot _root, float time)
        {
            root = _root;
            type = UnitBufType.ADD_SUPERARMOR;
            duplicateType = BufDuplicateType.ONLY_ONE;
            remainTime = time;
        }

        public override void FixedUpdate()
        {
            if(remainTime >= 0)
            {
                remainTime -= Time.deltaTime;
                if(remainTime < 0 || agent.IsDead())
                {
                    agent.LoseControl();
                    agent.Die();
                    Parasite();
                }
            }
        }

        public override void Init(UnitModel model)
        {
            base.model = model;
            agent = (AgentModel)model;
            if(remainTime < 0)
            {
                agent.LoseControl();
                agent.Die();
                Parasite();
            }
        }

        private void Parasite()
        {
            agent.RemoveUnitBuf(this);
            root.MakeChildCreature(agent);
        }
    }
}