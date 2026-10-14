using UnityEngine;
using System.Collections.Generic;

namespace Tuantu.SicklerCreature
{
    public class Sickler : CreatureBase
    {
        private SicklerAnim anim;
        private Timer attacktimer = new Timer();

        public bool attacking;
        public bool screaming;
        public bool moving;
        public bool chasing;
        public bool firstTargeting;
        public bool breaked;
        public UnitModel _target;

        private int killCount;
        private float speed;
        private int attcom;
        public float range;
        private float startRange;
        private Sefira sefira;

        public DamageInfo combo;
        public DamageInfo heavy;

        public override void ParamInit()
        {
            base.ParamInit();
            breaked = false;
            attacking = false;
            screaming = false;
            moving = false;
            chasing = false;
            _target = null;
            firstTargeting = true;
            killCount = 0;
            speed = 4f;
            attcom = 0;
            range = 3f;
            startRange = 1.5f;
            attacktimer = new Timer();
            combo = new DamageInfo(RwbpType.R, 10, 20);
            heavy = new DamageInfo(RwbpType.R, 40, 50);
            model.movementScale = speed;
            model.ResetQliphothCounter();
        }

        public override void OnViewInit(CreatureUnit unit)
        {
            base.OnViewInit(unit);
            anim = (SicklerAnim)unit.animTarget;
            anim.SetScript(this);
        }
        
        public override void OnStageStart()
        {
            base.OnStageStart();
            sefira = model.GetCurrentNode().GetAttachedPassage().GetSefira();
            ParamInit();
        }

        public override void OnWorkCoolTimeEnd(CreatureFeelingState state)
        {
            if(state == CreatureFeelingState.BAD || state == CreatureFeelingState.NORM)
            {
                if(Prob(50))
                    model.SubQliphothCounter();
            }
        }

        public override void OnFinishWork(UseSkill skill)
        {
            if(skill.skillTypeInfo.rwbpType == RwbpType.R)
            {
                model.SubQliphothCounter();
                model.SubQliphothCounter();
            }
        }

        public override bool OnAfterSuppressed()
        {
            // 죽음 애니메이션
            anim.PlayDead();
            return true;
        }

        public override void OnReturn()
        {
            // 기본 애니메이션
            anim.Idle();
            // 에니메이션 스크립트 세팅
            anim.SetScript(this);
            // 초기화
            model.ResetQliphothCounter();
            ParamInit();
            attacktimer = new Timer();
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
            AnyMove();
        }

        private void Scream()
        {
            screaming = true;
            List<UnitModel> targets = GetTargets();
            StopMoment();
            anim.OnScream(targets);
        }

        private List<UnitModel> GetTargets()
        {
            List<UnitModel> targetList = new List<UnitModel>();
            foreach (AgentModel target in sefira.agentList)
            {
                if(target.hp > 0)
                    targetList.Add((UnitModel)target);
            }
            foreach(OfficerModel target in sefira.officerList)
            {
                if(target.hp > 0)
                    targetList.Add((UnitModel)target);
            }
            foreach(CreatureModel target in sefira.creatureList)
            {
                if(target != model)
                {
                    if(target.hp > 0)
                        targetList.Add((UnitModel)target);
                }
            }
            return targetList;
        }

        public void AnyMove()
        {
            model.ClearCommand();
            model.MoveToNode(MapGraph.instance.GetCreatureRoamingPoint());
            model.commandQueue.GetCurrentCmd().SetEndCommand(EndMove);
            if(breaked)
                anim.BreakMove();
            else
                anim.NormalMove();
            moving = true;
        }

        private void EndMove()
        {
            moving = false;
        }

