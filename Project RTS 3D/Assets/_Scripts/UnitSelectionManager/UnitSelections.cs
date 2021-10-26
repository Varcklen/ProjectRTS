using System.Collections.Generic;
using UnityEngine;

//Collection of methods for managing units.
public class UnitSelections : MonoBehaviour
{
    public List<ObjectInfo> unitList { get; private set; } = new List<ObjectInfo>();
    public List<ObjectInfo> unitSelected { get; private set; } = new List<ObjectInfo>();

    public static UnitSelections Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    //Selects one unit
    public void ClickSelect(ObjectInfo unitToAdd)
    {
        DeselectAll();
        unitSelected.Add(unitToAdd);
        unitToAdd.SetIsSelected(true);
    }

    //Adds/removes a unit from the selected list using Shift.
    public void ShiftClickSelect(ObjectInfo unitToAdd)
    {
        if (unitSelected.Contains(unitToAdd))
        {
            unitSelected.Remove(unitToAdd);
            unitToAdd.SetIsSelected(false);
            UI_Manager.Instance.UIDisplaySquad(null, (UnitInfo)unitToAdd);
        }
        else
        {
            unitSelected.Add(unitToAdd);
            unitToAdd.SetIsSelected(true);
        }
    }

    //Adds all units from 2D box
    public void DragSelect(ObjectInfo unitToAdd)
    {
        if (!unitSelected.Contains(unitToAdd))
        {
            unitSelected.Add(unitToAdd);
            unitToAdd.SetIsSelected(true);
        }
    }

    //Clears the list of selected units.
    public void DeselectAll()
    {
        foreach (ObjectInfo unit in unitSelected)
        {
            unit.SetIsSelected(false);
        }
        unitSelected.Clear();
        UI_Manager.Instance.ClearInfoPanel();
    }

    //Removes a unit from the selected.
    public void Deselect(ObjectInfo unitToDeselect)
    {
        if (unitSelected.Contains(unitToDeselect))
        {
            unitSelected.Remove(unitToDeselect);
            unitToDeselect.SetIsSelected(false);
        }       
    }
}
