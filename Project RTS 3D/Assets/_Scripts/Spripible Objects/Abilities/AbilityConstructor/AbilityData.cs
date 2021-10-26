using System.Collections.Generic;
using UnityEngine;

namespace Project.AbilitySystem
{

    //Ability sheath
    public abstract class AbilityData : ScriptableObject
    {
        public Ability ability;

        //Use this when initializing an Ability and passing data from a Scriptable Object to an Ability.
        public virtual void Init(ObjectInfo caster, AbilityObject ability) { }
        //Use this when activating the ability
        public virtual void Use(UseParams p) { }
        //Use this when adding an ability with a unit.
        public virtual void AddedToUnit(UnitInfo caster) { }
        //Use this when removing an ability from a unit
        public virtual void RemovedFromUnit(UnitInfo caster) { }
        public abstract bool isPassive { get; }
    }

    //A structure that passes different parameters depending on the target type
    public struct UseParams
    {
        public UnitInfo caster;
        public GameObject target;
        public Ability ability;
        public Vector3 point;
        public List<UnitInfo> areaTargets;
    }

    //Interface with method for setting parameters from Scriptable Object to Ability
    public interface IStatsSO<T> where T : class
    {
        public void SetStatsFromSO(T so);
    }

    //Interface with methods for initializing and using the ability
    public interface IActiveAbilityInit
    {
        void Init(UnitInfo caster, AbilityObject ability);
        void Use(UseParams param);
    }

    //Interface with method to initialize the ability
    public interface IPassiveAbilityInit
    {
        void Init(UnitInfo caster, AbilityObject ability);
    }


    public interface IAbilitySOStats<T>
    {
        T abilityStats { get; }
    }

    //Dummy classes that are used when the ability does not need a MonoBehaviour component or/and Stats class.
    public class NullMono : MonoAbility<NullStat>, IActiveAbilityInit, IPassiveAbilityInit
    {
        public void Init(UnitInfo caster, AbilityObject ability) { }
        public void Use(UseParams param) { }
    }

    public class NullStat : IStatsSO<NullStat>
    {
        public void SetStatsFromSO(NullStat so) { }
    }
}