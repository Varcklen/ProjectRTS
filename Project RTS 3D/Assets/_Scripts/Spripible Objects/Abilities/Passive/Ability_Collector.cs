using UnityEngine;
using Project.AbilitySystem;

//Store collector information. Every collector must have this ability.
[CreateAssetMenu(fileName = "New Ability_Collector", menuName = "Custom/Ability/Passive/Collector")]
public class Ability_Collector : PassiveAbility<CollectorMono, Ability_Collector_Stats>
{
    public override void Init(ObjectInfo caster, AbilityObject ability)
    {
        InitComponent(caster, ability);
    }
}

[System.Serializable]
public class Ability_Collector_Stats : IStatsSO<Ability_Collector_Stats>
{
    [Header("Stats")]
    public float distanceToDeliver = 64f;

    public void SetStatsFromSO(Ability_Collector_Stats so)
    {
        distanceToDeliver = so.distanceToDeliver;
    }
}

public class CollectorMono : MonoAbility<Ability_Collector_Stats>, IPassiveAbilityInit
{
    public void Init(UnitInfo caster, AbilityObject ability)
    {
    }
}
