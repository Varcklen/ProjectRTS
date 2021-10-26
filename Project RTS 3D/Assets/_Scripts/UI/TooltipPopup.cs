using UnityEngine;
using UnityEngine.UI;
using TMPro;

//The script that is responsible for the tooltip UI element.
public class TooltipPopup : MonoBehaviour
{
    public static TooltipPopup Instance { get; private set; }

    private TextMeshProUGUI text;

    private GameObject popupCanvasObject;

    private RectTransform popupObject;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
        popupCanvasObject = transform.GetChild(0).gameObject;
        popupObject = popupCanvasObject.transform.GetChild(0).GetComponent<RectTransform>();
        text = popupObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    //Shows a tooltip with a description obtained from the interface.
    public void DisplayTooltip(IReturnDescriptionTooltip tooltipInterface, DescriptionInfo descriptionInfo)
    {
        text.text = tooltipInterface.GetTooltipInfoText(descriptionInfo);

        popupCanvasObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(popupObject);
    }

    //Hides Tooltip
    public void HideTooltip()
    {
        popupCanvasObject.SetActive(false);
    }

}

//GetTooltipInfoText - Returns the text for tooltip
public interface IReturnDescriptionTooltip
{
    string GetTooltipInfoText(DescriptionInfo descriptionInfo);
}

//Sometimes it is required to transfer some data for the description and sometimes it is not.
//This structure stores data about all possible requests and transfers the ones that you request.
public struct DescriptionInfo
{
    public KeyCode key;
    public bool withoutName;
}
