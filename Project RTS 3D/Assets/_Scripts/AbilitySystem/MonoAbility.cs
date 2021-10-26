using UnityEngine;
using Project.AbilitySystem;

public class MonoAbility<T> : MonoBehaviour, IAbilitySOStats<T> where T : class, IStatsSO<T>, new()
{
    private T abilityParams = new T();
    //Allows you to use data from the Scriptable Object from which it was created.
    public T abilityStats{ get => abilityParams; }
}