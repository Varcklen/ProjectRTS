using System;
using UnityEngine;

[Serializable]
public class HeroStats
{
    public Action<int> OnStrengthChange;
    public Action<int> OnAgilityChange;
    public Action<int> OnIntelligenceChange;

    //These accessors are needed to update UI elements.
    [SerializeField]
    private int strength;
    public int Strength
    {
        get { return strength; }
        set
        {
            strength = value;
            OnStrengthChange?.Invoke(value);
            UI_Manager.Instance?.RefreshInfoPanel();
        }
    }

    [SerializeField]
    private int agility;
    public int Agility
    {
        get { return agility; }
        set
        {
            agility = value;
            OnAgilityChange?.Invoke(value);
            UI_Manager.Instance?.RefreshInfoPanel();
        }
    }

    [SerializeField]
    private int intelligence;
    public int Intelligence
    {
        get { return intelligence; }
        set
        {
            intelligence = value;
            OnIntelligenceChange?.Invoke(value);
            UI_Manager.Instance?.RefreshInfoPanel();
        }
    }

    public void SetStatsFromData(HeroData heroData)
    {
        HeroStats stats = heroData.heroStats;

        Strength = stats.Strength;
        Agility = stats.Agility;
        Intelligence = stats.Intelligence;
    }
}
