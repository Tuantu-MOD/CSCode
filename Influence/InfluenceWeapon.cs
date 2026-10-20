using UnityEngine;
using System.Collections.Generic;
using System;
using Spine;
using Spine.Unity;

namespace Tuantu.InfluenceCreature
{
    public class InfluenceWeapon : EquipmentScriptBase
    {
        private const float _skillCoolTime = 30f;
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
        float upSpikeTime = 0.4f;
        Timer _spikeTimer = new Timer();
        private SkeletonDataAsset _spikeData;
        private Vector3 _spikePos;
        private Vector3 _spikeEndPos;
        private float _spikeMoveX;
        private float _spikeScale;
        private bool _spikeLookRight;
        private int _spikesLeft;
        private const int _maxSpikes = 5;
        private const float _spikeGap = 3.0f;
        private const float _spikeSizeMultiplier = 2.5f;
        private float _normalRange;
        private const float _spikeLifeTime = 3.0f;
        private float _spikeSpacing;
        private PassageObjectModel _spikePassage;
        private List<GameObject> _spikeObjects = new List<GameObject>();

        private void Init()
        {
            worker = base.model.owner as WorkerModel;
            defaultInfo.range = 2f;
            defaultInfo.type = SplashType.SPLASH;
            skillInfo.iff = false;
            _isSpecialAttack = false;
            movable = worker.GetMovableNode();
            currentPassagg = movable.GetPassage();
            if (_normalRange <= 0f)
            {
                _normalRange = base.model.metaInfo.range;
            }
        }

        private void SetEffectActive(bool state)
        {
            if (_skillEffect == null) return;
            _skillEffect.SetActive(state);
        }

        private void ClearEffect()
        {
            if (_skillEffect == null) return;
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
            foreach (GameObject spike in _spikeObjects)
            {
                if (spike != null) UnityEngine.Object.Destroy(spike);
            }
            _spikeObjects.Clear();
            _spikesLeft = 0;
            if (_normalRange > 0f)
            {
                base.model.metaInfo.range = _normalRange;
            }
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
                _skillCoolTimer.StartTimer(_skillCoolTime);
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
            if (!Add_On.instance.EffectList.TryGetValue("Influence_Spike", out var value))
            {
                return;
            }
            Vector3 viewPos = base.model.owner.GetCurrentViewPosition();

            _spikeData = value;
            _spikeLookRight = movable.GetDirection() == UnitDirection.RIGHT;
            _spikeScale = movable.currentScale * _spikeSizeMultiplier;
            _spikeSpacing = GetSpikeWidth(value, _spikeScale);
            _spikeMoveX = _spikeLookRight ? _spikeSpacing : -_spikeSpacing;
            _spikePassage = movable.GetPassage();
            _spikePos = new Vector3(viewPos.x, viewPos.y - 1f, viewPos.z - 5f);
            _spikeEndPos = GetPassageEndInFront();
            _spikesLeft = _maxSpikes;

            SpawnNextSpike();
        }

        private void SpawnNextSpike()
        {
            bool passedEnd = _spikeLookRight ? _spikePos.x > _spikeEndPos.x : _spikePos.x < _spikeEndPos.x;
            bool isFirst = _spikesLeft == _maxSpikes;
            if (_spikeData == null || (passedEnd && !isFirst))
            {
                _spikesLeft = 0;
                return;
            }

            SpawnSpike(_spikeData, _spikePos, _spikeScale, _spikeSpacing * 0.5f);
            _spikePos.x += _spikeMoveX;
            _spikesLeft--;

            if (_spikesLeft > 0)
            {
                _spikeTimer.StartTimer(upSpikeTime);
            }
        }