        public override void UniqueEscape()
        {
            base.UniqueEscape();
            // 가까운 직원 찾기
            model.CheckNearWorkerEncounting();

            // 목표 없으면 공격중 아님
            if(_target == null)
            {
                attacking = false;
                screaming = false;
            }

            // 공격이나 움직이는 중이 아니라면 추격 안 함
            if(!attacking && !moving && !screaming)
            {
                chasing = false;
                _target = null;
                AnyMove();
                return;
            }

            if(_target != null && model.GetMovableNode().currentPassage == null && _target.GetMovableNode().currentPassage == null)
            {
                moving = false;
                _target = null;
                chasing = false;
                attacking = false;
                screaming = false;
                AnyMove();
                return;
            }

            if(attacking || model.GetMovableNode().currentPassage == null)
                return;

            // 목표가 없지 않고 공격 쿨타임이 돌아 있다면
            if(_target != null && !attacktimer.started)
            {
                if(!screaming || breaked)
                {
                    // 목표 체력이 0 이하면(죽으면)
                    if(_target.hp <= 0)
                    {
                        // 움직임 멈춤(어차피 다시 호출하기 때문에 상관없음)
                        moving = false;
                        // 목표 제거함
                        _target = null;
                        // 공격중 아님
                        attacking = false;
                        // 추격중 아님
                        chasing = false;
                        return;
                    }
                    // 사거리에 있으면
                    if(InRange(_target, startRange))
                    {
                        if(!InView(_target))
                        {
                            if(model.GetDirection() == UnitDirection.LEFT)
                                model.GetMovableNode().SetDirection(UnitDirection.RIGHT);
                            else if(model.GetDirection() == UnitDirection.RIGHT)
                                model.GetMovableNode().SetDirection(UnitDirection.LEFT);
                        }
                        StartAttack(); // 공격 시작
                    }
                    
                    // 추격중이 아니라면
                    else if(!chasing)
                    {
                        // 이동 가능한 목표의 노드로 이동
                        model.MoveToMovable(_target.GetMovableNode());
                        // 맨 마지막 명령은 움직임 종료
                        model.commandQueue.GetCurrentCmd().SetEndCommand(EndMove);
                        // 움직임 애니메이션
                        if(breaked)
                            anim.BreakMove();
                        else
                            anim.NormalMove();
                        moving = true;
                        chasing = true;
                    }
                }
            }
            // 공격 중이고 내가 공간에 없으면(뭔지 모르겠음)
            if(screaming || attacking || model.GetMovableNode().currentPassage == null)
                return;
            
            // 적(직원, 환상체 포함)
            List<UnitModel> enemy = new List<UnitModel>();
            // 가능한 곳의 모든 직원을 찾음
            MovableObjectNode[] enteredTargets = model.GetMovableNode().currentPassage.GetEnteredTargets(model.GetMovableNode());
            foreach(MovableObjectNode movablenode in enteredTargets) // 이걸 순회함
            {
                // 체력이 0보다 높은 유닛 이면
                if(movablenode.GetUnit().hp > 0f)
                    enemy.Add(movablenode.GetUnit()); // 적 리스트에 추가
            }
            if(enemy.Count > 0) // 적 리스트가 비어있지 않다면
            {
                UnitModel unit = DistanceUnitUtil.SelectNearestUnit(model, enemy); // 가장 가까운 적 선택
                if(unit != null && _target != unit)
                {
                    if(firstTargeting)
                    {
                        Scream();
                        _target = unit;
                        chasing = true;
                        return;
                    }
                    _target = unit; // 목표를 설정
                    model.MoveToMovable(_target.GetMovableNode());
                    if(breaked)
                        anim.BreakMove();
                    else
                        anim.NormalMove();
                    chasing = true;
                }
            }
        }

        public bool InView(UnitModel target)
        {
            // 내 x
            float mx = model.GetCurrentViewPosition().x;
            // 목표 x
            float tx = target.GetCurrentViewPosition().x;

            // 왼쪽 바라보고 목표 x가 더 오른쪽이면
            if(model.GetDirection() == UnitDirection.LEFT && mx < tx)
                return false;
            // 오른쪽 바라보고 목표 x가 더 왼쪽이면
            if(model.GetDirection() == UnitDirection.RIGHT && mx > tx)
                return false;
            return true;
        }

        public bool InRange(UnitModel tar, float range)
        {
            // 거리는 함수 사용해 받아옴
            float dis = GetDis(tar);
            // true, false 반환
            return range >= dis;
        }

        private float GetDis(UnitModel tar)
        {
            // 목표 x 좌표
            float x = tar.GetCurrentViewPosition().x;
            // 내 x 좌표
            float x2 = model.GetCurrentViewPosition().x;
            // 변경할 수(기본값)
            float num = 1f;
            if(currentPassage != null)
                num = currentPassage.scaleFactor; // 축소, 확대 같은거
            return Mathf.Abs(x - x2) / num; // 계산
        }

        private void StartAttack()
        {
            if(_target != null && !attacking)
            {
                attcom++;
                if(attcom > 10)
                    attcom = 0;
                if(attcom < 10)
                {
                    // 다음 공격 쿨타임 3초
                    attacktimer.StartTimer(0.1f);
                    // 공격 중(false는 애니메이션 클래스에서 조정함)
                    attacking = true;
                    // 공격 애니메이션
                    if(attcom % 2 == 0)
                        anim.Attack1();
                    else
                        anim.Attack2();
                }
                if(attcom == 10)
                {
                    StopMoment();
                    attacktimer.StartTimer(1.3f);
                    attacking = true;
                    anim.Heavy();
                }
            }
        }

        private void StopMoment()
        {
            // 이건 애니메이션 클래스가 상속한 클래스의 Move임
            anim.Move();
            // 움직임 멈춤
            moving = false;
            // movable은 이 클래스가 상속한 클래스가 가긴 변수인데 이걸 해야 함
            movable.StopMoving();
            // 만약 지금 명령이 이동 명령이라면
            if(model.GetCurrentCommand() is MoveCreatureCommand)
                model.ClearCommand(); // 명령 제거
        }

        public override void OnFixedUpdate(CreatureModel creature)
        {
            base.OnFixedUpdate(creature);
            attacktimer.RunTimer(); // 타이머 시간 가게
        }

        public void KillUnit()
        {
            killCount++;
            combo.min = 10 + killCount;
            combo.max = 20 + killCount;
            heavy.min = 40 + killCount;
            heavy.max = 50 + killCount;
            speed = 4 + killCount;
            model.movementScale = speed;
            model.hp += model.baseMaxHp * 0.5f;
        }
    }
}