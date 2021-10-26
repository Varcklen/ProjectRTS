using UnityEngine;
using Project.BuffSystem;
using Project.AbilitySystem;

//Reduces the target's health periodically
[CreateAssetMenu(fileName = "New Ability_Bleed", menuName = "Custom/Ability/Active/Bleed")]
public class Ability_Bleed : ActiveAbility<Bleed, Ability_Bleed_Stats>
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
public class Ability_Bleed_Stats : IStatsSO<Ability_Bleed_Stats>
{
    public BuffData buff;

    public int time;
    public int damagePerTick;
    public float tickTime;

    public void SetStatsFromSO(Ability_Bleed_Stats so)
    {
        buff = so.buff;
        time = so.time;
        damagePerTick = so.damagePerTick;
        tickTime = so.tickTime;
    }
}

public class Bleed : MonoAbility<Ability_Bleed_Stats>, IActiveAbilityInit, IBuffDoT
{
    private UnitInfo caster;
    private UnitInfo target;

    public void Init(UnitInfo caster, AbilityObject ability)
    {
        this.caster = caster;
    }

    public void Use(UseParams param)
    {
        target = param.target?.GetComponent<UnitInfo>();
        if (target == null) return;
        BuffObject buff = target.buffManager.CreateBuff(abilityStats.buff);
        buff.Invoke(new BuffStruct { target = target }, abilityStats.time, this, abilityStats.tickTime);
    }

    public void DoTUse(BuffStruct param)
    {
        Debug.Log("Damage dealt!");
        param.target.TakeDamage(caster, 1, AttackType.Magical);
    }
}
