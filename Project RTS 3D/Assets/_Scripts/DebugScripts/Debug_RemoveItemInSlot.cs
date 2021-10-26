using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.DebugWorld
{
    public class Debug_RemoveItemInSlot : MonoBehaviour
    {
        [SerializeField] UnitInfo unitInfo;
        [SerializeField, Range(1, 6)] int slot = 1;
        [SerializeField] KeyCode key;

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(key))
            {
                DestroyItem();
            }
        }
#endif

        void DestroyItem()
        {
            Debug.Log("Item removed!");
            if (unitInfo == null) return;
            ItemObject item = unitInfo.inventoryManager.GetItemList()[slot - 1];
            if (item != null)
                unitInfo.inventoryManager.RemoveItemFromInventory(item, unitInfo.transform.position);
        }
    }
}
