using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.Inventory
{
    public class InventorySlot : MonoBehaviour,
    IDropHandler
    {
        private int inventorySlot;

        private RectTransform rectTransform;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        #region SetRegion
        public void SetInventorySlot(int inventorySlot)
        {
            this.inventorySlot = inventorySlot;
        }
        #endregion

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
                ItemSlot itemSlot = eventData.pointerDrag.GetComponent<ItemSlot>();
                if (itemSlot == null || (!itemSlot?.IsCanDrag ?? true))
                {
                    return;
                }
                //if (eventData.pointerDrag.TryGetComponent(out ItemSlot itemslot))
                //{
                //    itemslot.SetNewItemPosition(inventorySlot);
                //}
                itemSlot.SetNewItemPosition(inventorySlot);
                eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = rectTransform.anchoredPosition;
            }
        }
    }

}
