using UnityEngine;
using UnityEngine.UI;
using Spine;
using Spine.Unity;
using System.Collections.Generic;

namespace Tuantu.SicklerCreature
{
    public class ShadowHunter : CreatureBase
    {
        private ShadowHunterAnim anim;
        public bool moving;
        public bool attacking;
        public bool chasing;
        public bool haunting;
        private bool chasingmaster;
        private bool isdie;
        private bool idle;
        
        public UnitModel _target;

        private Timer attacktimer = new Timer();
        private Timer exctimer = new Timer();
        private Timer dietime = new Timer();
        private Timer roamTimer = new Timer();
        private Timer autoDieTimer = new Timer();
        public DamageInfo att1dmg;
        public DamageInfo att2dmg;
        public DamageInfo heavydmg;
        private DamageInfo excutiondmg;
        private int attcom;
        public float range;
        private float startRange;

        public ShadowHunterModel Model => base.model as ShadowHunterModel;

        public override void OnViewInit(CreatureUnit unit)
        {
            haunting = true;
            base.OnViewInit(unit);
            GameObject gameObject = SkeletonAnimation.NewSkeletonAnimationGameObject(Add_On.instance.EffectList["Sickler_ShadowHunter"]).gameObject;
            gameObject.transform.SetParent(base.Unit.transform, false);
            anim = gameObject.AddComponent<ShadowHunterAnim>();
            anim.SetScript(this);
            base.Unit.animTarget = anim;
            if(Model.hasGift)
            {
                base.Unit.escapeCreatureName.color = Color.black;
                base.Unit.hpFill.color = Color.green;
            }
            base.Unit.escapeUIRoot.transform.localPosition = new Vector3(0f, 2.1f, 0f);
            base.Unit.escapeUIRoot.transform.localScale *= 0.5f;

            att1dmg = new DamageInfo(RwbpType.R, 10, 24);
            att2dmg = new DamageInfo(RwbpType.R, 10, 18);
            heavydmg = new DamageInfo(RwbpType.B, 40, 60);
            excutiondmg = new DamageInfo(RwbpType.R, 50);
            attcom = 0;
            range = 3.4f;
            startRange = 2.2f;
            ((ShadowHunterModel)model).SetCanvas();
            autoDieTimer.StartTimer(450f);

            StopMoment();
            anim.Haunt();
        }

        public override void Escape()
        {
            base.Escape();
            AnyMove();
        }

        public override bool OnAfterSuppressed()
        {
            // 죽음 애니메이션
            anim.PlayDead();
            dietime.StartTimer(10f);
            isdie = true;
            return true;
        }

        private void StopMoment()
        {
            // 이건 애니메이션 클래스가 상속한 클래스의 Move임
            anim.Move();
            anim.Idle();
            // 움직임 멈춤
            moving = false;
            // movable은 이 클래스가 상속한 클래스가 가긴 변수인데 이걸 해야 함
            movable.StopMoving();
            // 만약 지금 명령이 이동 명령이라면
            if(model.GetCurrentCommand() is MoveCreatureCommand)
                model.ClearCommand(); // 명령 제거
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

        public override string GetName()
        {
            return model.metaInfo.name;
        }

        public override string GetRiskLevel()
        {
            return "WAW";
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
                    attacktimer.StartTimer(1.7f);
                    // 공격 중(false는 애니메이션 클래스에서 조정함)
                    attacking = true;
                    float attN = UnityEngine.Random.Range(0.0f, 1.0f);
                    StopMoment();
                    // 공격 애니메이션
                    if(attN < 0.6f)
                        anim.Attack1();
                    else
                        anim.Attack2();
                }
                if(attcom == 10)
                {
                    StopMoment();
                    attacktimer.StartTimer(2.6f);
                    attacking = true;
                    anim.Heavy();
                }
            }
        }

        public override void OnFixedUpdate(CreatureModel creature)
        {
            base.OnFixedUpdate(creature);
            if(!haunting)
            {
                UniqueEscape();

                attacktimer.RunTimer(); // 타이머 시간 가게
                exctimer.RunTimer();
                dietime.RunTimer();
                roamTimer.RunTimer();
                autoDieTimer.RunTimer();
            }
        }

