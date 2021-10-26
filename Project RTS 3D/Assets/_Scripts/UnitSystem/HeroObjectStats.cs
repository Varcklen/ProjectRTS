using UnityEngine;

[System.Serializable]
public class HeroObjectStats : HeroStats
{
    [field: SerializeField, ConditionalHide("unique", false)]
    public int Level { get; private set; } = 1;
    [field: SerializeField, ConditionalHide("unique", false)]
    public int UpgradePoints { get; private set; } = 1;

    private const int LEVEL_MIN_LIMIT = 1;
    private const int LEVEL_MAX_LIMIT = 100;

    public void AddLevel(int newLevel)
    {
        int oldLevel = Level;
        Level += newLevel;
        Level = Mathf.Clamp(Level, LEVEL_MIN_LIMIT, LEVEL_MAX_LIMIT);
        //If the level increases, it gives points to improve abilities.
        if (newLevel > 0)
        {
            int UpgradePoints = 0;
            if (oldLevel + newLevel > LEVEL_MAX_LIMIT)
            {
                UpgradePoints = (LEVEL_MAX_LIMIT - oldLevel);
            }
            else
            {
                UpgradePoints = newLevel;
            }

            if (UpgradePoints > 0)
            {
                this.UpgradePoints += UpgradePoints;
            }
        }
        UI_Manager.Instance.RefreshInfoPanel();
    }

    public void ReduceAbilityUpgradePoint()
    {
        UpgradePoints--;
    }
}