        private void SpawnSpike(SkeletonDataAsset data, Vector3 pos, float scale, float halfWidth)
        {
            GameObject effect = SkeletonAnimation.NewSkeletonAnimationGameObject(data).gameObject;
            SkeletonAnimation animator = effect.GetComponent<SkeletonAnimation>();

            effect.transform.SetParent(EffectLayer.currentLayer.transform);
            effect.transform.position = pos;
            effect.transform.localScale = new Vector3(scale, scale, 2f);

            _spikeObjects.RemoveAll(o => o == null);
            _spikeObjects.Add(effect);
            UnityEngine.Object.Destroy(effect, _spikeLifeTime);

            float spikeX = pos.x;
            PassageObjectModel passage = _spikePassage;
            TrackEntry entry = animator.AnimationState.SetAnimation(0, "Spike", false);

            UnityEngine.Debug.Log("[InfluenceSpike] spawn x=" + spikeX + " halfWidth=" + halfWidth + " passage=" + (passage == null ? "null" : "ok"));

            entry.Event += (trackEntry, e) =>
            {
                UnityEngine.Debug.Log("[InfluenceSpike] event=" + e.Data.Name);
                if (e.Data.Name == "Damage")
                {
                    List<UnitModel> targets = GetEnemiesOnSpike(passage, spikeX, halfWidth);
                    UnityEngine.Debug.Log("[InfluenceSpike] targets=" + targets.Count);
                    foreach (UnitModel item in targets)
                    {
                        item.TakeDamage(base.model.owner, base.model.metaInfo.damageInfos[1].Copy());
                    }
                }
            };
        }

        private List<UnitModel> GetEnemiesOnSpike(PassageObjectModel passage, float spikeX, float halfWidth)
        {
            List<UnitModel> result = new List<UnitModel>();
            if (passage == null) return result;

            foreach (MovableObjectNode node in passage.GetEnteredTargets(movable))
            {
                UnitModel unit = node.GetUnit();
                if (unit == null) continue;
                UnityEngine.Debug.Log("[InfluenceSpike] in passage: " + unit.GetType().Name + " hp=" + unit.hp + " x=" + unit.GetCurrentViewPosition().x);
                if (unit.hp <= 0f || !IsSpikeTarget(unit)) continue;

                float x = unit.GetCurrentViewPosition().x;
                if (Mathf.Abs(x - spikeX) <= halfWidth + unit.radius)
                {
                    result.Add(unit);
                }
            }
            return result;
        }

        private bool IsSpikeTarget(UnitModel unit)
        {
            if (unit is CreatureModel) return true;

            if (unit is WorkerModel target && target != worker)
            {
                if (!target.IsAttackTargetable()) return false;
                if (target.IsPanic()) return true;
                return worker != null && worker.IsHostile(target);
            }
            return false;
        }

        private float GetSpikeWidth(SkeletonDataAsset data, float scale)
        {
            SkeletonData skeletonData = data.GetSkeletonData(true);
            if (skeletonData == null || skeletonData.Width <= 0f) return _spikeGap;
            return skeletonData.Width * data.scale * scale;
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
            UpdateAttackRange();
            if (_spikesLeft > 0)
            {
                _spikeTimer.RunTimer();
                if (!_spikeTimer.started)
                {
                    SpawnNextSpike();
                }
            }
            if (_skillCoolTimer.started)
            {
                if(_isBattle && _skillEffect != null && _skillEffect.activeInHierarchy)
                {
                    SetEffectActive(state:false);
                }
                _skillCoolTimer.RunTimer();
            }
            else if(_isBattle && _skillEffect != null && !_skillEffect.activeInHierarchy)
            {
                SetEffectActive(state:true);
            }
        }

        private void UpdateAttackRange()
        {
            if (movable == null || _normalRange <= 0f) return;

            bool skillReady = !_skillCoolTimer.started && _spikesLeft == 0;
            float spikeScale = movable.currentScale * _spikeSizeMultiplier;
            float spikeWidth = _spikeGap;
            if (Add_On.instance.EffectList.TryGetValue("Influence_Spike", out var data))
            {
                spikeWidth = GetSpikeWidth(data, spikeScale);
            }
            float spikeReach = spikeWidth * _maxSpikes;
            base.model.metaInfo.range = skillReady ? spikeReach : _normalRange;
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