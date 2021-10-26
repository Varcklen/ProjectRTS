using UnityEngine;
using UnityEngine.EventSystems;

public class SquadButton : MonoBehaviour, IPointerClickHandler
{
    public UnitInfo Unit { get; set; }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Unit == null)
        {
            Debug.LogWarning("Unit for " + gameObject + " button is not set.");
            return;
        }
        UI_Manager.Instance.UIDisplaySquad(Unit);
    }
}
