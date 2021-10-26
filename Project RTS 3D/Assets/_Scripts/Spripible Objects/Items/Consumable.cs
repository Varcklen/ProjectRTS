using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Custom/Items/Consumable")]
public class Consumable : ItemData
{
    private void Awake()
    {
        item.itemType = ItemType.Consumable;
    }
}