        public void AnyMove()
        {
            model.ClearCommand();
            model.MoveToMovable(Model.master.GetMovableNode());
            model.commandQueue.GetCurrentCmd().SetEndCommand(EndMove);
            anim.OnMove();
            moving = true;
        }

        private void EndMove()
        {
            moving = false;
            if(!attacking && !idle)
            {
                idle = true;
                anim.Idle();
            }
        }

        public override bool IsSuppressable()
        {
            if(Model.hasGift)
            {
                bool flag = false;
                if (!Model.master.IsDead())
                {
                    if (Model.master.CannotControll() && Model.master.unconAction != null && Model.master.unconAction.HasUniqueHostility())
                    {
                        flag = Model.master.unconAction.IsHostile();
                    }
                    else if (Model.master.IsPanic() && Model.master.CurrentPanicAction is PanicViolence)
                    {
                        flag = true;
                    }
                }
                return base.IsSuppressable() && flag;
            }
            return base.IsSuppressable();
        }

        public override bool IsAutoSuppressable()
        {
            if(Model.hasGift)
            {
                bool flag = false;
                if (!Model.master.IsDead())
                {
                    if (Model.master.CannotControll() && Model.master.unconAction != null && Model.master.unconAction.HasUniqueHostility())
                    {
                        flag = Model.master.unconAction.IsHostile();
                    }
                    else if (Model.master.IsPanic() && Model.master.CurrentPanicAction is PanicViolence)
                    {
                        flag = true;
                    }
                }
                return base.IsAutoSuppressable() && flag;
            }
            return base.IsAutoSuppressable();
        }

        public void KillUnit()
        {
            ((WorkerModel)Model.master).RecoverHP(Model.master.maxHp * 0.25f);
            Model.weapon.OnKillByHunter();
            attcom = 10;
        }

        public override void UniqueEscape()
        {
            base.UniqueEscape();
            // 가까운 직원 찾기
            if(!Model.hasGift)
                model.CheckNearWorkerEncounting();

            if(isdie && !dietime.started)
            {
                Model.OnDestroy();
            }
            
            if(isdie)
                return;

            if((((WorkerModel)Model.master).IsDead() && model.hp > 0 || !autoDieTimer.started) && !exctimer.started)
            {
                model.TakeDamage(model, excutiondmg);
                exctimer.StartTimer(0.3f);
                return;
            }
            
            if(((UnitModel)Model.master).GetMovableNode().currentPassage != null && ((UnitModel)Model.master).GetMovableNode().currentPassage != model.GetMovableNode().currentPassage && !chasingmaster)
            {
                _target = null;
                chasing = false;
                chasingmaster = true;
                AnyMove();
                return;
            }

            if(((UnitModel)Model.master).GetMovableNode().currentPassage == model.GetMovableNode().currentPassage)
            {
                chasingmaster = false;
            }

            // 목표 없으면 공격중 아님
            if(_target == null)
            {
                attacking = false;
            }

            // 공격이나 움직이는 중이 아니라면 추격 안 함
            if(!attacking && !moving)
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
                AnyMove();
                return;
            }

            if(attacking || model.GetMovableNode().currentPassage == null)
                return;

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
                    anim.OnMove();
                    moving = true;
                    chasing = true;
                }
            }
            
            if(!chasingmaster)
            {
                // 적(직원, 환상체 포함)
                List<UnitModel> enemy = new List<UnitModel>();
                // 가능한 곳의 모든 직원을 찾음
                MovableObjectNode[] enteredTargets = model.GetMovableNode().currentPassage.GetEnteredTargets(model.GetMovableNode());
                foreach(MovableObjectNode movablenode in enteredTargets) // 이걸 순회함
                {
                    // 체력이 0보다 높은 유닛 이면
                    if(movablenode.GetUnit().hp > 0f && model.IsHostile(movablenode.GetUnit()))
                        enemy.Add(movablenode.GetUnit()); // 적 리스트에 추가
                }
                if(enemy.Count > 0) // 적 리스트가 비어있지 않다면
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
    }
}