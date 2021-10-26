using Project.BuffSystem;
using System;
using System.Collections;
using UnityEngine;

public class DamageManager
{
    public delegate void OnTakeDamage(UnitInfo dealer, ref DamageStruct damageStruct, in float oldDamage);
    public OnTakeDamage OnUntilUnitTakeDamage;
    public Action<DamageStruct> OnAfterDamageModifiers;
    public Action<DamageStruct> OnAfterUnitTakeDamage;
    public Action OnUnitDied;

    public static Action<UnitInfo> OnUnitDiedGlobal;

    public bool isDead;

    private UnitInfo unit;

    private UnitStats stats;

    private BuffObject shieldBuff;

    #region Invulnerable
    private int invulPoints;
    public int InvulPoints
    {
        get
        {
            return invulPoints;
        }
        set
        {
            invulPoints = value;
            if (invulPoints < 0)
            {
                Debug.LogWarning("DamageManager. \"invulPoints\" variable is less than 0.");
            }
        }
    }
    public bool IsInvulnerable { get { return InvulPoints > 0; } }
    #endregion

    public float MaxShield { get; private set; }
    public float Shield { get; private set; }

    private readonly LayerMask defaultLayerMask;

    private const string DODGE_INSCRIPTION = "Dodge!";
    private const float TIME_TO_DELETE_AFTER_DEATH = 3f;

    public DamageManager(UnitInfo unit)
    {
        this.unit = unit;
        stats = unit.Stats;
        defaultLayerMask = LayerMask.NameToLayer("Default");
        OnUntilUnitTakeDamage += DamageResistance;
        OnUntilUnitTakeDamage += SpellDamage;
        OnAfterDamageModifiers += FloatingTextDamage;
    }

    public void OnDestroy()
    {
        OnUntilUnitTakeDamage -= DamageResistance;
        OnUntilUnitTakeDamage -= SpellDamage;
        OnAfterDamageModifiers -= FloatingTextDamage;
    }

    //Activated when a unit takes damage
    public void TakeDamage(UnitInfo dealer, float damage, AttackType attackType)
    {
        if (isDead)
        {
            return;
        }
        DamageStruct damageStruct = new DamageStruct { damage = damage, attackType = attackType };
        OnUntilUnitTakeDamage?.Invoke(dealer, ref damageStruct, damage);
        OnAfterDamageModifiers?.Invoke(damageStruct);
        ShieldDamage(ref damageStruct);
        GeneralDamageChange(ref damageStruct);
        OnAfterUnitTakeDamage?.Invoke(damageStruct);
        if (stats.Health <= 0)
        {
            Kill();
        }
    }

    private void GeneralDamageChange(ref DamageStruct damageStruct)
    {
        if (damageStruct.damage < 0 || IsInvulnerable)
        {
            damageStruct.damage = 0;
        }
        switch (damageStruct.damageModifier)
        {
            case DamageModifier.Critical:
                damageStruct.damage *= 2;
                break;
            case DamageModifier.Dodge:
                if (damageStruct.attackType == AttackType.Physical)
                {
                    damageStruct.damage = 0;
                }
                break;
        }
        //Debug.Log("damage: " + damage);
        stats.Health -= damageStruct.damage;
    }

    //This is where the unit's base damage resistances are located.
    private void DamageResistance(UnitInfo dealer, ref DamageStruct damageStruct, in float oldDamage)
    {
        if (damageStruct.attackType == AttackType.Physical && Shield <= 0)
        {
            damageStruct.damage -= oldDamage * stats.ArmorPercent;
        }
    }

    //Increases magic damage based on Spell Power
    private void SpellDamage(UnitInfo dealer, ref DamageStruct damageStruct, in float oldDamage)
    {
        if (damageStruct.attackType == AttackType.Magical)
        {
            damageStruct.damage += oldDamage * dealer.Stats.SpellPowerPoint;
        }
    }

    //After all modifiers, it deals damage to the shield, if its possible
    private void ShieldDamage(ref DamageStruct damageStruct)
    {
        if (Shield > 0)
        {
            if (damageStruct.damage >= Shield)
            {
                damageStruct.damage -= Shield;
                DeleteShield();
            }
            else
            {
                Shield -= damageStruct.damage;
                if (Shield <= 0)
                {
                    DeleteShield();
                }
                damageStruct.damage = 0;
            }
        }
    }

    /// <summary>
    /// Activate this to kill the unit.
    /// </summary>
    public void Kill()
    {
        isDead = true;
        OnUnitDied?.Invoke();
        OnUnitDiedGlobal?.Invoke(unit);
        if (unit.isSelected)
        {
            if (UnitSelections.Instance.unitSelected.Count == 1)
            {
                UI_Manager.Instance.ClearInfoPanel();
            }
            else
            {
                //Remove a dead unit from the unit selected list
                //UnitSelections.Instance.Deselect(unit.gameObject);
            }
        }
        unit.Animator.SetBool("IsDead", true);
        unit.gameObject.layer = defaultLayerMask;
        unit.StartCoroutine(DeleteUnit());
    }

    IEnumerator DeleteUnit()
    {
        yield return new WaitForSeconds(TIME_TO_DELETE_AFTER_DEATH);
        if (unit.IsUnitHasFlag(ObjectType.Hero))
        {
            stats.Health = 0;
            unit.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log(unit.gameObject + " is dead.");
            GameObject.Destroy(unit.gameObject);
        }
    }

    ///<summary>
    ///Adds a shield to the unit.
    ///</summary>
    public void AddShield(UnitInfo caster, float addedShield)
    {
        if (Shield <= 0)
        {
            shieldBuff = unit.buffManager.CreateBuff(LoadResourceManager.Instance.baseShieldBuff);
        }
        Shield += (addedShield * (caster.Stats.SpellPowerPoint+1));
        if (Shield > MaxShield)
        {
            MaxShield = Shield;
        }
    }

    ///<summary>
    ///Destroys a shield from the unit.
    ///</summary>
    public void DeleteShield()
    {
        shieldBuff.BuffEnd();
        MaxShield = 0;
        Shield = 0;
    }

    ///<summary>
    ///Displays text above the target indicating how much damage was dealt.
    ///</summary>
    private void FloatingTextDamage(DamageStruct damageStruct)
    {
        switch (damageStruct.damageModifier)
        {
            case DamageModifier.Critical:
                if (damageStruct.damage >= 1)
                {
                    FloatingText.Create(unit.transform.position, Mathf.CeilToInt(damageStruct.damage).ToString() + "!", Color.red, TextSize.Big);
                }
                break;
            case DamageModifier.Dodge:
                FloatingText.Create(unit.transform.position, DODGE_INSCRIPTION, Color.red, TextSize.Small);
                break;
            default:
                if (damageStruct.damage >= 1)
                {
                    FloatingText.Create(unit.transform.position, Mathf.CeilToInt(damageStruct.damage).ToString(), Color.yellow, TextSize.Medium);
                }
                break;
        }
    }
}

//Damage data
public struct DamageStruct
{
    public float damage;
    public DamageModifier damageModifier;
    public AttackType attackType;
}

//Damage modifiers.
public enum DamageModifier : byte
{
    None,
    Critical, //Deal x2 damage
    Dodge   //if physical, deal 0 damage
}
