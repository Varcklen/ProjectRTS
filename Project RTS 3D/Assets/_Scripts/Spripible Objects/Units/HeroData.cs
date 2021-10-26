using UnityEngine;

//ScriptableObject for Unit
[CreateAssetMenu(fileName = "New Hero", menuName = "Custom/Unit/Hero")]
public class HeroData : Unit
{
    [Header("Hero Stats")]
    public HeroStats heroStats;

    private void Awake()
    {
        if (objectType.HasFlag(ObjectType.Minion))
        {
            objectType -= (int)ObjectType.Minion;
        }
        objectType += (int)ObjectType.Hero;
    }
}
