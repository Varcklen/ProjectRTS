using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour,
    IReturnDescriptionTooltip, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private string tooltipName;
    [SerializeField, TextArea(5,30)] private string description;

    private TooltipPopup tooltipPopup;

    private DescriptionInfo descriptionInfo = new DescriptionInfo();

    private void Start()
    {
        tooltipPopup = GameObject.Find("Tooltip Popup").GetComponent<TooltipPopup>();
    }

    public string GetTooltipInfoText(DescriptionInfo descriptionInfo)
    {
        StringBuilder builder = new StringBuilder();

        builder.Append("<size=35>").Append(tooltipName).Append("</size>").AppendLine();
        builder.Append(description).AppendLine();

        return builder.ToString();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipPopup.DisplayTooltip(this, descriptionInfo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipPopup.HideTooltip();
    }
}
