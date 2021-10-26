using UnityEngine;

namespace Project.AbilitySystem
{

    public abstract class ActiveAbility<T, E> : AbilityData
    where T : Component, IActiveAbilityInit, IAbilitySOStats<E>
    where E : class, IStatsSO<E>
    {
        [SerializeField] private E abilityStats;
        public override bool isPassive => false;

        //Initializes the ability for the object
        public T InitComponent(ObjectInfo myObject, AbilityObject ability)
        {
            ability.isPassive = isPassive;
            T activeTrigger = myObject.gameObject.AddComponent<T>();
            ability.component = activeTrigger;
            SetStatsSO(activeTrigger.abilityStats);
            if (myObject is UnitInfo unit)
            {
                activeTrigger.Init(unit, ability);
            }
            else
            {
                activeTrigger.Init(null, ability);
            }
            return activeTrigger;
        }

        //Sets parameters from the E class of the Scriptable Object to the T component
        private void SetStatsSO(IStatsSO<E> so)
        {
            so.SetStatsFromSO(abilityStats);
        }

        //Activates the ability
        public void UseComponent(UseParams param)
        {
            if (param.caster == null)
                return;
            if (!param.caster.TryGetComponent(out T component))
            {
                Debug.LogWarning("Unit " + param.caster + " does not have a required component.");
                return;
            }
            component.Use(param);
        }
    }
}
