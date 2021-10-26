using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Project.AbilitySystem
{

    //All actions directly related to the ability itself take place here.
    public class AbilityObject : Ability
    {
        private GameObject clickObject;

        private UnitInfo unitInfo;

        private Image buttonImage;

        private TextMeshProUGUI cooldownText;

        private float currentCooldown = 0f;
        private float basicCooldown;
        private float fillAmountFloat;

        private bool isShowVisual;
        private bool onDelay;
        //The condition for whether the ability is visible on the panel.
        public bool isVisible { get; set; }

        private UseParams parameters;

        public AbilityObject(GameObject gameObject, AbilityData abilityData)
        {
            SetAbilityFromData(abilityData, gameObject?.GetComponent<UnitInfo>(), this);
        }

        #region SetRegion
        //Sets the new active cooldown time. Not tested yet!
        public void SetNewActiveCooldown(float newCooldown)
        {
            if (currentCooldown == 0)
                return;
            currentCooldown = newCooldown;
            basicCooldown = newCooldown;
            SetFillAmount(1f);
            SetVisualText(true);
        }

        //If the cooldown is active and switches to another unit and back, then the cooldown will be displayed again and at the same time it will be correct.
        public void SetActiveCooldown(Transform button, bool isShowVisual = true)
        {
            isVisible = true;
            this.isShowVisual = isShowVisual;
            clickObject = button.Find("KeyCodeText").gameObject;
            buttonImage = button.Find("CooldownDark").GetComponent<Image>();
            cooldownText = button.Find("CooldownText").GetComponent<TextMeshProUGUI>();

            if (buttonImage == null)
            {
                Debug.LogWarning("buttonImage in " + GetType().Name + " is null.");
                return;
            }
            if (currentCooldown > 0 && isShowVisual)
            {
                SetFillAmount(currentCooldown / basicCooldown);
                cooldownText.text = ((int)Mathf.Ceil(currentCooldown)).ToString();
                SetVisualText(true);
            }
            else
            {
                SetFillAmount(0f);
                SetVisualText(false);
            }
        }
        #endregion

        #region GetRegion
        public bool IsOnCooldown()
        {
            return currentCooldown > 0;
        }
        public bool IsOnDelay()
        {
            return onDelay;
        }
        #endregion

        //Launches ability
        public void Launch(UseParams p)
        {
            parameters = p;
            unitInfo = p.caster.GetComponent<UnitInfo>();
            CostRecourseRemove();
            onDelay = true;
            unitInfo.stopDelegate();
            if (p.target != null)
            {
                p.caster.TurnToSide(p.target.transform.position);
            }
            else if (p.point != Vector3.zero)
            {
                p.caster.TurnToSide(p.point);
            }
            unitInfo.SetTask(TaskList.Cast);
            unitInfo.StartCoroutine(Delay());
        }

        private bool CostRecourseRemove()
        {
            Ability ability = parameters.ability;
            switch (ability.costType)
            {
                case CostType.Mana:
                    unitInfo.RemoveMana(ability.cost);
                    break;
                case CostType.Health:
                    unitInfo.RemoveHealth(ability.cost);
                    break;
                    case CostType.Gold:
                    unitInfo.Owner.Gold -= ability.cost;
                    break;
            }
            return true;
        }

        //Delay before casting the ability and then casting.
        IEnumerator Delay()
        {
            yield return new WaitForSeconds(delay);
            onDelay = false;
            if (unitInfo.GetTask() != TaskList.Cast)
            {
                yield break;
            }
            unitInfo.stopDelegate();
            actionUse(parameters);
            if (cooldown == 0)
            {
                yield break;
            }
            currentCooldown = cooldown;
            basicCooldown = cooldown;
            SetFillAmount(1f);
            SetVisualText(true);
            unitInfo.StartCoroutine(CooldownTick());
        }

        //Cooldown Tick
        IEnumerator CooldownTick()
        {
            while (currentCooldown > 0)
            {
                CooldownChange();
                yield return null;
            }
            SetFillAmount(0);
            if (isVisible)
            {
                SetVisualText(false);
            }
        }

        //Reduces cooldown per frame
        void CooldownChange()
        {
            SetFillAmount(fillAmountFloat - ((1 / basicCooldown) * Time.deltaTime));
            currentCooldown -= Time.deltaTime;
            if (isVisible && isShowVisual)
            {
                int visibleTime = Mathf.CeilToInt(currentCooldown);
                if (visibleTime <= 0)
                {
                    SetVisualText(false);
                }
                else
                {
                    cooldownText.text = visibleTime.ToString();
                }
            }
        }

        //Sets the FillAmount.If the ability is not selected, but on cooldown, it will cooldown correctly.
        void SetFillAmount(float fillAmount)
        {
            fillAmountFloat = Mathf.Clamp(fillAmount, 0f, 1f);
            if (isVisible)
            {
                buttonImage.fillAmount = fillAmountFloat;
            }
        }

        //If true, hides hotkey text and displays cooldown text. If false, does the same, but vice versa.
        void SetVisualText(bool visual)
        {
            clickObject.SetActive(!visual);
            cooldownText.gameObject.SetActive(visual);
        }

        public void HideVisualCooldown()
        {
            isShowVisual = false;
            SetVisualText(false);
            SetFillAmount(0);
        }
    }
}
