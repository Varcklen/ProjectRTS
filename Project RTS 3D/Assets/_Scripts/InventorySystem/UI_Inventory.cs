using System.Collections.Generic;
using UnityEngine;

namespace Project.Inventory
{
    public class UI_Inventory : MonoBehaviour
    {
        [SerializeField] private KeyCode[] slotKeys = new KeyCode[6];

        private InventoryManager inventoryManager;

        private Transform itemSlotContainer;
        private Transform inventorySlotsContainer;

        [HideInInspector] private ItemObject[] items;

        private ItemSlot[] itemSlots;

        private List<CanvasGroup> itemSlotCanvasGroup;

        private List<InventorySlot> inventorySlots;

        private UnitInfo unitInfo;

        private int inventoryLimit;
        private readonly int globalInventoryLimit = 6;

        private void Awake()
        {
            itemSlots = new ItemSlot[globalInventoryLimit];
            items = new ItemObject[globalInventoryLimit];
            inventorySlots = new List<InventorySlot>();
            itemSlotCanvasGroup = new List<CanvasGroup>();
            itemSlotContainer = transform.Find("ItemCollector");
            inventorySlotsContainer = transform.Find("InventorySlots");
            List<GameObject> itemSlotObjects = GlobalMethods.GetChildrens(itemSlotContainer.gameObject);
            for (int i = 0; i < itemSlotObjects.Count; i++)
            {
                itemSlots[i] = itemSlotObjects[i].GetComponent<ItemSlot>();
                itemSlotCanvasGroup.Add(itemSlotObjects[i].GetComponent<CanvasGroup>());
            }
            List<GameObject> inventorySlotObjects = GlobalMethods.GetChildrens(inventorySlotsContainer.gameObject);
            for (int i = 0; i < inventorySlotObjects.Count; i++)
            {
                inventorySlots.Add(inventorySlotObjects[i].GetComponent<InventorySlot>());
                inventorySlots[i].SetInventorySlot(i);
            }
        }

        #region SetRegion
        //Sets the EMPTY inventory slot to a new one.
        public void SetItemsSlot(int slot, ItemObject item)
        {
            if (items[slot] != null && items[slot] != item)
            {
                Debug.LogWarning("Slot " + slot + " is not empty!");
                return;
            }
            if (!GlobalMethods.NumberInRange(slot, 0, 5))
            {
                return;
            }
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == item)
                {
                    items[i] = null;
                    break;
                }
            }
            items[slot] = item;
            RefreshInventoryItems();
        }

        //Makes all item buttons non-clickable, or vice versa.
        public void SetItemSlotsInteractable(bool isInteractable)
        {
            foreach (CanvasGroup canvasGroup in itemSlotCanvasGroup)
                canvasGroup.interactable = isInteractable;
            for (int i = 0; i < itemSlots.Length; i++)
                itemSlots[i].SetSlotInteract(isInteractable);
        }

        //Sets the inventory for the player
        public void SetInventory(UnitInfo unitInfo)
        {
            ResetOldVariables();
            this.unitInfo = unitInfo;
            inventoryManager = unitInfo?.inventoryManager;
            if (inventoryManager == null)
            {
                itemSlotContainer.gameObject.SetActive(false);
                inventorySlotsContainer.gameObject.SetActive(false);
                return;
            }
            else
            {
                itemSlotContainer.gameObject.SetActive(true);
                inventorySlotsContainer.gameObject.SetActive(true);
            }
            if (inventoryManager != null) inventoryManager.OnItemListChanged += InventoryManager_OnItemListChanged;
            inventoryLimit = inventoryManager.GetInventoryLimit();
            items = inventoryManager.GetItemList();
            for (int i = 0; i < globalInventoryLimit; i++)
            {
                if (inventoryLimit <= i)
                    inventorySlots[i].gameObject.SetActive(false);
                else
                    inventorySlots[i].gameObject.SetActive(true);
            }
            RefreshInventoryItems();
        }

        //Zeroes the variables of the previous owner
        private void ResetOldVariables()
        {
            if (inventoryManager != null) inventoryManager.OnItemListChanged -= InventoryManager_OnItemListChanged;
            for (int i = 0; i < items.Length; i++)
                items[i]?.SetAbilitiesVision(false);
        }
        #endregion

        //Event
        private void InventoryManager_OnItemListChanged(object sender, System.EventArgs e)
        {
            RefreshInventoryItems();
        }

        //Refreshes data about items in inventory
        public void RefreshInventoryItems()
        {
            SlotOptimize();
            for (int i = 0; i < itemSlots.Length; i++)
            {
                HideSlot(i);
                if (items.Length <= i || items[i] == null) continue;
                ShowSlot(items[i]);
            }
            if (unitInfo.IsPlayerOwnerToUnit() && !unitInfo.IsStunned)
            {
                SetItemSlotsInteractable(true);
            }
            else
            {
                SetItemSlotsInteractable(false);
            }
        }

        //Shows the slot and updates the information
        private void ShowSlot(ItemObject item)
        {
            int slot = item.currentSlot;
            item.SetAbilitiesVision(true);
            itemSlots[slot].gameObject.SetActive(true);
            itemSlots[slot].GetComponent<ItemSlot>().SetSlot(item, unitInfo, slotKeys[slot]);
            itemSlots[slot].GetComponent<RectTransform>().anchoredPosition = inventorySlots[slot].GetComponent<RectTransform>().anchoredPosition;
            itemSlots[slot].SetBaseVision();
        }
        
        //Hides the slot
        void HideSlot(int i)
        {
            itemSlots[i].gameObject.SetActive(false);
        }

        //If there is an excessive amount of items, it throws them out of the inventory. If there are several items for the same slot, it optimizes the slots
        void SlotOptimize()
        {
            bool[] isFill = new bool[inventoryLimit];
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) continue;
                for (int j = 0; j < inventoryLimit; j++)
                {
                    if (items[i].currentSlot == j)
                    {
                        if (!GlobalMethods.NumberInRange(j, 0, inventoryLimit))
                        {
                            inventoryManager.DropItemUnderUnit(items[i]);
                        }
                        if (isFill[j])
                        {
                            FindEmptySlot(i, ref isFill);
                        }
                        else
                        {
                            isFill[j] = true;
                        }
                        break;
                    }
                }
            }
        }

        //Looks for an empty slot in the inventory
        void FindEmptySlot(int oldSlot, ref bool[] isFill)
        {
            bool isFind = false;
            for (int i = 0; i < inventoryLimit; i++)
            {
                if (!isFill[i])
                {
                    isFind = true;
                    isFill[i] = true;
                    items[oldSlot].currentSlot = i;
                    break;
                }
            }
            if (!isFind) inventoryManager.DropItemUnderUnit(items[oldSlot]);
        }

        public void SwapAbility(int abilityIndexA, int abilityIndexB)
        {
            if (abilityIndexA == abilityIndexB || !GlobalMethods.NumberInRange(abilityIndexA, 0, 5) || !GlobalMethods.NumberInRange(abilityIndexB, 0, 5))
            {
                Debug.LogWarning("Swap between slots is not possible.");
                return;
            }
            int pos = items[abilityIndexA].currentSlot;
            items[abilityIndexA].currentSlot = items[abilityIndexB].currentSlot;
            items[abilityIndexB].currentSlot = pos;
            ItemObject obj = items[abilityIndexA];
            items[abilityIndexA] = items[abilityIndexB];
            items[abilityIndexB] = obj;
            RefreshInventoryItems();
        }
    }
}
