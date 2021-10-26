using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Project.AbilitySystem
{
    public class AbilitySlot : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public AbilityObject Ability { get; private set; }

        private RectTransform upgradePlus;

        private UnitInfo objectInfo;

        private Image image;

        private KeyCode key = KeyCode.None;

        private TextMeshProUGUI keyCodeText;

        private TooltipPopup tooltipPopup;

        private CanvasGroup canvasGroup;

        private bool isCanUsed = true;
        private bool isCanUpgrade;

        private const string GAMEOBJECT_NAME_TOOLTIP = "Tooltip Popup";
        private const string GAMEOBJECT_NAME_UPGRADEPLUS = "UpgradePlus";

        private void Start()
        {
            //Debug.Log("AbilitySlot::Start() - called. gameObject.name = " + gameObject.name);
            image = GetComponent<Image>();
            keyCodeText = transform.Find("KeyCodeText")?.GetComponent<TextMeshProUGUI>();
            tooltipPopup = GameObject.Find(GAMEOBJECT_NAME_TOOLTIP)?.GetComponent<TooltipPopup>();
            if (tooltipPopup == null)
            {
                Debug.LogWarning("GameObject \"" + GAMEOBJECT_NAME_TOOLTIP + "\" does not exist on stage. Tooltip capabilities are not available.");
            }
            upgradePlus = transform.Find(GAMEOBJECT_NAME_UPGRADEPLUS)?.GetComponent<RectTransform>();
            if (upgradePlus == null)
            {
                Debug.LogWarning("GameObject \"" + GAMEOBJECT_NAME_UPGRADEPLUS + "\" does not exist on stage. PlusUI on button are not available.");
            }
            canvasGroup = transform.GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(key))
            {
                ButtonClick();
            }
        }

        //Sets all information about a button
        public void SetData(AbilityObject ability, UnitInfo objectInfo, KeyCode key)
        {
            Ability = ability;
            this.key = key;
            if (key != KeyCode.None && !ability.isPassive)
            {
                if(keyCodeText != null)
				{
                    keyCodeText.enabled = true;
                    keyCodeText.SetText(ColorText.KeyCodeToString(key));
                }
                else
				{
                    Debug.LogWarning("keyCodeText was null");
                }
            }
            else if(keyCodeText != null)
            {
                keyCodeText.enabled = false;
            }

            this.objectInfo = objectInfo;

            if(image != null)
			{
                image.sprite = ability?.icon;
            }
            else
			{
                Debug.Log("AbilitySlot::SetData() - image is null.");
            }
        }

        //Fires when a button is pressed
        public void OnPointerClick(PointerEventData eventData)
        {
            ButtonClick();
        }

        //Fires when the cursor enters from the button.
        public void OnPointerEnter(PointerEventData eventData)
        {
            tooltipPopup?.DisplayTooltip(Ability, new DescriptionInfo { key = key });
        }

        //Fires when the cursor leaves the button.
        public void OnPointerExit(PointerEventData eventData)
        {
            tooltipPopup?.HideTooltip();
        }

        //Fires a button trigger
        void ButtonClick()
        {
            if (!isCanUsed)
            {
                return;
            }
            if (isCanUpgrade)
            {
                if (objectInfo is HeroInfo heroInfo)
                {
                    heroInfo.UpgradeAbility(Ability, key);
                }
                else
                {
                    Debug.LogWarning("Unit " + objectInfo.gameObject + " is not a hero. You cannot upgrade ability.");
                }
                return;
            }
            tooltipPopup?.HideTooltip();
            objectInfo.abilityManager.ButtonClick(Ability);
        }

        ///<summary>
        ///Turns on the button regardless of what level the ability is, whether the unit is stunned or silenced.
        ///To enable abilities with condition, use HideButtonsForUpgrade().
        ///</summary>
        private void EnableAnyButton()
        {
            isCanUsed = true;
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
            }
            else
            {
                Debug.LogWarning("AbilitySlot::EnableButton() - canvasGroup is null.");
            }
        }

        ///<summary>
        ///Disables the button.
        ///</summary>
        public void DisableButton()
        {
            isCanUsed = false;
            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
            }
            else
            {
                Debug.LogWarning("AbilitySlot::DisableButton() - canvasGroup is null.");
            }
        }

        #region Upgrade Ability
        //If the ability is not learned (level 0), makes it inactive.
        public void SetButtonActivness(bool isUpgradeMode)
        {
            if (!isUpgradeMode)
            {
                if (Ability.level == 0)
                {
                    DisableButton();
                }
                else
                {
                    EnableAnyButton();
                }
            }
        }

        ///<summary>
        ///Includes a button whose ability is level 0 and can be upgraded. All other buttons are disabled.
        ///</summary>
        public void RevealButtonForUpgrade()
        {
            //Debug.Log("AbilitySlot::RevealButtonsForUpdate() - called.");

            if (Ability == null)
            {
                return;
            }
            Ability.HideVisualCooldown();
            if (Ability.isUpgraded && Ability.upgradeLimit > Ability.level)
            {
                upgradePlus?.gameObject?.SetActive(true);
                isCanUpgrade = true;
                EnableAnyButton();
            }
            else
            {
                upgradePlus?.gameObject?.SetActive(false);
                isCanUpgrade = false;
                DisableButton();
            }
        }

        ///<summary>
        ///Enables a button whose ability is level 1 or higher, the unit is not stunned, not silenced, and so on.
        ///</summary>
        public void EnableButton()
        {
            //Debug.Log("AbilitySlot::HideButtonsForUpdate() - called.");
            if (upgradePlus != null)
			{
                upgradePlus.gameObject.SetActive(false);
            }
            else
			{
                Debug.LogWarning("AbilitySlot::HideButtonsForUpdate() - upgradePlus is null.");
            }
            if (Ability?.level > 0)
            {
                EnableAnyButton();
            }
            else
            {
                DisableButton();
            }
            isCanUpgrade = false;
        }
        #endregion
    }
}
