using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Project.Inventory
{

    public class ItemSlot : MonoBehaviour,
        IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,
        IDropHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private ItemObject item;

        private KeyCode key = KeyCode.None;

        private UnitInfo unitInfo;

        private TextMeshProUGUI keyCodeText;
        private TextMeshProUGUI itemChargeText;

        private Transform itemChargeBackground;

        private Image image;

        private TooltipPopup tooltipPopup;

        private RectTransform rectTransform;

        private Canvas canvas;

        private CanvasGroup canvasGroup;

        private Vector2 startAnchortedPosition;

        private UI_Inventory inventoryUI;

        private bool isDropped;
        public bool IsCanDrag { get; private set; } = true;
        private bool isCanClick = true;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            inventoryUI = GameObject.Find("UI_Inventory").GetComponent<UI_Inventory>();
            canvas = GameObject.Find("CanvasCore").GetComponent<Canvas>();
            keyCodeText = transform.Find("KeyCodeText").GetComponent<TextMeshProUGUI>();
            itemChargeBackground = transform.Find("ChargeBackdrop");
            tooltipPopup = GameObject.Find("Tooltip Popup").GetComponent<TooltipPopup>();
            itemChargeText = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
            image = GetComponent<Image>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(key))
            {
                ButtonClick();
            }
        }

        //Sets all information for a slot
        public void SetSlot(ItemObject item, UnitInfo unitInfo, KeyCode key)
        {
            this.key = key;
            this.item = item;
            this.unitInfo = unitInfo;
            image.sprite = item.icon;
            item?.usedAbility?.SetActiveCooldown(transform);
            if (item.itemType == ItemType.Consumable)
            {
                itemChargeBackground.gameObject.SetActive(true);
                itemChargeText.text = item.charges.ToString();
            }
            else
            {
                itemChargeBackground.gameObject.SetActive(false);
            }
            if (key != KeyCode.None)
            { 
                keyCodeText.text = ColorText.KeyCodeToString(key); 
            }
        }

        public void SetSlotInteract(bool interact)
        {
            isCanClick = interact;
            IsCanDrag = interact;
        }

        //Fires when a button is pressed
        public void OnPointerClick(PointerEventData eventData)
        {
            ButtonClick();
        }

        //Fires a button trigger
        void ButtonClick()
        {
            if (!isCanClick || !unitInfo.IsPlayerOwnerToUnit())
            {
                return;
            }
            tooltipPopup.HideTooltip();
            unitInfo.abilityManager.ButtonClick(item.usedAbility);
            if (item.itemType == ItemType.Consumable)
            {
                item.charges--;
                if (item.charges <= 0)
                {
                    unitInfo.inventoryManager.RemoveItem(item);
                }
                inventoryUI.RefreshInventoryItems();
            }
        }

        #region DragSystem
        //Returns the previous ability parameters at the end of Drag.
        public void SetBaseVision()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        //Occurs when moved to an empty inventory slot.
        public void SetNewItemPosition(int newPos)
        {
            isDropped = true;
            item.currentSlot = newPos;
            inventoryUI.SetItemsSlot(newPos, item);
        }

        //It is activated when the icon with the item starts moving.
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsCanDrag || !unitInfo.IsPlayerOwnerToUnit())
            {
                return;
            }
            canvasGroup.alpha = 0.7f;
            canvasGroup.blocksRaycasts = false;
            startAnchortedPosition = rectTransform.anchoredPosition;
            isDropped = false;
        }

        //It is activated when you release the icon with an item while moving.
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsCanDrag || !unitInfo.IsPlayerOwnerToUnit())
            {
                return;
            }
            SetBaseVision();
            if (!isDropped)
            {
                rectTransform.anchoredPosition = startAnchortedPosition;
            }
            isDropped = false;
        }

        //Each frame is activated in the process of moving an icon with an item.
        public void OnDrag(PointerEventData eventData)
        {
            if (!IsCanDrag || !unitInfo.IsPlayerOwnerToUnit())
            {
                return;
            }
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        //It is activated when releasing an icon with an item and touching another item icon.
        public void OnDrop(PointerEventData eventData)
        {
            if (!IsCanDrag || !unitInfo.IsPlayerOwnerToUnit())
            {
                return;
            }
            if (eventData.pointerDrag != null)
            {
                ItemSlot inventorySlot = eventData.pointerDrag.GetComponent<ItemSlot>();//InventorySlot
                if (inventorySlot != null)
                {
                    isDropped = true;
                    inventoryUI.SwapAbility(item.currentSlot, inventorySlot.item.currentSlot);// inventorySlot.GetInventorySlot()
                }
            }
        }
        #endregion

        #region Tooltip
        //Fires when the cursor enters from the button.
        public void OnPointerEnter(PointerEventData eventData)
        {
            tooltipPopup.DisplayTooltip(item, new DescriptionInfo { key = key } );
        }

        //Fires when the cursor leaves the button.
        public void OnPointerExit(PointerEventData eventData)
        {
            tooltipPopup.HideTooltip();
        }
        #endregion
    }

}
