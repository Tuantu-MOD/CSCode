using UnityEngine;
using Harmony;
using LobotomyBaseMod;

namespace Tuantu.SicklerCreature
{
    public class ShadowHunterModel : CreatureModel
    {
        public AgentModel master;
        public bool hasGift;
        public SicklerWeapon weapon;

        public ShadowHunterModel(long id)
            :base(id)
        {
            base.state = CreatureState.ESCAPE;
            SetFaction(FactionTypeList.StandardFaction.EscapedCreature);
        }

        public static ShadowHunterModel MakeCreature(long id, AgentModel masterAgent, SicklerWeapon _weapon)
        {
            ShadowHunterModel hunterModel = new ShadowHunterModel(id);
            hunterModel.master = masterAgent;
            BuildCreature(hunterModel, new LcIdLong("Tuantu_MOD", 120008L));
            hunterModel.GetMovableNode().Assign(masterAgent.GetMovableNode());
            hunterModel.GetMovableNode().SetActive(true);
            hunterModel.baseMaxHp = hunterModel.metaInfo.maxHp;
            hunterModel.hp = hunterModel.baseMaxHp;
            AddUnit(hunterModel);
            hunterModel.script.OnInit();
            hunterModel.sefiraNum = (hunterModel.sefira = masterAgent.GetMovableNode().GetCurrentStandingSefira()).indexString;

            var CreatureList = AccessTools.Field(typeof(CreatureManager), "creatureList").GetValue(CreatureManager.instance) as System.Collections.Generic.List<CreatureModel>;
            CreatureList.Add(hunterModel);

            hunterModel.weapon = _weapon;

            return hunterModel;
        }
        
        private static void BuildCreature(ShadowHunterModel model, LcIdLong id)
        {
            CreatureTypeInfo data = CreatureTypeList.instance.GetData_Mod(id);
            model.metadataId = id.id;
            model.metaInfo = data;
            model.observeInfo = new CreatureObserveInfoModel(id.id);
            model.script = new ShadowHunter();
            model.script.SetModel(model);
            model.script.OnInitialBuild();
        }

        private static void AddUnit(ShadowHunterModel model)
        {
            CreatureUnit component = ResourceCache.instance.LoadPrefab("Unit/CreatureBase").GetComponent<CreatureUnit>();
            component.transform.SetParent(CreatureLayer.currentLayer.transform, false);
            component.model = model;
            model.SetUnit(component);
            component.returnObject = component.returnSpriteRenderer.gameObject;
            component.returnObject.SetActive(false);
        }

        public override string GetUnitName()
        {
            return metaInfo.collectionName;
        }

        public override bool IsHostile(UnitModel target)
        {
            if(hasGift)
            {
                return master.IsHostile(target);
            }
            return true;
        }

        public void SetCanvas()
        {
            Vector3 canvPosition = Unit.currentCreatureCanvas.transform.position;
            Unit.currentCreatureCanvas.transform.position = new Vector3(canvPosition.x, -2.7f, canvPosition.z);
        }

        public override void OnFixedUpdate()
        {
            base.UpdateBufState();
            tempAnim.OnFixedUpdate();
            if (_equipment.weapon != null)
            {
                _equipment.weapon.OnFixedUpdate();
            }
            commandQueue.Execute(this);
            if (!script.UniqueMoveControl())
            {
                if (base.GetMovableNode().IsMoving())
                {
                    base.SetMoveAnimState(b: true);
                }
                else if (_unit != null && _unit.animTarget != null)
                {
                    _unit.animTarget.StopMoving();
                }
            }
            if (GameManager.currentGameManager.ManageStarted)
            {
                script.OnFixedUpdate(this);
            }
            movableNode.ProcessMoveNode(metaInfo.speed * movementScale);
        }

        public void OnDestroy()
        {
            ClearCommand();
            GetMovableNode().SetActive(false);
            if(base.Unit != null)
                Object.Destroy(base.Unit.gameObject);
            weapon.spawn = false;
        }
    }
}