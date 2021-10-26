using UnityEngine;

namespace Project.AbilitySystem
{
    public abstract class PassiveAbility<T, E> : AbilityData
    where T : Component, IPassiveAbilityInit, IAbilitySOStats<E>
    where E : class, IStatsSO<E>
    {
        [SerializeField] private E abilityStats;
        public override bool isPassive => true;

        //Initializes the ability for the unit
        public T InitComponent(ObjectInfo myObject, AbilityObject ability)
        {
            ability.isPassive = isPassive;
            T passiveTrigger = myObject.gameObject.AddComponent<T>();
            ability.component = passiveTrigger;
            SetStatsSO(passiveTrigger.abilityStats);
            if (myObject is UnitInfo unit)
            {
                passiveTrigger.Init(unit, ability);
            }
            return passiveTrigger;
        }

        //Sets parameters from the E class of the Scriptable Object to the T component
        private void SetStatsSO(IStatsSO<E> so)
        {
            so.SetStatsFromSO(abilityStats);
        }
    }
}