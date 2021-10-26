using System;
using System.Collections.Generic;
using UnityEngine;
using Project.AbilitySystem;
using Photon.Pun;

/// <summary>
/// All basic information about unit or item.
/// </summary>
[Serializable]
public class ObjectInfo : MonoBehaviourPunCallbacks
{
    [Tooltip("If true, no longer updates data from the Scriptable Object. Allows you to uniquely modify an object.")] 
    [SerializeField] protected bool unique;
    
    #region Delegates
    public Action selectionDelegate;
    #endregion

    public bool isSelected { get; private set; } = false;

    protected GameObject selectionIndicator { get; private set; }
    public Camera iconCam { get; private set; }

    protected SpriteRenderer minimapIcon;

    protected ObjectType objectType;

    //in online, change to camera owner
    protected Camera mainCamera { get; private set; }

    protected void AwakeInfo()
    {
        minimapIcon = transform.Find("MinimapImage")?.GetComponent<SpriteRenderer>();
        iconCam = transform.Find("IconCamera")?.GetComponent<Camera>();
        selectionIndicator = transform.Find("SelectCircle")?.gameObject;
        mainCamera = Camera.main;
    }

    protected void StartInfo()
    {
        UnitSelections.Instance.unitList.Add(this);
    }

    private void OnDestroy()
    {
        if (UnitSelections.Instance.unitList != null)
        {
            UnitSelections.Instance.Deselect(this);
            UnitSelections.Instance.unitList.Remove(this);
        }
    }

    #region SetRegion
    public void SetIsSelected(bool isSelected)
    {
        this.isSelected = isSelected;
        selectionIndicator.SetActive(isSelected);
        if (selectionDelegate != null && isSelected)
        {
            ButtonManager.Instance?.HideButtonsForUpdate();
            selectionDelegate();
        }
    }
    #endregion

    #region GetRegion
    public bool GetIsSelected()
    {
        return isSelected;
    }
    public ObjectType GetObjectType()
    {
        return objectType;
    }
    #endregion

    public bool IsUnitHasFlag(ObjectType type)
    {
        return objectType.HasFlag(type);
    }

    public bool IsUnitHasAnyFlag(params ObjectType[] types)
    {
        foreach (var type in types)
        {
            if (objectType.HasFlag(type))
            {
                return true;
            }
        }
        return false;
    }
}
