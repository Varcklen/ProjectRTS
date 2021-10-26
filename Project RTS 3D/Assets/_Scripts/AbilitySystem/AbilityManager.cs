using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Project.Inventory;

namespace Project.AbilitySystem
{
    //This is where all the ability-related activities take place.
    public class AbilityManager
    {
        public AbilityObject[] abilities;
        public List<AbilityObject> abilitiesHide;
        public List<AbilityData> abilityData;

        //private GameObject buttons;
        private GameObject gameObject;
        private GameObject targetAreaPrefab;
        private GameObject targetArea;

        private UnitInfo unit;

        private AbilityObject ability;

        private Camera mainCam;

        private NavMeshAgent agent;

        private UI_Inventory inventoryUI;

        //true if the player chooses a target or area for the ability
        //Need to configure it locally?
        public static bool IsFindTarget;

        private const float SIZE_RATIO = 0.5f;//0.5-approximate size ratio. Can be corrected in the future. For 640x640 sprite
        private const string ARET_TARGET_PREFAB_PATH = "Other\\AreaTarget";

        //Creates abilities from those given in AbilityData
        public AbilityManager(UnitInfo unit, List<AbilityData> abilityData)
        {
            this.unit = unit;
            this.abilityData = abilityData;
            gameObject = unit.gameObject;
            agent = gameObject.GetComponent<NavMeshAgent>();
            abilities = new AbilityObject[16];
            abilitiesHide = new List<AbilityObject>();
            mainCam = Camera.main;
            inventoryUI = GameObject.Find("UI_Inventory").GetComponent<UI_Inventory>();
            //buttons = GameObject.Find("ButtonsUI");

            targetAreaPrefab = Resources.Load(ARET_TARGET_PREFAB_PATH) as GameObject;
            if (targetAreaPrefab == null)
            {
                Debug.LogWarning("targetAreaPrefab in " + GetType().Name + " is null. Please, check ARET_TARGET_PREFAB_PATH variable.");
            }

            if (abilityData.Count > 0)
            {
                for (int i = 0; i < abilityData.Count; i++)
                {
                    AddAbility(abilityData[i]);
                }
            }
        }

        public void OnDisable()
        {
            //if (Application.isPlaying)
            //{
            //    return;
            //}
            if (IsFindTarget)
            {
                IsFindTarget = false;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
                Cursor.visible = true;
            }
        }

        public AbilityObject AddAbility(AbilityData newAbility)
        {
            AbilityObject ability = new AbilityObject(gameObject, newAbility);
            if (ability.isHide)
            {
                abilitiesHide.Add(ability);
                abilitiesHide[abilitiesHide.Count - 1].abilityData.AddedToUnit(unit);
            }
            else
            {
                for (int i = 0; i < abilities.Length; i++)
                {
                    if (abilities[i] == null)
                    {
                        abilities[i] = ability;
                        abilities[i]?.abilityData.AddedToUnit(unit);
                        break;
                    }
                }
            }
            if (unit.GetIsSelected())
            {
                ButtonManager.Instance.UpdateButtons(unit);
            } 
            return ability;
        }

        public void RemoveAbility(AbilityObject ability)
        {
            if (ability == null)
            {
                return;
            }
            ability.abilityData.RemovedFromUnit(unit);
            if (abilities.Length > 0)
            {
                for (int i = 0; i < abilities.Length; i++)
                {
                    if (abilities[i] == ability)
                    {
                        Object.Destroy(abilities[i].component);

                        abilities[i] = null;
                        //Do I need to remove the ability? Destroy(ability)?
                        break;
                    }
                }
            }
            abilitiesHide.Remove(ability);
            if (unit.GetIsSelected())
            {
                ButtonManager.Instance.UpdateButtons(unit);
            } 
        }

        //Replaces an ability with another in the same slot.
        public AbilityObject ReplaceAbility(AbilityObject oldAbility, AbilityData newAbility)
        {
            if (newAbility == null || oldAbility == null)
                return null;
            AbilityObject ability = null;
            if (abilities.Length > 0)
            {
                for (int i = 0; i < abilities.Length; i++)
                {
                    if (abilities[i] == oldAbility)
                    {
                        Object.Destroy(abilities[i].component);
                        abilities[i] = null;
                        //Do I need to remove the ability? Destroy(ability)?
                        ability = new AbilityObject(gameObject, newAbility);
                        abilities[i] = ability;
                        break;
                    }
                }
            }
            if (abilitiesHide.Remove(oldAbility))
            {
                abilitiesHide.Add(new AbilityObject(gameObject, newAbility));
            }
            if (unit.GetIsSelected())
                ButtonManager.Instance.UpdateButtons(unit);
            return ability;
        }

