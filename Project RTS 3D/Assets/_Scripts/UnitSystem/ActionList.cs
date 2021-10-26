using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;


//Basis of commands for units. Imperfect. Can be improved.
public class ActionList : MonoBehaviour
{
    [SerializeField] private GameObject groundMarker;
    public static ActionList Instance { get; private set; }

    private GameObject agentObject;

    private UnitInfo unit;

    public static Action<UnitInfo> OnStopUnit;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    //Move command
    public void Move(NavMeshAgent agent, Vector3 point, bool isCommand = false)
    {
        agentObject = agent.gameObject;
        unit = agentObject.GetComponent<UnitInfo>();
        if (!unit.IsPlayerOwnerToUnit() && isCommand)
        {
            return;
        }
        //if (unitInfo.view.IsMine)
        //{
        unit.SetTask(TaskList.Move);
        unit.Animator.SetBool("IsAttack", false);
        unit.Animator.SetBool("IsMoved", true);
        unit.SetMovePoint(point);
        agent.destination = point;

        StartCoroutine(MovePoint(agentObject, point, unit));
        //}
        //Starts the animation of the ground marker
        groundMarker.transform.position = point;
        groundMarker.SetActive(false);
        groundMarker.SetActive(true);
    }

    //Stop command
    public void Stop(NavMeshAgent agent)
    {
        agentObject = agent.gameObject;
        unit = agentObject.GetComponent<UnitInfo>();
        //if (!unit.IsPlayerOwnerToUnit())
        //{
        //    return;
        //}
        unit.Animator.SetBool("IsAttack", false);
        unit.Animator.SetBool("IsMoved", false);
        unit.attackManager.standPost = Vector3.zero;
        unit.SetTask(TaskList.Idle);
        agent.destination = agentObject.transform.position;
        OnStopUnit?.Invoke(unit);
    }

    //Sets the command "Idle" when the unit reaches the point.
    IEnumerator MovePoint(GameObject moveObject, Vector3 point, UnitInfo objectInfo)
    {
        objectInfo.TurnToSide(point);
        while (moveObject != null && objectInfo.GetTask() == TaskList.Move && !IfPositionIsPoint(moveObject.transform.position, point) && objectInfo.GetMovePoint() == point)
        {
            yield return null;
        }
        if (moveObject == null)
        {
            yield break;
        }
        if (IfPositionIsPoint(moveObject.transform.position, point) && objectInfo.GetMovePoint() == point)
        {
            if (objectInfo.stopDelegate != null)
                objectInfo.stopDelegate();
        }
    }

    bool IfPositionIsPoint(Vector3 position, Vector3 point)
    {
        return position.x == point.x && position.z == point.z;
    }
}
