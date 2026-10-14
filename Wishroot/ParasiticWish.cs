using UnityEngine;
using System;
using System.Collections.Generic;

namespace Tuantu.WishrootCreature
{
    public class ParasiticWish : CreatureBase
    {
        public Wishroot root;
        private ParasiteAnim anim;
        private Timer attacktimer;
        public bool attacking;
        public bool chasing;
        public bool moving;
        private int attn;
        public UnitModel _target;
        public string name;

        private ChildCreatureModel Model => model as ChildCreatureModel;

        public override void OnViewInit(CreatureUnit unit)
        {
            base.OnViewInit(unit);
            anim = (ParasiteAnim)unit.animTarget;
            Model.SetDefenseId("1");
            attacktimer = new Timer();
            anim.SetScript(this);
            Escape();
        }

        public override void SetModel(CreatureModel model)
        {
            base.model = model;
            Setting(((ChildCreatureModel)model).parent.script as Wishroot);
        }

        public override void Escape()
        {
            base.Escape();
            model.Escape();
            AnyMove();
        }

        // 아무 움직임을 해야 할 때
        public void AnyMove()
        {
            // 현재 입력된 명령(공격, 이동 등)을 제거함
            model.ClearCommand();
            // 떠돌 노드(칸)을 지정해서 거기로 이동
            model.MoveToNode(MapGraph.instance.GetCreatureRoamingPoint());
            // 마지막 명령을 움직임 끝으로 지정
            model.commandQueue.GetCurrentCmd().SetEndCommand(EndMove);
            // 애니메이션 클래스에서 움직임을 하는 애니메이션으로 변경하라는 함수(직접 만듬)
            anim.OnMove();
            moving = true;
        }

        private void EndMove()
        {
            // 안 움직임
            moving = false;
        }

        public override void UniqueEscape()
        {
            try
            {
                base.UniqueEscape();
                // 가까운 직원 찾기
                model.CheckNearWorkerEncounting();

                // 목표 없으면 공격중 아님
                if(_target == null)
                    attacking = false;

                // 공격이나 움직이는 중이 아니라면 추격 안 함
                if(!attacking && !moving)
                {
                    chasing = false;
                    _target = null;
                    AnyMove();
                    return;
                }

                // 목표가 없지 않고 공격 쿨타임이 돌아 있다면
                if(_target != null && !attacktimer.started)
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
                    if(InRange(_target, 2.3f))
                    {
                        if(!InView(_target))
                        {
                            Debug.Log("not in view");
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
                        anim.OnMove();
                        moving = true;
                        chasing = true;
                    }
                }

                // 공격 중이고 내가 공간에 없으면(뭔지 모르겠음)
                if(attacking || model.GetMovableNode().currentPassage == null)
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
                    if(!attacktimer.started) // 공격 쿨타임이 아니라면(스탠의 공격 이후 쿨타임 돌때까지 추적 안 하는거 반영)
                    {
                        UnitModel unit = DistanceUnitUtil.SelectNearestUnit(model, enemy); // 가장 가까운 적 선택
                        if(unit != null && _target != unit)
                        {
                            _target = unit; // 목표를 설정
                            model.MoveToMovable(_target.GetMovableNode());
                            anim.OnMove();
                            chasing = true;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex);
            }
        }

        private void StartAttack()
        {
            // 목표 있고 이미 공격 중이 아닐 때
            if(_target != null && !attacking)
            {
                // 잠시 멈춤
                StopMoment();
                // 공격 타입 2개, 랜덤으로 뽑아 실행
                attn++;
                if(attn > 2)
                    attn = 0;
                // 다음 공격 쿨타임 3초
                attacktimer.StartTimer(2.5f);
                // 공격 중(false는 애니메이션 클래스에서 조정함)
                attacking = true;

                // 공격 애니메이션
                switch(attn)
                {
                    case 0:
                        anim.Attack1(); break;
                    case 1:
                        anim.Attack2(); break;
                    case 2:
                        anim.Attack3(); break;
                }
            }
        }

        private void StopMoment()
        {
            // 이건 애니메이션 클래스가 상속한 클래스의 Move임
            anim.Move();
            moving = false;
            // movable은 이 클래스가 상속한 클래스가 가긴 변수인데 이걸 해야 함
            movable.StopMoving();
            // 만약 지금 명령이 이동 명령이라면
            if(model.GetCurrentCommand() is MoveCreatureCommand)
                model.ClearCommand(); // 명령 제거
        }

        public bool InRange(UnitModel tar, float range)
        {
            if(tar != null)
            {
                float dis = GetDis(tar);
                // true, false 반환
                return range >= dis;
            }
            return false;
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
            return Math.Abs(x - x2) / num; // 계산
        }

        // 반대편에 있는지 확인(반대편이면 false)
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

        public override bool OnAfterSuppressed()
        {
            // 죽음 애니메이션
            anim.PlayDead();
            return true;
        }

        public void Setting(Wishroot master)
        {
            root = master;
        }

        public override void OnFixedUpdate(CreatureModel creature)
        {
            base.OnFixedUpdate(creature);
            attacktimer.RunTimer(); // 타이머 시간 가게
        }

        public override string GetName()
        {
            return name;
        }
    }
}