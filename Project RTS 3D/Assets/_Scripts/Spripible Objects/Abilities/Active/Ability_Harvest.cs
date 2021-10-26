using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Project.AbilitySystem;

//Forces the unit to extract the resource and, after extraction, carry the resource to the collector.
[CreateAssetMenu(fileName = "New Ability_Harvest", menuName = "Custom/Ability/Active/Harvest")]
public class Ability_Harvest : ActiveAbility<Harvest, Ability_Harvest_Stats>
{
    public override void Init(ObjectInfo caster, AbilityObject ability)
    {
        InitComponent(caster, ability);
    }

    public override void Use(UseParams p)
    {
        UseComponent(p);
    }
}

[Serializable]
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ObjectType), true)]
#endif
public class Ability_Harvest_Stats : IStatsSO<Ability_Harvest_Stats>
{
    [Header("Deliver")]
    public string deliverName;
    public Sprite deliverSprite;
    [TextArea] public string deliverDecription;
    public ObjectType collectorType;

    [Header("Stats")]
    public int resourcePerTick = 1;
    public int maxHeldResource;
    public float nodeFindArea;
    public float harvestTime = 1;

    public void SetStatsFromSO(Ability_Harvest_Stats so)
    {
        deliverName = so.deliverName;
        deliverSprite = so.deliverSprite;
        deliverDecription = so.deliverDecription;
        resourcePerTick = so.resourcePerTick;
        maxHeldResource = so.maxHeldResource;
        nodeFindArea = so.nodeFindArea;
        harvestTime = so.harvestTime;
        collectorType = so.collectorType;
    }
}

public class Harvest : MonoAbility<Ability_Harvest_Stats>, IActiveAbilityInit
{
    private GameObject caster;
    private GameObject node;
    private GameObject temple;

    private AbilityObject ability;

    //Change for multiplayer
    private Camera mainCam;

    private UnitInfo unitInfo;

    private NavMeshAgent agent;

    private PlayerInfo playerInfo;

    private Node nodeManager;

    private string oldName;
    private Sprite oldSprite;
    private string oldDecription;

    private Action<UseParams> mainOrder;

    private Vector3 nodePos;

    private int heldResource;

    public void Init(UnitInfo caster, AbilityObject ability)
    {
        unitInfo = caster;
        this.caster = unitInfo.gameObject;
        this.ability = ability;
        ability.distance = Mathf.Infinity;
        mainCam = Camera.main;
        unitInfo = caster.GetComponent<UnitInfo>();
        agent = caster.GetComponent<NavMeshAgent>();
        mainOrder = OrderHarvest;
        playerInfo = GameObject.Find(unitInfo.PlayerOwning.ToString()).GetComponent<PlayerInfo>();

        oldName = ability.name;
        oldSprite = ability.icon;
        oldDecription = ability.description;

        InputManager.OnRightClick += RightClick;
        ActionList.OnStopUnit += Stop;
    }

    private void OnDestroy()
    {
        InputManager.OnRightClick -= RightClick;
        ActionList.OnStopUnit -= Stop;
    }

    //Activates the ability when the player presses the right mouse button
    private void RightClick(RaycastHit hit)
    {
        GameObject gameObject = hit.collider.gameObject;
        if (!gameObject.TryGetComponent(out UnitInfo objectInfo))
        {
            return;
        }
        if (this.unitInfo.GetIsSelected())
        {
            Use(new UseParams { target = gameObject });
        }   
    }

    public void Use(UseParams param)
    {
        mainOrder(param);
    }

    #region SetRegion
    public void NodeToZero()
    {
        node = null;
        nodeManager = null;
    }
    #endregion

    //Activated when the unit stops.
    private void Stop(UnitInfo objectInfo)
    {
        if (this.unitInfo != objectInfo)
            return;
        if (node != null)
            nodeManager.GathererRemove(gameObject);
        node = null;
        temple = null;
    }

    //Finding another node of the same type
    void FindAnotherNode()
    {
        Collider[] colliders = Physics.OverlapSphere(nodePos, abilityStats.nodeFindArea, LayerMask.GetMask("Selectable"));
        foreach (Collider collider in colliders)
        {
            GameObject unit = collider.gameObject;
            if (unit.GetComponent<UnitInfo>().GetObjectType() == ObjectType.Node)
            {
                //unit.transform.position !=  param.nodePos - can be improved. A bug may occur if 2 nodes have the same position.
                if (unit.transform.position != nodePos)//unit.GetComponent<Node>().resourceType == heldResourceType && 
                {
                    SetNode(unit);
                    break;
                }
            }
        }
        if (node != null)
        {
            Gather();
        }
        else if (heldResource > 0)
        {
            ToDeliver();
        }
        else
        {
            unitInfo.stopDelegate();
        }
    }

    #region Harvest
    //Activates a chain of methods related to resource extraction.
    void OrderHarvest(UseParams param)
    {
        if (param.target == null)
        {
            if (nodePos != Vector3.zero)
            {
                FindAnotherNode();
            }
            return;
        }
        if (!param.target.TryGetComponent(out Node nodeManager))
            return;
        SetNode(param.target);
        Gather();
    }

    //Sets the object from which mining will be carried out.
    void SetNode(GameObject newNode)
    {
        node = newNode;
        if (node != null)
        {
            nodePos = node.transform.position;
            nodeManager = node.GetComponent<Node>();
            //heldResourceType = nodeManager.resourceType;
        }
    }

