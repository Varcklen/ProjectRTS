using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Project.AbilitySystem
{
    //Creates buttons on the button panel
    public class ButtonManager : MonoBehaviour
    {
        [SerializeField] private KeyCode[] buttonKeys = new KeyCode[BUTTONS_COUNT];

        public static ButtonManager Instance { get; private set; }

        public bool isUpgradeMode { get; private set; }

        private AbilitySlot[] buttons;

        private GameObject abilitySpacer;

        private TextMeshProUGUI upgradePlus;

        private bool[] isFill;

        private int filled;

        private List<AbilityObject> noSlotAbility = new List<AbilityObject>();

        private const int BUTTONS_COUNT = 16;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        private void Start()
        {
            buttons = new AbilitySlot[BUTTONS_COUNT];
            isFill = new bool[BUTTONS_COUNT];
            abilitySpacer = GameObject.Find("ButtonsUI").gameObject;
            upgradePlus = GameObject.Find("PlusLevelButtonText").GetComponent<TextMeshProUGUI>();
            CreateButtons();
        }

        //Creates buttons for the panel
        private void CreateButtons()
        {
            for (int i = 0; i < BUTTONS_COUNT; i++)
            {
                buttons[i] = abilitySpacer.transform.GetChild(i).GetComponent<AbilitySlot>();
                buttons[i].gameObject.SetActive(false);
            }
        }

        //Hide all buttons
        public void HideAllButtons()
        {
            List<GameObject> childs = GlobalMethods.GetChildrens(abilitySpacer);
            foreach (GameObject child in childs)
            {
                child.SetActive(false);
            }
        }

        public void DisableAllButtons()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null)
                {
                    continue;
                }
                buttons[i].DisableButton();
            }
        }

        public void EnableAllButtons()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null)
                {
                    continue;
                }
                buttons[i].EnableButton();
            }
        }

        //Updates information on buttons
        public void UpdateButtons(UnitInfo unit)
        {
            if (!unit.GetIsSelected())
            {
                return;
            }

            HideAllButtons();
            filled = 0;
            for (int i = 0; i < BUTTONS_COUNT; i++)
            {
                isFill[i] = false;
                AbilityObject oldAbility = buttons[i].GetComponent<AbilitySlot>().Ability;
                if (oldAbility != null)
                {
                    oldAbility.isVisible = false;
                }
                buttons[i].gameObject.SetActive(false);
            }
            noSlotAbility.Clear();

            AbilityObject[] abilities = unit.abilityManager.abilities;
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i] == null)
                {
                    continue;
                }

                int k = abilities[i].buttonPosition;
                if (isFill[k])
                {
                    noSlotAbility.Add(abilities[i]);
                }
                else
                {
                    RevealSlot(k, abilities[i], unit);
                }
            }

            if (noSlotAbility.Count > 0)
            {
                for (int i = 0; i < noSlotAbility.Count; i++)
                {
                    if (filled >= BUTTONS_COUNT)
                    {
                        break;
                    }
                    FindEmptySlot(noSlotAbility[i], unit);
                }
            }

            if (!isUpgradeMode)
            {
                if (GetButtonActivness(unit))
                {
                    EnableAllButtons();
                }
                else
                {
                    DisableAllButtons();
                }
            }
        }

        private bool GetButtonActivness(UnitInfo unit)
        {
            if (unit.IsSilenced) return false;
            if (unit.IsStunned) return false;
            return true;
        }

        //If the ability slot is already occupied, it finds for another slot nearby (in priority it searches for the next slots, and then the previous ones).
        void FindEmptySlot(AbilityObject ability, UnitInfo unit)
        {
            int oldSlot = ability.buttonPosition;
            for (int i = oldSlot + 1; i < BUTTONS_COUNT; i++)
            {
                if (!isFill[i])
                {
                    RevealSlot(i, ability, unit);
                    return;
                }
            }

            for (int i = oldSlot - 1; i > 0; i--)
            {
                if (!isFill[i])
                {
                    RevealSlot(i, ability, unit);
                    return;
                }
            }
        }

        //Shows the slot and updates the information in it.
        void RevealSlot(int buttonSlot, AbilityObject ability, UnitInfo unit)
        {
            if (ability == null)
            {
                Debug.LogWarning("ability in " + GetType().Name + "/RevealSlot is null. The method has been stopped.");
                return;
            }
            if (unit == null)
            {
                Debug.LogWarning("unit in " + GetType().Name + "/RevealSlot is null. The method has been stopped.");
                return;
            }
            buttons[buttonSlot].gameObject.SetActive(true);
            buttons[buttonSlot].SetData(ability, unit, buttonKeys[buttonSlot]);
            buttons[buttonSlot].SetButtonActivness(isUpgradeMode);
            ability.SetActiveCooldown(buttons[buttonSlot].transform, !isUpgradeMode);
            isFill[buttonSlot] = true;
            filled++;
        }

        //Enables abilities to improve and disables all others
        public void RevealButtonsForUpdate()
        {
            isUpgradeMode = true;
            upgradePlus.text = "-";
            foreach (var button in buttons)
            {
                button?.RevealButtonForUpgrade();
            }
        }

        //Turns off the ability for abilities to improve and turns on all others
        public void HideButtonsForUpdate()
        {
            isUpgradeMode = false;
            upgradePlus.text = "+";
            foreach (var button in buttons)
            {
                button?.EnableButton();
            }
        }
    }
}
