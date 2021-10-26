using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Inventory
{
    [Serializable]
    public class InventoryManager
    {
        public event EventHandler OnItemListChanged;

        private int inventoryLimit;
        private int itemsInInventory;

        private float catchRange = 40f;

        private ItemObject[] items;

        private UnitInfo unit;

        private NavMeshAgent agent;

        private ItemInfo target;

        public InventoryManager(UnitInfo unit, InventoryData inventoryData, int inventoryLimit)
        {
            this.unit = unit;
            items = new ItemObject[inventoryLimit];
            this.inventoryLimit = inventoryLimit;
            agent = unit.GetComponent<NavMeshAgent>();

            if (inventoryData.container.Count > 0)
            {
                for (int i = 0; i < inventoryData.container.Count; i++)
                {
                    CreateItemForInventory(inventoryData.container[i]);
                }
            }
        }

        #region GetRegion
        public ItemObject[] GetItemList()
        {
            return items;
        }
        public int GetInventoryLimit()
        {
            return inventoryLimit;
        }
        public int GetNumItemsInInventory()
        {
            return itemsInInventory;
        }
        #endregion

        //Adds a 3D item to inventory.
        void CatchItemObject()
        {
            AddItemToInventory(target.GetItemStats());
            target.DestroySelf();
        }

        //Creates an item in inventory.
        void CreateItemForInventory(ItemData itemData)
        {
            if (itemsInInventory >= inventoryLimit)
            {
                return;
            }
            ItemObject item = new ItemObject();
            item.SetItemFromData(itemData);
            AddItemToInventory(item);
        }

        //Adds an item to inventory.
        void AddItemToInventory(ItemObject item)
        {
            if (item.itemType == ItemType.Consumable)
            {
                //If the item is Consumable, instead of adding an item, it adds charges to the same item
                bool hasCopy = false;
                for (int i = 0; i < inventoryLimit; i++)
                {
                    if (items[i] == null)
                    {
                        continue;
                    }
                    if (item.itemData == items[i].itemData)
                    {
                        items[i].charges += item.charges;
                        hasCopy = true;
                        OnItemListChanged?.Invoke(this, EventArgs.Empty);
                        break;
                    }
                }
                if (!hasCopy)
                {
                    SetSlot(item);
                }
            }
            else
            {
                SetSlot(item);
            }
        }

        void SetSlot(ItemObject item)
        {
            if (itemsInInventory >= inventoryLimit || ItemContains(item))
            {
                return;
            }
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                {
                    items[i] = item;
                    items[i].AddItemAbilities(unit);
                    break;
                }
            }
            itemsInInventory++;
            OnItemListChanged?.Invoke(this, EventArgs.Empty);
        }

        //Removes an item from inventory.
        public void RemoveItemFromInventory(ItemObject item, Vector3 point)
        {
            if (ItemContains(item))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] == item)
                    {
                        items[i].RemoveItemAbilities(unit);
                        items[i] = null;
                        itemsInInventory--;
                        Item.CreateItemObject(point, item.itemData);
                        OnItemListChanged?.Invoke(this, EventArgs.Empty);
                        break;
                    }
                }
            }
        }

        public void RemoveItem(ItemObject item)
        {
            if (ItemContains(item))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] == item)
                    {
                        items[i].RemoveItemAbilities(unit);
                        items[i] = null;
                        itemsInInventory--;
                        OnItemListChanged?.Invoke(this, EventArgs.Empty);
                        break;
                    }
                }
            }
        }

        bool ItemContains(ItemObject item)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == item)
                {
                    return true;
                }
            }
            return false;
        }

        //Right-clicking an item
        public void RightClick(ItemInfo target)
        {
            unit.stopDelegate();
            if (inventoryLimit <= 0)
            {
                Debug.LogWarning("You have no inventory slots.");
                return;
            }
            if (itemsInInventory >= inventoryLimit && target.GetItemStats().itemType != ItemType.Consumable)
            {
                Debug.LogWarning("Inventory is full.");
                return;
            }
            this.target = target;
            unit.TurnToSide(target.transform.position);
            if (catchRange >= GlobalMethods.DistanceBetweenPoints(unit.transform.position, target.transform.position) )
            {
                CatchItemObject();
            }
            else
            {
                MoveToItem();
            }
        }

        public void DropItemUnderUnit(ItemObject item)
        {
            RemoveItemFromInventory(item, unit.transform.position);
        }

        #region MoveToItem
        //Commands the unit to go to the item.
        void MoveToItem()
        {
            unit.StopAllCoroutines();
            unit.SetTask(TaskList.TakeItem);
            unit.StartCoroutine(Move());
        }

        //Movement occurs until the condition is met.
        IEnumerator Move()
        {
            unit.Animator.SetBool("IsMoved", true);
            while (MoveToTargetCondition() && catchRange < GlobalMethods.DistanceBetweenPoints(unit.transform.position, target.transform.position))
            {
                agent.destination = target.transform.position;
                yield return null;
            }
            unit.Animator.SetBool("IsMoved", false);
            if (MoveToTargetCondition() && catchRange >= GlobalMethods.DistanceBetweenPoints(unit.transform.position, target.transform.position))
            {
                unit.stopDelegate();
                CatchItemObject();
            }
        }

        //Condition
        bool MoveToTargetCondition()
        {
            if (target == null)                         return false;
            if (unit.GetTask() != TaskList.TakeItem)    return false;
            if (!unit.IsUnitAlive())                    return false;
            return true;
        }
        #endregion

    }

}
