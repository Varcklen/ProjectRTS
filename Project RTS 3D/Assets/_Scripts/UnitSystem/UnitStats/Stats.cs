using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Stats
{
    public string name = "New Unit";
    public Sprite icon;
    [TextArea] public string description;

    [Header("Attributes")]
    #region MaxHealth
    [SerializeField]
    private float maxHealth;
    public float MaxHealth
    {
        get { return Mathf.Clamp(maxHealth, 1, Mathf.Infinity); }
        set { maxHealth = value; }
    }
    #endregion
    public float maxMana;
    public float healthRegeneration;
    public float manaRegeneration;

    private const float ARMOR_COEFFICIENT = 0.06f;

    #region Armor
    [SerializeField]
    private float armor;
    public float Armor {
        get { return armor; }
        set
        {
            armor = value;
            ArmorPercent = armor;
            UI_Manager.Instance?.RefreshInfoPanel();
        }
    }

    private float armorPercent;
    public float ArmorPercent
    {
        get { return armorPercent; }
        private set
        {
            float armory = value * ARMOR_COEFFICIENT;
            armorPercent = armory / (1 + armory);
            armorPercent = Mathf.Round(armorPercent * 1000f) * 0.001f;
        }
    }
    #endregion
    #region SpellPower
    //Spell power in %
    [SerializeField]
    private float spellPower;
    public float SpellPower
    {
        get { return spellPower; }
        set
        {
            spellPower = value;
            UI_Manager.Instance?.RefreshInfoPanel();
        }
    }

    //Spell power in points
    public float SpellPowerPoint
    { 
        get
        {
            return Mathf.Clamp((spellPower / 100f), 0, Mathf.Infinity); 
        } 
    }
    #endregion
    #region Luck
    //Luck points
    private int luckPoint;
    public int LuckPoint
    {
        get { return luckPoint; }
        set
        {
            luckPoint = value;
            LuckPercent = value;
            UI_Manager.Instance?.RefreshInfoPanel();
        }
    }

    //Luck %
    private int luck;
    public float LuckPercent
    {
        get { return luck; }
        private set
        {
            float lucky = 0;
            for (int i = 0; i < value; i++)
            {
                lucky += (Mathf.Sqrt(i) / (i + 1));
            }
            luck = Mathf.FloorToInt(lucky);
        }
    }
    #endregion
    public float movementSpeed = 10f;
    public float angularSpeed = 180f;

    [Header("Other")]
    [Range(0, 6)] public int inventoryLimit;
    public InventoryData inventoryData;

    [Header("Attack")]
    public Attack attack;

    public void SetStatsFromData(Unit unit)
    {
        Stats so = unit.stats;

        name = so.name;
        icon = so.icon;
        description = so.description;
        MaxHealth = so.MaxHealth;
        maxMana = so.maxMana;
        attack.SetAttackStatsFromUnit(unit);
        Armor = so.Armor;
        inventoryLimit = so.inventoryLimit;
        inventoryData = so.inventoryData;
        LuckPoint = so.LuckPoint;
        spellPower = so.spellPower;
        healthRegeneration = so.healthRegeneration;
        manaRegeneration = so.manaRegeneration;
        movementSpeed = so.movementSpeed;
        angularSpeed = so.angularSpeed;
    }

    //Sets movement parameters for a unit
    public void SetNavMeshParameters(NavMeshAgent agent)
    {
        agent.speed = movementSpeed;
        agent.angularSpeed = angularSpeed;
    }
}
