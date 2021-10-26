using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Custom/Items/Inventory")]
public class InventoryData : ScriptableObject
{
    public List<ItemData> container = new List<ItemData>();
}
