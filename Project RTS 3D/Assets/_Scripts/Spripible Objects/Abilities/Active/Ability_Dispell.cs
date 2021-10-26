using UnityEngine;
using Project.AbilitySystem;

//Remove all effects from the caster
[CreateAssetMenu(fileName = "New Ability_Dispell", menuName = "Custom/Ability/Active/Dispell")]
public class Ability_Dispell : ActiveAbility<NullMono, NullStat>
{
    public override void Use(UseParams p)
    {
        p.caster.GetComponent<UnitInfo>().buffManager.DispellAllBuffs();
    }
}