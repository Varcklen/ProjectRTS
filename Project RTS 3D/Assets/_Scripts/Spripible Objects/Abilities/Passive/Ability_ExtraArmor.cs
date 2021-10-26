using UnityEngine;
using Project.AbilitySystem;

[CreateAssetMenu(fileName = "New Ability_ExtraArmor", menuName = "Custom/Ability/Passive/ExtraArmor")]
public class Ability_ExtraArmor : PassiveAbility<NullMono, NullStat>
{
    public int extraArmor;

    public override void AddedToUnit(UnitInfo caster)
    {
        caster.GetStats().Armor += extraArmor;
    }

    public override void RemovedFromUnit(UnitInfo caster)
    {
        caster.GetStats().Armor -= extraArmor;
    }
}
