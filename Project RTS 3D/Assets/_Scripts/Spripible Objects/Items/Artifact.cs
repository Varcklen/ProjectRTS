using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Artifact", menuName = "Custom/Items/Artifact")]
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ItemSet), true)] 
#endif
public class Artifact : ItemData
{
    private void Awake()
    {
        item.itemType = ItemType.Artifact;
    }
}
