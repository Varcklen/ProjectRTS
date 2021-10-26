using UnityEngine;

/*Prevents the unit's minimap icon from spinning behind units. Incomplete.
*The icon is always located at the angle above which it was originally, even if the initial angle was set incorrectly.
*/
public class MinimapImage : MonoBehaviour
{
    Quaternion rotation;
    void Awake()
    {
        rotation = transform.rotation;
    }
    void LateUpdate()
    {
        transform.rotation = rotation;
    }
}
