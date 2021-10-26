using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Allows to select multiple units using a 2D box.
public class UnitDrag : MonoBehaviour
{

    [SerializeField] private RectTransform boxVisual;

    [SerializeField] LayerMask clickable;

    //For online, will need to change for different players
    private Camera myCam;

    private Rect selectionBox;

    private Vector2 startPos = Vector2.zero;
    private Vector2 endPos = Vector2.zero;

    private bool isCanBeActive = true;

    private const float MINIMAL_SELECTION_BOX = 5f;

    void Start()
    {
        myCam = Camera.main;
        DrawVisual();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && isCanBeActive)
        {
            //If the start of the box is on the UI, do not activate the box.
            if (GlobalMethods.IsPointerOverUIObject())
            {
                isCanBeActive = false;
                return;
            }
            startPos = Input.mousePosition;
            selectionBox = new Rect();
        }

        if (Input.GetMouseButton(0) && isCanBeActive)
        {
            endPos = Input.mousePosition;
            DrawVisual();
            DrawSelection();
        }

        if (Input.GetMouseButtonUp(0))
        {
            SelectUnits();
            startPos = Vector2.zero;
            endPos = Vector2.zero;
            DrawVisual();
            isCanBeActive = true;
        }
        ChooseClick();
    }

    //Draws a box with specific dimensions on the screen.
    void DrawVisual()
    {
        boxVisual.position = (startPos + endPos) / 2;
        boxVisual.sizeDelta = new Vector2(Mathf.Abs(startPos.x - endPos.x), Mathf.Abs(startPos.y - endPos.y));
    }

    //Draws a box with specific dimensions on the screen.
    void DrawSelection()
    {
        if(Input.mousePosition.x < startPos.x)
        {
            selectionBox.xMin = Input.mousePosition.x;
            selectionBox.xMax = startPos.x;
        }
        else
        {
            selectionBox.xMin = startPos.x;
            selectionBox.xMax = Input.mousePosition.x;
        }

        if (Input.mousePosition.y < startPos.y)
        {
            selectionBox.yMin = Input.mousePosition.y;
            selectionBox.yMax = startPos.y;
        }
        else
        {
            selectionBox.yMin = startPos.y;
            selectionBox.yMax = Input.mousePosition.y;
        }
    }

    //Unit selection
    void SelectUnits()
    {
        if (selectionBox.xMax-selectionBox.xMin < MINIMAL_SELECTION_BOX || selectionBox.yMax - selectionBox.yMin < MINIMAL_SELECTION_BOX)
        {
            return;
        }
        UnitSelections.Instance.DeselectAll();
        foreach (ObjectInfo unit in UnitSelections.Instance.unitList)
        {
            if (SetectUnitCondition(unit))
            {
                UnitSelections.Instance.DragSelect(unit);
            }
        }
    }

    //Unit selection condition. Can be optimized.
    bool SetectUnitCondition(ObjectInfo unit)
    {
        if (unit == null) { return false; }
        if (!selectionBox.Contains(myCam.WorldToScreenPoint(unit.transform.position), true)) { return false; }
        if (!unit.TryGetComponent(out UnitInfo unitInfo)) { return false; }
        if (!unitInfo.IsUnitHasAnyFlag(ObjectType.Minion, ObjectType.Hero, ObjectType.Boss)) { return false; }
        if (!unitInfo.IsPlayerOwnerToUnit()) { return false; }
        return true;
    }

    //It works when you click on a unit.
    void ChooseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Project.AbilitySystem.AbilityManager.IsFindTarget)
            {
                return;
            }
            RaycastHit hit;
            Ray ray = myCam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickable))
            {
                ObjectInfo hittedObject = hit.collider.gameObject.GetComponent<ObjectInfo>();
                //If: Unit
                if (hittedObject is UnitInfo unitInfo)
                {
                    //There is a problem. When you select an enemy unit and then select an ally via Shift, both of them will be selected.
                    if (Input.GetKey(KeyCode.LeftShift) && unitInfo.IsPlayerOwnerToUnit())
                    {
                        UnitSelections.Instance.ShiftClickSelect(unitInfo);
                    }
                    else if (!GlobalMethods.IsPointerOverUIObject())
                    {
                        UnitSelections.Instance.ClickSelect(unitInfo);
                    }
                }
                //else: Item
                else if (!GlobalMethods.IsPointerOverUIObject())
                {
                    UnitSelections.Instance.ClickSelect(hittedObject);
                }
            }
            else if (!GlobalMethods.IsPointerOverUIObject())
            {
                UnitSelections.Instance.DeselectAll();
            }
        }
    }
}
