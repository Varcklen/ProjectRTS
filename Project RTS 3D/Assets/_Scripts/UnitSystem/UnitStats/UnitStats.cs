using UnityEngine;

[System.Serializable]
public class UnitStats : Stats
{
    private float health;
    public float Health
    {
        get
        {
            return health;
        }
        set
        {
            health = Mathf.Clamp(value, 0, Mathf.Infinity);
        }
    }

    private float mana;
    public float Mana
    {
        get
        {
            return mana;
        }
        set
        {
            mana = Mathf.Clamp(value, 0, Mathf.Infinity);
        }
    }

    private HeroInfo heroInfo;

    private int strengthChange;
    private int agilityChange;
    private int intelligenceChange;

    private const int MAX_HEALTH_PER_STRENGTH = 25;
    private const float HEALTH_REGENERATION_PER_STRENGTH = 0.02f;

    private const float ARMOR_PER_AGILITY = 0.1f;
    private const float DAMAGE_PER_AGILITY = 1;

    private const float MAX_MANA_PER_INTELLIGENCE = 10;
    private const float SPELLPOWER_PER_INTELLIGENCE = 0.75f;

    public void SetUnitStatsFromStats(Stats stats, UnitInfo unit)
    {
        Health = stats.MaxHealth;
        Mana = stats.maxMana;

        if (heroInfo == null)
        {
            if (unit is HeroInfo heroInfo)
            {
                this.heroInfo = heroInfo;
                HeroObjectStats heroStats = heroInfo.HeroStats;
                heroStats.OnStrengthChange += StrengthChange;
                heroStats.OnAgilityChange += AgilityChange;
                heroStats.OnIntelligenceChange += IntelligenceChange;
            }
        }
    }

    public void OnDestroy()
    {
        if (heroInfo != null)
        {
            HeroObjectStats heroStats = heroInfo.HeroStats;
            heroStats.OnStrengthChange -= StrengthChange;
            heroStats.OnAgilityChange -= AgilityChange;
            heroStats.OnIntelligenceChange -= IntelligenceChange;
        }
    }

    private void StrengthChange(int strength)
    {
        int difference = strength - strengthChange;
        if (difference == 0)
        {
            return;
        }
        MaxHealth += difference * MAX_HEALTH_PER_STRENGTH;
        if (difference > 0)
        {
            Health += difference * MAX_HEALTH_PER_STRENGTH;
        }
        healthRegeneration += difference * HEALTH_REGENERATION_PER_STRENGTH;
        strengthChange = strength;
    }

    private void AgilityChange(int agility)
    {
        int difference = agility - agilityChange;
        if (difference == 0)
        {
            return;
        }
        Armor += difference * ARMOR_PER_AGILITY;
        attack.Damage += difference * DAMAGE_PER_AGILITY;

        agilityChange = agility;
    }

    private void IntelligenceChange(int intelligence)
    {
        int difference = intelligence - intelligenceChange;
        if (difference == 0)
        {
            return;
        }
        maxMana += difference * MAX_MANA_PER_INTELLIGENCE;
        if (difference > 0)
        {
            Mana += difference * MAX_MANA_PER_INTELLIGENCE;
        }
        SpellPower += difference * SPELLPOWER_PER_INTELLIGENCE;
        intelligenceChange = intelligence;
    }
}
