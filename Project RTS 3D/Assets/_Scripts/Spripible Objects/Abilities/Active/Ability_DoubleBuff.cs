using UnityEngine;
using Project.BuffSystem;
using Project.AbilitySystem;

//Adds armor to the target upon use. After that, add attack.
[CreateAssetMenu(fileName = "New Ability_DoubleBuff", menuName = "Custom/Ability/Active/DoubleBuff")]
public class Ability_DoubleBuff : ActiveAbility<DoubleBuff, Ability_DoubleBuff_Stats>
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
public class Ability_DoubleBuff_Stats : IStatsSO<Ability_DoubleBuff_Stats>
{
    [Header("Buff One")]
    public BuffData buffOne;
    public int timeOne;
    public int armorBonus;

    [Header("Buff Two")]
    public BuffData buffTwo;
    public int timeTwo;
    public int attackBonus;

    public void SetStatsFromSO(Ability_DoubleBuff_Stats so)
    {
        buffOne = so.buffOne;
        timeOne = so.timeOne;
        armorBonus = so.armorBonus;

        buffTwo = so.buffTwo;
        timeTwo = so.timeTwo;
        attackBonus = so.attackBonus;
    }
}

public class DoubleBuff : MonoAbility<Ability_DoubleBuff_Stats>, IActiveAbilityInit, IBuffInvoke
{
    private UnitInfo target;

    public void Init(UnitInfo caster, AbilityObject ability)
    {
    }

    public void Use(UseParams param)
    {
        target = param.target?.GetComponent<UnitInfo>();
        if (target == null) return;
        BuffObject buff = target.buffManager.CreateBuff(abilityStats.buffOne);
        buff.Invoke(new BuffStruct { target = target }, abilityStats.timeOne, this);
    }

    public void BuffStart(BuffStruct param)
    {
        if (param.alternative == 1)
        {
            Debug.Log("Attack damage added!");
            param.target.GetStats().attack.Damage += abilityStats.attackBonus;
        }
        else
        {
            Debug.Log("Armor added!");
            param.target.GetStats().Armor += abilityStats.armorBonus;
        }
        
    }

    public void BuffEnd(BuffStruct param)
    {
        if (param.alternative == 1)
        {
            Debug.Log("Attack damage removed!");
            param.target.GetStats().attack.Damage -= abilityStats.attackBonus;
        }
        else
        {
            Debug.Log("Armor removed!");
            param.target.GetStats().Armor -= abilityStats.armorBonus;
            BuffObject buff = param.target.buffManager.CreateBuff(abilityStats.buffTwo);
            buff.Invoke(new BuffStruct { target = param.target, alternative = 1 }, abilityStats.timeTwo, this);
        }
    }
}