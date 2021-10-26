using UnityEngine;
using Project.AbilitySystem;
using System.Text;

//When a unit dies, it deals damage to nearby creatures.
[CreateAssetMenu(fileName = "New Ability_DeathExplode", menuName = "Custom/Ability/Passive/DeathExplode")]
public class Ability_DeathExplode : PassiveAbility<DeathExplode, Ability_DeathExplode_Stats>
{
    public override void Init(ObjectInfo caster, AbilityObject ability)
    {
        InitComponent(caster, ability);
    }
}

[System.Serializable]
public class Ability_DeathExplode_Stats : IStatsSO<Ability_DeathExplode_Stats>
{
    [Header("Stats")]
    public int damage;

    public void SetStatsFromSO(Ability_DeathExplode_Stats so)
    {
        damage = so.damage;
    }
}

public class DeathExplode : MonoAbility<Ability_DeathExplode_Stats>, IPassiveAbilityInit, IReplaceTextInAbilityTooltip
{
    private Ability ability;

    private UnitInfo caster;

    public void Init(UnitInfo caster, AbilityObject ability)
    {
        this.ability = ability;
        this.caster = caster;

        if (caster != null)
        {
            caster.damageManager.OnUnitDied += Use;
        }
    }

    public string ReplaceTextForAbility(string description)
    {
        StringBuilder newDescription = new StringBuilder(description);

        string newText = ColorText.ColorStringAndChangeNumber(caster, abilityStats.damage, ColorPreset.SpellPower);
        newDescription.Replace("{damage}", newText);

        return newDescription.ToString();
    }

    private void OnDestroy()
    {
        if (caster != null)
        {
            caster.damageManager.OnUnitDied -= Use;
        }
    }

    private void Use()
    {
        Debug.Log("Explode:" + caster.GetStats().name);
        Collider[] colliders = Physics.OverlapSphere(caster.transform.position, ability.area, GlobalMethods.GetSelectableMask());
        foreach (Collider collider in colliders)
        {
            if (!collider.TryGetComponent(out UnitInfo target)) continue;
            if ((target.GetObjectType() & ability.targetType) != 0 && caster != target)
            {
                Debug.Log("Exploded:" + collider.gameObject);
                target.TakeDamage(caster, abilityStats.damage, AttackType.Magical);
            }
        }
    }
}
