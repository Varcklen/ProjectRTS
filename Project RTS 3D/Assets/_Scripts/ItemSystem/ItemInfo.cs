using UnityEngine;

public class ItemInfo : ObjectInfo
{
    [field: Header("Stats"), SerializeField, ConditionalHide("unique", false)]
    public ItemObject Stats { get; private set; }

    [Header("Parameters")]
    [SerializeField] private ItemData itemData;

    private void Awake()
    {
        // Initializing Stats if it wasn't already in order to make unit testing easier
        if (Stats == null)
		{
            Stats = new ItemObject();
        }
        AwakeInfo();
        SetItem();
        selectionDelegate = Selection;
    }

    private void Start()
    {
        StartInfo();
        AddItemAbilities();
    }

    private void Selection()
    {
        UI_Manager.Instance.UIDisplay(this);
    }

    //Sets parameters for a unit in the editor.
    private void OnDrawGizmos()
    {
        if (itemData == null || Application.isPlaying)
            return;
        SetItem();
    }

    #region SetRegion
    public void SetItemData(ItemData itemData)
    {
        this.itemData = itemData;
        SetItem();
    }
    #endregion

    #region GetRegion
    public ItemObject GetItemStats()
    {
        return Stats;
    }
    #endregion

    //Sets parameters from a Scriptible Object
    void SetItem()
    {
        if (itemData == null)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("\"Item\" for " + gameObject + " is not setted.");
            }
            return;
        }
        //objectType = ObjectType.Item;
        if (minimapIcon != null)
        {
            minimapIcon.sprite = GlobalMethods.GetMinimapSprite(objectType);
        }
        else
        {
            minimapIcon = transform.Find("MinimapImage")?.GetComponent<SpriteRenderer>();
        }
        if (unique)
        {
            return;
        } 
        Stats.SetItemFromData(itemData);
        
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    //Adds the components of the abilities it has to the item.
    //This is necessary for the tooltip to display correctly when selecting an item.
    public void AddItemAbilities()
    {
        foreach (Project.AbilitySystem.AbilityObject ability in Stats.abilities)
        {
            ability.level = 1;
            ability.AbilityInitialization(ability.abilityData, this, ability);
        }
    }
}
