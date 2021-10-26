using UnityEngine;
using System.Collections.Generic;
using Project.AbilitySystem;

//ScriptableObject for Unit
[CreateAssetMenu(fileName = "New Minion", menuName = "Custom/Unit/Minion")]
public class Unit : ScriptableObject
{
    [Header("Stats")]
    public Stats stats;
    
    [Header("Parameters")]
    public ObjectType objectType;
    public string unitPrefab;

    public List<AbilityData> abilities;

    private void Awake()
    {
        if (!objectType.HasFlag(ObjectType.Minion))
        {
            objectType += (int)ObjectType.Minion;
        }
    }
}