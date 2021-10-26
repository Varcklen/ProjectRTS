using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Try to make the movement of the detachment. It is worth completing or reworking.
public class SquadParent : MonoBehaviour
{
    [SerializeField] int squadLimit = 12;

    public GameObject target;
    public GameObject child_prefab;
    public List<GameObject> childrens = new List<GameObject>();

    //RaycastHit RaycastTarget()
    //{
    //    //Vector3 fromPosition = source.transform.position;
    //    //Vector3 toPosition = destination.transform.position;
    //    //Vector3 direction = toPosition - fromPosition;
    //    RaycastHit hit;

    //    if (Physics.Raycast(target.transform.position, Vector3.zero, out hit))
    //    {
    //        print("ray just hit the gameobject: " + hit.collider.gameObject.name);
    //        return hit;
    //    }
    //    return hit;
    //}

    private void Start()
    {
        for (int i = 0; i < squadLimit; i++)
        {
            Vector3 relative_spawn = new Vector3(i%4, 0.35f, i/4);
            GameObject temp = Instantiate(child_prefab, transform.position + relative_spawn * 6f, transform.rotation);
            //ActionList.Instance.Move(temp.GetComponent<Peasant>().agent, target.transform.position);
            childrens.Add(temp);
        }
    }

    private void Update()
    {
        transform.position += (target.transform.position - transform.position).normalized * Time.deltaTime * 5f;
    }
}