        private bool IsCostResourceNotEnough(in AbilityObject ability)
        {
            switch (ability.costType)
            {
                case CostType.Mana:     
                    if (ability.cost > unit.GetMana()) return true;
                    break;
                case CostType.Health:   
                    if (ability.cost > unit.GetHealth()-1) return true;
                    break;
                case CostType.Gold:
                    if (ability.cost > unit.Owner.Gold) return true;
                    break;
            }
            return false;
        }

        //Activates the ability depending on the mode
        public void ButtonClick(AbilityObject ability)
        {
            if (ability == null)
            {
                return;
            }
            if (ability.IsOnCooldown())
            {
                Debug.LogWarning("Ability " + ability.name + " on cooldown.");
                return;
            }
            if (IsCostResourceNotEnough(ability))
            {
                Debug.LogWarning("Not enough cost recourse.");
                return;
            }
            if (!ability.isVisible || ability.isPassive || ability.IsOnDelay())
            {
                return;
            }
            this.ability = ability;
            //unit.SetTask(TaskList.Cast);
            if (this.ability.abilityTarget == AbilityTarget.Target)
            {
                TargetCheck();
            }
            else if (this.ability.abilityTarget == AbilityTarget.Area)
            {
                AreaCheck();
            }
            else
            {
                NoTarget();
            }
        }

        //All conditions for choosing a target
        bool TargetCheck(UnitInfo target)
        {
            ObjectType objectType = ability.targetType;
            if (target == null) return false;
            if ((target.GetObjectType() & objectType) == 0) return false;
            if (target == unit && !ability.targetType.HasFlag(ObjectType.Self)) return false;
            if (unit.IsUnitAllyToUnit(target) && !objectType.HasFlag(ObjectType.Ally)) return false;
            if (!unit.IsUnitAllyToUnit(target) && !objectType.HasFlag(ObjectType.Enemy)) return false;
            return true;
        }

        #region NoTarget
        void NoTarget()
        {
            UseAbility(new UseParams { caster = unit, ability = ability});
        }
        #endregion

        #region Target
        //Starts target selection
        void TargetCheck()
        {
            if (unit == null)
                return;
            SetVisual(isTargetCheckActive: true);
            unit.StartCoroutine(ClickCheck());
        }

        //Checks hit and ends checking
        IEnumerator ClickCheck()
        {
            while (ClickTargetCheck()) yield return null;
            SetVisual(isTargetCheckActive: false);
        }

