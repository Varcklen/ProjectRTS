using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquadPanel : MonoBehaviour
{
    public UnitInfo selectedUnit { get; set; }

    private List<RectTransform> squadButtons;

    private void Awake()
    {
        squadButtons = GlobalMethods.GetChildrensRectTransform(transform);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToNextUnit();
        }
    }

    void ToNextUnit()
    {
        List<ObjectInfo> unitSelections = UnitSelections.Instance.unitSelected;
        if (unitSelections.Count <= 1)
        {
            return;
        }
        if (unitSelections.Contains(selectedUnit))
        {
            int index = unitSelections.IndexOf(selectedUnit);
            if (index == unitSelections.Count-1)
            {
                selectedUnit = (UnitInfo)unitSelections[0];
            }
            else
            {
                selectedUnit = (UnitInfo)unitSelections[index+1];
            }
            UI_Manager.Instance.UIDisplaySquad(selectedUnit);
        }
    }

    public void SetSquadDisplay(List<ObjectInfo> units, UnitInfo choosedUnit)
    {
        if (units.Count == 0)
        {
            Debug.Log("SquadPanel/SetSquadDisplay. \"units\" variable has no units.");
            return;
        }
        selectedUnit = choosedUnit;//(UnitInfo)units[0];
        int i = 0;
        foreach (var button in squadButtons)
        {
            if (i < units.Count)
            {
                button.gameObject.SetActive(true);
                if (units[i] is UnitInfo unitInfo)
                {
                    if (unitInfo == selectedUnit)
                    {
                        button.transform.GetChild(0).gameObject.SetActive(true);
                    }
                    else
                    {
                        button.transform.GetChild(0).gameObject.SetActive(false);
                    }
                    button.transform.GetChild(1).GetComponent<Image>().sprite = unitInfo.Stats.icon;
                    button.GetComponent<SquadButton>().Unit = unitInfo;
                }
            }
            else
            {
                button.gameObject.SetActive(false);
            }
            i++;
        }
    }
}
