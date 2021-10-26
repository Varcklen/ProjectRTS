using Project.AbilitySystem;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public Item item = new Item();
    public List<AbilityData> abilities;
}

public enum ItemType
{
    Item,
    Consumable,
    Artifact
}

public enum ItemRarity
{
    Common,
    Rare,
    Legendary
}

[System.Flags]
public enum ItemSet
{
    NoSet = 0,
    Blood = 1,
    Rune = 2,
    Mech = 4,
    Moon = 8,
    Nature = 16,
    Alchemy = 32,
    Ring = 64,
    Crystal = 128,
    Weapon = 256
}
