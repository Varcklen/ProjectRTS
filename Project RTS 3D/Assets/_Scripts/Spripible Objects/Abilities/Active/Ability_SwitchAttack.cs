using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.AbilitySystem;

//Changes attack modes: Aggressive/Defensive/Stand
[CreateAssetMenu(fileName = "New Ability_SwitchAttack", menuName = "Custom/Ability/Active/SwitchAttack")]
public class Ability_SwitchAttack : ActiveAbility<NullMono, NullStat>
{
    public override void Use(UseParams p)
    {
        if (!p.caster.TryGetComponent(out UnitInfo objectInfoCaster))
            return;
        Debug.Log("Switched!");
        objectInfoCaster.attackManager.SwitchAttackState();
    }
}
