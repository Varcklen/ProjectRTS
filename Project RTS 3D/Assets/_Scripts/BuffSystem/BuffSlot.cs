using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.BuffSystem
{
    public class BuffSlot : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
    {

        private TooltipPopup tooltipPopup;

        private BuffObject buff;

        private bool isTooltipActive;

        private DescriptionInfo descriptionInfo = new DescriptionInfo();

        private void Awake()
        {
            tooltipPopup = GameObject.Find("Tooltip Popup").GetComponent<TooltipPopup>();
        }

        public void SetBuff(BuffObject buff)
        {
            this.buff = buff;
        }

        private void OnDisable()
        {
            if (isTooltipActive) tooltipPopup.HideTooltip();
        }

        #region Tooltip
        //Fires when the cursor enters from the button.
        public void OnPointerEnter(PointerEventData eventData)
        {
            isTooltipActive = true;
            tooltipPopup.DisplayTooltip(buff, descriptionInfo);
        }

        //Fires when the cursor leaves the button.
        public void OnPointerExit(PointerEventData eventData)
        {
            isTooltipActive = false;
            tooltipPopup.HideTooltip();
        }
        #endregion
    }
}
