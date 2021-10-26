using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//Some useful static classes. It might be worth optimizing.
public static class GlobalMethods
{

    private static Sprite[] minimapSprite = Resources.LoadAll<Sprite>("MinimapSprites");

    public static Sprite GetMinimapSprite(ObjectType objectType)
    {
        if (objectType.HasFlag(ObjectType.Hero))
        {
            return minimapSprite[0];
        }
        else if (objectType.HasFlag(ObjectType.Boss))
        {
            return minimapSprite[2];
        }
        return minimapSprite[1];
    }

    //The condition of whether the cursor is on the interface or not.
    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    //Returns all children of the parent as List<GameObject>.
    public static List<GameObject> GetChildrens(this GameObject go)
    {
        List<GameObject> childrens = new List<GameObject>();
        foreach (Transform tran in go.transform)
        {
            childrens.Add(tran.gameObject);
        }
        return childrens;
    }
    public static List<Transform> GetChildrensTransform(this Transform go)
    {
        List<Transform> childrens = new List<Transform>();
        foreach (Transform tran in go)
        {
            childrens.Add(tran);
        }
        return childrens;
    }
    public static List<RectTransform> GetChildrensRectTransform(this Transform go)
    {
        List<RectTransform> childrens = new List<RectTransform>();
        foreach (RectTransform tran in go)
        {
            childrens.Add(tran);
        }
        return childrens;
    }

    //Returns the distance between two points.
    public static float DistanceBetweenPoints(Vector3 pointOne, Vector3 pointTwo)
    {
        return (pointOne - pointTwo).sqrMagnitude;
    }

    //Returns the Ground Mask. Worth improving in the future.
    public static LayerMask GetSelectableMask()
    {
        return 64;
    }

    //If the number is in the range between two numbers, returns true
    public static bool NumberInRange(float number, float min, float max)
    {
        return number >= min && number <= max;
    }

    //Checks the probability of triggering depending on the luck of the unit
    public static bool LuckChance(UnitInfo unit, int chance, int minimunPoint = 1, int maximumPoint = 100)
    {
        return Random.Range(minimunPoint, maximumPoint) >= (chance + unit.Stats.LuckPercent);
    }
}
