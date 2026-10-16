using UnityEngine;
using System.Collections.Generic;
using System;
using Spine;
using Spine.Unity;

namespace Tuantu.InfluenceCreature
{
    public class InfluenceWeapon : EquipmentScriptBase
    {
        private const float _skillCoolTime = 60f;
        private Timer _skillCoolTimer = new Timer();
        private AutoTimer _closeTimer;
        private WorkerModel worker;
        private bool _isSpecialAttack;
        private GameObject _skillEffect;
        private SplashInfo defaultInfo = new SplashInfo();
        private SplashInfo skillInfo = new SplashInfo();
        private bool _isBattle = true;
        private MovableObjectNode movable;
        private PassageObjectModel currentPassagg;
        int attStack;

        private void Init()
        {
            worker = base.model.owner as WorkerModel;
            defaultInfo.range = 2f;
            defaultInfo.type = SplashType.SPLASH;
            skillInfo.iff = false;
            _isSpecialAttack = false;
            movable = worker.GetMovableNode();
            currentPassagg = movable.GetPassage();
        }

        private void SetEffectActive(bool state)
        {
            _skillEffect.SetActive(state);
        }

        private void ClearEffect()
        {
            UnityEngine.Object.Destroy(_skillEffect);
            _skillEffect = null;
        }

        public override void OnStageStart()
        {
            base.OnStageStart();
            Init();
            SetEffectActive(state:false);
        }

        public override void OnStageRelease()
        {
            base.OnStageRelease();
            ClearEffect();
        }

        public override void OnPrepareWeapon(UnitModel actor)
        {
            _isBattle = true;
        }

        public override void OnCancelWeapon(UnitModel actor)
        {
            _isBattle = false;
            SetEffectActive(state:false);
        }

        public void CloseSkill()
        {
            if (_isSpecialAttack)
            {
                _skillCoolTimer.StartTimer(60f);
                _isSpecialAttack = false;
            }
        }

        public override WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
        {
            List<DamageInfo> list = new List<DamageInfo>();
            string animname = string.Empty;
            List<int> list2 = new List<int>();
            
            list.Add(base.model.metaInfo.damageInfos[0].Copy());

            if (!_skillCoolTimer.started)
            {
                animname = base.model.metaInfo.animationNames[1];
                MakeSpike();

                _isSpecialAttack = true;
            }
            else
            {
                animname = base.model.metaInfo.animationNames[0];
            }
            

            
            AutoTimer autoTimer = new AutoTimer();
            autoTimer.Init();
            autoTimer.StartTimer(5f, CloseSkill);
            _closeTimer = autoTimer;

            return new WeaponDamageInfo(animname, list.ToArray());
        }

        private void MakeSpike()
        {
            float moveX = 3.0f;
            if (!Add_On.instance.EffectList.TryGetValue("Influence_Spike", out var value))
            {
                return;
            }
            GameObject effect = SkeletonAnimation.NewSkeletonAnimationGameObject(value).gameObject;
            SkeletonAnimation animator = effect.GetComponent<SkeletonAnimation>();
            CustomEffect component = effect.GetComponent<CustomEffect>();
            Vector3 viewPos = base.model.owner.GetCurrentViewPosition();
            UnitDirection unit  = movable.GetDirection();
            float currentScale = ((WorkerModel)base.model.owner).GetMovableNode().currentScale;
            
            Vector3 pos = new Vector3(viewPos.x, viewPos.y -1f, viewPos.z -5f);
            Vector3 endPos = GetPassageEndInFront();
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "Spike", false);

            while (true)
            {
                if(Mathf.Abs(pos.x) >= Mathf.Abs(endPos.x))
                {
                    break;
                }
                effect.transform.SetParent(EffectLayer.currentLayer.transform);
                effect.transform.localScale = new Vector3(currentScale, currentScale, 2f);
                effect.transform.position = pos;
                List<CreatureModel> list = GetCreaturesInRange(pos, currentScale);
                entry.Event += (TrackEntry, e) =>
                {
                    if(e.Data.Name == "Dmage")
                    {
                        foreach(UnitModel item in list){
                            item.TakeDamage(base.model.metaInfo.damageInfos[1].Copy());
                        }
                    }
                };
                pos.x += moveX;
            };
            entry.Complete += (trackEntry) =>
            {
                UnityEngine.Object.Destroy(effect);
            };
        }

        public static List<CreatureModel> GetCreaturesInRange(Vector3 center, float radius)
        {
            List<CreatureModel> result = new List<CreatureModel>();
            float sqrRadius = radius * radius;

            foreach (CreatureModel creature in CreatureManager.instance.GetCreatureList())
            {
                if (IsInRange(creature, center, sqrRadius)) result.Add(creature);
            }

            return result;
        }

        private static bool IsInRange(UnitModel unit, Vector3 center, float sqrRadius)
        {
            Vector3 pos = unit.GetCurrentViewPosition();
            Vector2 diff = new Vector2(pos.x - center.x, pos.y - center.y);
            return diff.sqrMagnitude <= sqrRadius;
        }

        public override void OnAttackEnd(UnitModel actor, UnitModel target)
        {
            base.OnAttackEnd(actor, target);
            CloseSkill();
            if(_closeTimer != null)
            {
                AutoTimer.Destroy(_closeTimer);
            }
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if(GameManager.currentGameManager.state != 0)
            {
                return;
            }
            if (_skillCoolTimer.started)
            {
                if(_isBattle && _skillEffect.activeInHierarchy)
                {
                    SetEffectActive(state:false);
                }
                if (!_skillCoolTimer.RunTimer())
                {
                }
            }
            else if(_isBattle && !_skillEffect.activeInHierarchy)
            {
                SetEffectActive(state:true);
            }
        }

        private Vector3 GetPassageEndInFront()
        {
            PassageObjectModel passage = movable.GetPassage();
            Vector3 myPos = movable.GetCurrentViewPosition();
            if (passage == null) return myPos;

            bool lookRight = movable.GetDirection() == UnitDirection.RIGHT;
            Vector3 end = myPos;

            foreach (MapNode node in passage.GetNodeList())
            {
                Vector3 nodePos = node.GetPosition();
                if (lookRight ? nodePos.x > end.x : nodePos.x < end.x)
                {
                    end = nodePos;
                }
            }

            return end;
        }
    }
}    