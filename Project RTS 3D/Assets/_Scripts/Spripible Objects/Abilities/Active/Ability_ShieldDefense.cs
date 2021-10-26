using UnityEngine;
using Project.BuffSystem;
using Project.AbilitySystem;

//Adds armor to the target upon use.
[CreateAssetMenu(fileName = "New Ability_ShieldDefense", menuName = "Custom/Ability/Active/ShieldDefense")]
public class Ability_ShieldDefense : ActiveAbility<ShieldDefense, Ability_ShieldDefense_Stats>
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
public class Ability_ShieldDefense_Stats : IStatsSO<Ability_ShieldDefense_Stats>
{
    public BuffData buff;

    public int time;
    public int armorBonus;

    public void SetStatsFromSO(Ability_ShieldDefense_Stats so)
    {
        buff = so.buff;
        time = so.time;
        armorBonus = so.armorBonus;
    }
}

public class ShieldDefense : MonoAbility<Ability_ShieldDefense_Stats>, IActiveAbilityInit, IBuffInvoke
{
    private UnitInfo target;

    public void Init(UnitInfo caster, AbilityObject ability)
    {
        
    }

    public void Use(UseParams param)
    {
        target = param.target?.GetComponent<UnitInfo>();
        if (target == null) return;
        BuffObject buff = target.buffManager.CreateBuff(abilityStats.buff);
        buff.Invoke(new BuffStruct{target = target}, abilityStats.time, this);
    }

    public void BuffStart(BuffStruct param)
    {
        Debug.Log("Armor added!");
        param.target.GetStats().Armor += abilityStats.armorBonus;
    }

    public void BuffEnd(BuffStruct param)
    {
        Debug.Log("Armor removed!");
        param.target.GetStats().Armor -= abilityStats.armorBonus;
    }

}