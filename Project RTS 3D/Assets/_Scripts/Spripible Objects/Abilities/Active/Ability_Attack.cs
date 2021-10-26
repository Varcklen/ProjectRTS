using UnityEngine;
using Project.AbilitySystem;

//Selects a target and attacks it
[CreateAssetMenu(fileName = "New Ability_Attack", menuName = "Custom/Ability/Active/Attack")]
public class Ability_Attack : ActiveAbility<AttackMono, NullStat>
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

public class AttackMono : MonoAbility<NullStat>, IActiveAbilityInit
{
    private UnitInfo caster;

    public void Init(UnitInfo caster, AbilityObject ability)
    {
        this.caster = caster;

        InputManager.OnRightClick += RightClick;
    }

    private void OnDestroy()
    {
        InputManager.OnRightClick -= RightClick;
    }

    private void RightClick(RaycastHit hit)
    {
        if (!hit.collider.TryGetComponent(out UnitInfo target) || !caster.isSelected)
        {
            return;
        }
        if (!caster.IsUnitAllyToUnit(target))
        {
            Use(new UseParams { target = hit.collider.gameObject });
        }
    }

    public void Use(UseParams param)
    {
        if (!param.target.TryGetComponent(out UnitInfo target))
        {
            return;
        }
        caster.attackManager.AttackTarget(target);
    }
}
