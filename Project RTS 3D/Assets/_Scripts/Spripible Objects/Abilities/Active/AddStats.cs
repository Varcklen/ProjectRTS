using UnityEngine;
using Project.AbilitySystem;
using Project.BuffSystem;

[CreateAssetMenu(fileName = "New AddStats", menuName = "Custom/Ability/Active/AddStats")]
public class AddStats : ActiveAbility<Ability_AddStats, AddStats_Stats>
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
public class AddStats_Stats : IStatsSO<AddStats_Stats>
{
    public BuffData buff;

    public int time;
    public int strengthBonus;

    public void SetStatsFromSO(AddStats_Stats so)
    {
        buff = so.buff;
        time = so.time;
        strengthBonus = so.strengthBonus;
    }
}

public class Ability_AddStats : MonoAbility<AddStats_Stats>, IActiveAbilityInit, IBuffInvoke
{
    private HeroInfo target;

    //Use abilityStats to get data from AddStats_Stats
    public void Init(UnitInfo caster, AbilityObject ability)
    {
    }

    public void Use(UseParams param)
    {
        target = param.target?.GetComponent<HeroInfo>();
        if (target == null) return;
        BuffObject buff = target.buffManager.CreateBuff(abilityStats.buff);
        buff.Invoke(new BuffStruct { target = target }, abilityStats.time, this);
    }

    public void BuffStart(BuffStruct param)
    {
        if (param.target is HeroInfo heroInfo)
        {
            Debug.Log("Str added!");
            heroInfo.HeroStats.Strength += abilityStats.strengthBonus;
            heroInfo.HeroStats.Agility += abilityStats.strengthBonus;
            heroInfo.HeroStats.Intelligence += abilityStats.strengthBonus;
        }
    }

    public void BuffEnd(BuffStruct param)
    {
        if (param.target is HeroInfo heroInfo)
        {
            Debug.Log("Str removed!");
            heroInfo.HeroStats.Strength -= abilityStats.strengthBonus;
            heroInfo.HeroStats.Agility -= abilityStats.strengthBonus;
            heroInfo.HeroStats.Intelligence -= abilityStats.strengthBonus;
        }
    }

}
