using UnityEngine;
using Project.AbilitySystem;

//Deals area damage to all enemies
[CreateAssetMenu(fileName = "New Ability_Slam", menuName = "Custom/Ability/Active/Slam")]
public class Ability_Slam : ActiveAbility<SlamMono, NullStat>
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

public class SlamMono : MonoAbility<NullStat>, IActiveAbilityInit
{
    public void Init(UnitInfo caster, AbilityObject ability)
    {
    }

    public void Use(UseParams param)
    {
        if (param.point == Vector3.zero)
        {
            Debug.LogWarning("areaX and areaY is 0.");
            return;
        }
        foreach (UnitInfo target in param.areaTargets)
        {
            Debug.Log("Slammed:" + target.Stats.name);
        }
    }
}
