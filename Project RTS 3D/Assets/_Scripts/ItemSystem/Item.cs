using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Project.AbilitySystem;

[System.Serializable]
public class Item : IReturnDescriptionTooltip
{
    [Header("Core")]
    public string name;
    public Sprite icon;
    public bool useDescriptionFromAbility;
    [ConditionalHide("useDescriptionFromAbility", true)] public int whichAbility;
    [TextArea] public string description;
    public string prefabPath = null;

    [Header("Parameters")]
    public ItemRarity rarity;//For Artifact
    public ItemSet itemSet;//For Artifact
    [Range(0,100)] public int charges;//For Consumable

    public List<AbilityObject> abilities = new List<AbilityObject>();

    [HideInInspector] public ItemType itemType;
    [HideInInspector] public ItemData itemData;
    [HideInInspector] public AbilityObject usedAbility;

    public void SetItemFromData(ItemData item)
    {
        Item so = item.item;

        name = so.name;
        icon = so.icon;
        description = so.description;
        if (so.prefabPath == "") prefabPath = "Artifact";//Other\\
        else prefabPath = so.prefabPath ?? "Artifact";
        itemType = so.itemType;
        charges = so.charges;
        rarity = so.rarity;
        itemSet = so.itemSet;
        useDescriptionFromAbility = so.useDescriptionFromAbility;
        whichAbility = so.whichAbility;
        itemData = item;
        AbilitySet();
    }

    public void AbilitySet()
    {
        if (itemData?.abilities == null || !Application.isPlaying) return;
        abilities.Clear();
        bool isHasActiveAbility = false;
        for (int i = 0; i < itemData.abilities.Count; i++)
        {
            AbilityObject ability = new AbilityObject(null, itemData.abilities[i]);
            abilities.Add(ability);
            if (!isHasActiveAbility && !ability.isPassive)
            {
                if (usedAbility == null)
                {
                    usedAbility = ability;
                }
                isHasActiveAbility = true;
            }
        }
        if (useDescriptionFromAbility)
        {
            if (GlobalMethods.NumberInRange(whichAbility, 0, Mathf.Clamp(abilities.Count-1, 0, Mathf.Infinity)))
            {
                usedAbility = abilities[whichAbility];
            }
            else
            {
                Debug.LogWarning("\"whichAbility\" variable in " + name + " ability is out of range. Please select a different value.");
            }
        }
    }

    public void RemoveItemAbilities(UnitInfo caster)
    {
        foreach (AbilityObject ability in abilities)
        {
            ability.abilityData.RemovedFromUnit(caster);
            Object.Destroy(ability.component);
        }
    }

    public void SetAbilitiesVision(bool isVisible)
    {
        foreach (AbilityObject ability in abilities)
        {
            ability.isVisible = isVisible;
        }
    }

    public static ItemInfo CreateItemObject(Vector3 point, ItemData itemData)
    {
        //Object prebab = Resources.Load(prefabPath);
        //if (prebab = null)
        //{
        //    Debug.LogWarning("The path to the object is wrong. Item cannot be created in " + this + "/" + MethodBase.GetCurrentMethod());
        //    return null;
        //}
        //Debug.Log("prefabPath: " + prefabPath);
        //Debug.Log("prebab: " + prebab);
        //GameObject prefab = ;
        GameObject newItem = GameObject.Instantiate(LoadResourceManager.Instance.baseItemPrefab, point, Quaternion.identity);
        ItemInfo itemInfo = newItem.GetComponent<ItemInfo>();
        itemInfo.SetItemData(itemData);
        itemInfo.GetItemStats().SetItemFromData(itemData);
        return itemInfo;
    }

    public void AddItemAbilities(UnitInfo caster)
    {
        foreach (AbilityObject ability in abilities)
        {
            ability.AbilityInitialization(ability.abilityData, caster, ability);
            ability.abilityData.AddedToUnit(caster);
        }
    }

    public string GetTooltipInfoText(DescriptionInfo descriptionInfo)
    {
        StringBuilder builder = new StringBuilder();

        if (!descriptionInfo.withoutName)
            builder.Append("<size=35>").Append(name).Append("</size>").AppendLine();
        if (useDescriptionFromAbility && usedAbility != null)
        {
            if (!usedAbility.isPassive)
            {
                builder.Append("Activation: ");
            }
            else if (descriptionInfo.key != KeyCode.None)
            {
                builder.Append("<color=red>Use:</color> ").Append(ColorText.KeyCodeToString(descriptionInfo.key)).AppendLine();
            }
            builder.Append(usedAbility.ReplaceTextInTooltip(usedAbility.description)).AppendLine();
            if (usedAbility.cooldown > 0)
            {
                builder.Append("<color=yellow>Cooldown:</color> ").Append(usedAbility.cooldown).Append(usedAbility.GetSecondTime(usedAbility.cooldown)).AppendLine();
            }  
            if (usedAbility.cost > 0)
            {
                builder.Append("<color=blue>Cost:</color> ").Append(usedAbility.cost).Append(" ").Append(usedAbility.GetCostTypeName());
            }  
        }
        else
        {
            builder.Append(description).AppendLine();
        }

        return builder.ToString();
    }
}
