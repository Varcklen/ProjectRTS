using UnityEngine;
using Project.AbilitySystem;
using System.Text;

[CreateAssetMenu(fileName = "New EnergyShield", menuName = "Custom/Ability/Active/EnergyShield")]
public class Ability_EnergyShield : ActiveAbility<EnergyShield, EnergyShield_Stats>
{
    public override void Init(ObjectInfo caster, AbilityObject ability)
    {
        InitComponent(caster, ability);
    }

    public override void Use(UseParams p)
    {
        UseComponent(p);
    }
}

[System.Serializable]
public class EnergyShield_Stats : IStatsSO<EnergyShield_Stats>
{
    public int addedShield;
    public int chance;

    [Space]
    public int addedShieldPerLevel;

    public void SetStatsFromSO(EnergyShield_Stats so)
    {
        addedShield = so.addedShield;
        chance = so.chance;
        addedShieldPerLevel = so.addedShieldPerLevel;
    }
}

public class EnergyShield : MonoAbility<EnergyShield_Stats>, IActiveAbilityInit, IReplaceTextInAbilityTooltip, IUpgradeAbility
{
    private UnitInfo target;
    private UnitInfo caster;

    private AbilityObject ability;

    private int addedShield;

    public void Init(UnitInfo caster, AbilityObject ability)
    {
        this.caster = caster;
        this.ability = ability;
        UpgradeAbility();
    }

    public string ReplaceTextForAbility(string description)
    {
        StringBuilder newDescription = new StringBuilder(description);

        string newText = ColorText.ColorStringAndChangeNumber(caster, addedShield, ColorPreset.SpellPower);
        newText += ColorText.AddAbilityUpgradeText(ability, abilityStats.addedShieldPerLevel);
        newDescription.Replace("{addedShield}", newText);

        newText = ColorText.ColorStringAndChangeNumber(caster, abilityStats.chance, ColorPreset.Luck);
        newDescription.Replace("{chance}", newText);

        return newDescription.ToString();
    }

    public void UpgradeAbility()
    {
        addedShield = abilityStats.addedShield + Mathf.FloorToInt(Mathf.Clamp(abilityStats.addedShieldPerLevel * (ability.level - 1), 0, Mathf.Infinity));
    }

    public void Use(UseParams param)
    {
        target = param.target?.GetComponent<UnitInfo>();
        if (target == null) return;
        int shield = addedShield;
        if (GlobalMethods.LuckChance(caster, abilityStats.chance))
        {
            Debug.Log("Chance procked! x2 shield");
            shield *= 2;
        }
        target.damageManager.AddShield(caster, shield);
    }

}
