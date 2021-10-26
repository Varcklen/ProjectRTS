using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Properties for nodes
[RequireComponent(typeof(UnitInfo))]
public class Node : MonoBehaviour
{

    //public ResourceTypes resourceType;

    [SerializeField] private int avaibleRecource;
    [SerializeField] private int initialRecource;
    [SerializeField] private float distToHarvest = 5f;

    private List<GameObject> gathererList = new List<GameObject>();

    private void Awake()
    {
        avaibleRecource = initialRecource;
    }

    #region GetRegion
    public int GetAvaibleRecource()
    {
        return avaibleRecource;
    }
    public int GetInitialRecource()
    {
        return initialRecource;
    }
    public float GetDistToHarvest()
    {
        return distToHarvest;
    }
    #endregion

    //Gives a worker a resource and takes a resource away from a node
    public int ResourceGather(int resource)
    {
        int resourceAdded = 0;
        if (gathererList.Count > 0)
        {
            resourceAdded = resource;
            if (avaibleRecource < resourceAdded)
            {
                resourceAdded = avaibleRecource;
            }
            avaibleRecource -= resourceAdded;
            if (avaibleRecource <= 0)
            {
                DestroyObject();
            }
        }
        return resourceAdded;
    }

    //Adds a worker to a node
    public void GathererAdd(GameObject gatherer)
    {
        if (!gathererList.Contains(gatherer))
        {
            gathererList.Add(gatherer);
        }
    }

    //Removes a worker to a node
    public void GathererRemove(GameObject gatherer)
    {
        if (gathererList.Contains(gatherer))
        {
            if (gatherer.TryGetComponent(out Harvest harvest))
                harvest.NodeToZero();
            gathererList.Remove(gatherer);
        }
    }

    //Removes all workers to a node
    public void GathererClear()
    {
        foreach (GameObject gatherer in gathererList)
        {
            if (gatherer.TryGetComponent(out Harvest harvest))
                harvest.NodeToZero();
        }
        gathererList.Clear();
    }

    /* Deletes a node. It is worth improving. For example, replace with OnDestroy(). 
     * The previous attempt to do so was unsuccessful.
     */
    private void DestroyObject()
    {
        GathererClear();
        Destroy(gameObject);
    }
}