    //Commands the unit to go to the node
    void Gather()
    {
        if (node == null || nodeManager == null)
        {
            FindAnotherNode();
            return;
        }
        nodeManager.GathererAdd(caster);
        unitInfo.SetTask(TaskList.Gather);
        agent.destination = node.transform.position;
        unitInfo.StartCoroutine(GatherTick());
    }

    //If the unit is near the node, it starts harvesting.
    IEnumerator GatherTick()
    {
        while (unitInfo.GetTask() == TaskList.Gather && node != null)
        {
            yield return null;
            if (unitInfo.GetTask() == TaskList.Gather && node != null)
            {
                float distance = (node.transform.position - caster.transform.position).sqrMagnitude;
                if (distance <= Mathf.Pow(nodeManager.GetDistToHarvest(), 2))
                {
                    HarvestStart();
                }
            }
        }
    }

    //Start of harvesting
    void HarvestStart()
    {
        unitInfo.SetTask(TaskList.Harvest);
        //If a unit has switched to a node of a different type, we "discard" the current resource.
        //if (heldResourceType != nodeManager.resourceType)
        //{
        //    heldResource = 0;
        //}
        //heldResourceType = nodeManager.resourceType;
        agent.destination = caster.transform.position;
        unitInfo.StartCoroutine(HarvestTick());
    }

    //Keeps track of how much of the resource is being mined and when to carry the resource to the collector.
    IEnumerator HarvestTick()
    {
        abilityStats.harvestTime = Mathf.Clamp(abilityStats.harvestTime, 0.02f, Mathf.Infinity);
        while (unitInfo.GetTask() == TaskList.Harvest && node != null)
        {
            yield return new WaitForSeconds(abilityStats.harvestTime);
            if (unitInfo.GetTask() == TaskList.Harvest && node != null)
            {
                if (node.GetComponent<Node>().GetAvaibleRecource() > 0)
                {
                    heldResource += nodeManager.ResourceGather(abilityStats.resourcePerTick);
                    if (heldResource >= abilityStats.maxHeldResource)
                    {
                        ToDeliver();
                        yield break;
                    }
                }
            }
        }
        if (unitInfo.GetTask() == TaskList.Harvest)
        {
            if (node == null)
            {
                FindAnotherNode();
            }
            else if (heldResource > 0)
            {
                ToDeliver();
            }
            else
            {
                unitInfo.stopDelegate();
            }
        }
    }

    //Toggles ability to Deliver
    void ToDeliver()
    {
        ability.name = abilityStats.deliverName;
        ability.icon = abilityStats.deliverSprite;
        ability.description = abilityStats.deliverDecription;
        ability.abilityTarget = AbilityTarget.NoTarget;

        ButtonManager.Instance.UpdateButtons(unitInfo);
        mainOrder = OrderDeliver;

        mainOrder(new UseParams());
    }
    #endregion

    #region Deliver
    //Commands the unit to assign the resource
    void OrderDeliver(UseParams param)
    {
        if (param.target != null)
        {
            if (param.target.TryGetComponent(out Node n))
            {
                temple = null;
                
            }
            else if (param.target.TryGetComponent(out CollectorMono t))
            {
                temple = param.target;
            } 
        } 
        else
        {
            temple = null;
        }
        DeliverStart();
    }

    //Begins a resource transfer and searches for a collector.
    void DeliverStart()
    {
        if (temple == null)
        {
            List<GameObject> drops = GlobalMethods.GetChildrens(GameObject.Find(unitInfo.PlayerOwning.ToString() + " Units"));
            GameObject closestDrop = null;
            float closestDistance = Mathf.Infinity;

            //Looks for the closest building you control with the Collector type as closestDrop.
            foreach (GameObject drop in drops)
            {
                if (!drop.TryGetComponent(out UnitInfo unitInfo) )
                    continue;
                if ((unitInfo.GetObjectType() & abilityStats.collectorType) == 0 || !unitInfo.IsPlayerOwnerToUnit())
                    continue;
                float distance = (drop.transform.position - caster.transform.position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDrop = drop;
                }
            }
            if (closestDrop != null)
            {
                temple = closestDrop;
            }
            else
            {
                //Debug.LogWarning(caster + " cannot deliver. No collectors.");
                unitInfo.stopDelegate();
                return;
            }
        }
        if (temple == null)
        {
            Debug.LogWarning(caster + " cannot deliver. target is null.");
            unitInfo.stopDelegate();
            return;
        }
        unitInfo.SetTask(TaskList.Deliver);
        agent.destination = temple.transform.position;
        unitInfo.StartCoroutine(DeliverTick());
    }

    //Keeps track of when a resource is due to be delivered
    IEnumerator DeliverTick()
    {
        while (unitInfo.GetTask() == TaskList.Deliver && temple != null)
        {
            yield return null;
            if (unitInfo.GetTask() == TaskList.Deliver && temple != null)
            {
                float distance = (temple.transform.position - caster.transform.position).sqrMagnitude;
                if (distance <= temple.GetComponent<CollectorMono>().abilityStats.distanceToDeliver)
                {
                    playerInfo.Gold += heldResource;
                    heldResource = 0;
                    ToHarvest();
                    yield break;
                }
            }
        }
    }

    //Toggles ability to Harvest
    void ToHarvest()
    {
        temple = null;
        ability.name = oldName;
        ability.icon = oldSprite;
        ability.description = oldDecription;
        ability.abilityTarget = AbilityTarget.Target;

        ButtonManager.Instance.UpdateButtons(unitInfo);
        mainOrder = OrderHarvest;

        mainOrder(new UseParams { target = node });
    }
    #endregion
}