        //Condition of pressing and target
        bool ClickTargetCheck()
        {
            if (Input.GetMouseButton(1))
                return false;
            if (!Input.GetMouseButton(0))
                return true;
            RaycastHit hit;
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 64) && !GlobalMethods.IsPointerOverUIObject())//64=Selectable
            {
                UnitInfo target = hit.collider.GetComponent<UnitInfo>();
                if (!TargetCheck(target))
                {
                    if (target == null)
                    {
                        Debug.LogWarning("Target in AbilityManager/ClickTargetCheck is null!");
                    }
                    else
                    {
                        Debug.Log("Wrong target");
                    }
                    return true;
                }
                var p = new UseParams
                {
                    caster = unit,
                    target = target.gameObject,
                    ability = ability,
                };
                UseAbility(p);
                return false;
            }
            return true;
        }

        //Sets all visual parameters for different UI
        void SetVisual(bool isTargetCheckActive)
        {
            IsFindTarget = isTargetCheckActive;
            inventoryUI.SetItemSlotsInteractable(!isTargetCheckActive);
            if (isTargetCheckActive)
            {
                Cursor.SetCursor(LoadResourceManager.Instance.cursorTargetSprite, Vector2.zero, CursorMode.ForceSoftware);
                ButtonManager.Instance.HideAllButtons();
            }
            else
            {
                ButtonManager.Instance.UpdateButtons(unit);
                Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
            }
        }
        #endregion

        #region Area
        void AreaCheck()
        {
            Cursor.visible = false;
            IsFindTarget = true;
            ButtonManager.Instance.HideAllButtons();
            inventoryUI.SetItemSlotsInteractable(false);

            targetArea = GameObject.Instantiate(targetAreaPrefab, Vector3.zero, Quaternion.identity);
            targetArea.transform.eulerAngles = new Vector3(-90, 0, 0);
            float boundX = targetArea.GetComponent<SpriteRenderer>().bounds.size.x * SIZE_RATIO;
            float scale = Mathf.Clamp(ability.area / boundX, 0, Mathf.Infinity);
            targetArea.transform.localScale = targetArea.transform.localScale * scale;

            unit.StartCoroutine(AreaCheckMove());
        }

        IEnumerator AreaCheckMove()
        {
            while (ClickAreaCheck())
            {
                yield return null;
            }
            Cursor.visible = true;
            GameObject.Destroy(targetArea);
            inventoryUI.SetItemSlotsInteractable(true);
            ButtonManager.Instance.UpdateButtons(unit);
            IsFindTarget = false;
        }

        //Condition of pressing and target
        bool ClickAreaCheck()
        {
            if (Input.GetMouseButton(1))
                return false;
            RaycastHit hit;
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 8))//8=Ground
            {
                if (!Input.GetMouseButton(0))
                {
                    targetArea.transform.position = hit.point + new Vector3(0, 0.1f, 0);
                    return true;
                }
                else if (!GlobalMethods.IsPointerOverUIObject())
                {
                    AreaLaunch(hit.point);
                    return false;
                }
            }
            return true;
        }

        private void AreaLaunch(Vector3 point)
        {
            List<UnitInfo> list = new List<UnitInfo>();
            Collider[] colliders = Physics.OverlapSphere(point, ability.area, GlobalMethods.GetSelectableMask());
            foreach (Collider collider in colliders)
            {
                if (!collider.TryGetComponent(out UnitInfo unitInfo))
                {
                    continue;
                }
                if (TargetCheck(unitInfo))
                {
                    list.Add(unitInfo);
                }
            }
            var p = new UseParams
            {
                caster = unit,
                point = point,
                ability = ability,
                areaTargets = list
            };
            UseAbility(p);
        }
        #endregion

        //If a unit can cast an ability due to range, it is used. Otherwise it goes to the target.
        void UseAbility(UseParams p)
        {
            unit.SetTask(TaskList.Cast);
            if (ability.distance == 0 || DistanceCondition(p))
            {
                ability.Launch(p);
            }
            else
            {
                unit.StartCoroutine(Move(p));
            }
        }

        //Depending on the type of target, will pass the condition.
        bool DistanceCondition(UseParams p)
        {
            switch (ability.abilityTarget)
            {
                case AbilityTarget.Target:
                    if (p.target.TryGetComponent(out UnitInfo unitInfo))
                    {
                        return ability.distance >= unit.DistanceBetweenUnits(unitInfo);
                    }
                    else
                    {
                        return false;
                    }
                case AbilityTarget.Area:
                    return ability.distance >= GlobalMethods.DistanceBetweenPoints(gameObject.transform.position, p.point);
            }
            return false;
        }

        //Depending on the type of target, forces the unit to go towards the target.
        void Destination(UseParams p)
        {
            switch (ability.abilityTarget)
            {
                case AbilityTarget.Target:
                    agent.destination = p.target.transform.position;
                    break;
                case AbilityTarget.Area:
                    agent.destination = p.point;
                    break;
            }
        }

        //Movement towards the target until the ability range is greater than the distance between units.
        IEnumerator Move(UseParams p)
        {
            unit.Animator.SetBool("IsMoved", true);
            while (AbilityCondition(p) && !DistanceCondition(p))
            {
                Destination(p);
                yield return null;
            }
            unit.Animator.SetBool("IsMoved", false);
            if (AbilityCondition(p) && DistanceCondition(p))
            {
                ability.Launch(p);
            }
            else
            {
                if (unit.stopDelegate != null)
                    unit.stopDelegate();
            }
        }

        //Basic conditions necessary for use ability to a target.
        bool AbilityCondition(UseParams p)
        {
            if (unit.GetTask() != TaskList.Cast)    return false;
            if (!unit.IsUnitAlive())    return false;
            if (p.target != null)
            {
                if (!p.target.GetComponent<UnitInfo>().IsUnitAlive())   return false;
            }
            return true;
        }

    }
}
