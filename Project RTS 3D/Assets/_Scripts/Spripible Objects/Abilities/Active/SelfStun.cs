using UnityEngine;
using Project.AbilitySystem;
using Project.BuffSystem;

[CreateAssetMenu(fileName = "New SelfStun", menuName = "Custom/Ability/Active/SelfStun")]
public class SelfStun : ActiveAbility<Ability_SelfStun, SelfStun_Stats>
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
public class SelfStun_Stats : IStatsSO<SelfStun_Stats>
{
    public BuffData buff;
    public float time;

    public void SetStatsFromSO(SelfStun_Stats so)
    {
        buff = so.buff;
        time = so.time;
    }
}

public class Ability_SelfStun : MonoAbility<SelfStun_Stats>, IActiveAbilityInit, IBuffInvoke
{
    private UnitInfo caster;

    //Use abilityStats to get data from SelfSilence_Stats
    public void Init(UnitInfo caster, AbilityObject ability)
    {
        this.caster = caster;
    }

    public void Use(UseParams param)
    {
        BuffObject buff = caster.buffManager.CreateBuff(abilityStats.buff);
        buff.Invoke(new BuffStruct { target = caster }, abilityStats.time, this);
    }

    public void BuffStart(BuffStruct param)
    {
        param.target.AddStunPoints(1);
    }

    public void BuffEnd(BuffStruct param)
    {
        param.target.AddStunPoints(-1);
    }

}
