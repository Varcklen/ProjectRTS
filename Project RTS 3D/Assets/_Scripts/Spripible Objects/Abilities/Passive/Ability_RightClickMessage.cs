using UnityEngine;
using Project.AbilitySystem;

//A test ability that takes an action whenever the right mouse button is pressed.
[CreateAssetMenu(fileName = "New Ability_RightClickMessage", menuName = "Custom/Ability/Passive/RightClickMessage")]
public class Ability_RightClickMessage : PassiveAbility<PassiveTrigger, NullStat>
{
    public override void Init(ObjectInfo caster, AbilityObject ability) {
        InitComponent(caster, ability);
    }
}

public class PassiveTrigger : MonoAbility<NullStat>, IPassiveAbilityInit
{
    private Ability ability;

    public void Init(UnitInfo caster, AbilityObject ability)
    {
        this.ability = ability;

        InputManager.OnRightClick += Use;
    }

    private void OnDestroy()
    {
        InputManager.OnRightClick -= Use;
    }

    private void Use(RaycastHit hit)
    {
        Debug.Log("Click!");
        Debug.Log("isPassive: " + ability.isPassive);
    }
}
