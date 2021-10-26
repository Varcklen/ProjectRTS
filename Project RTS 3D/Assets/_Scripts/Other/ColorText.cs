using Project.AbilitySystem;
using UnityEngine;

//Use these methods to color-match text.
public static class ColorText
{
    static private Color SpellPowerColor = Color.magenta;
    static private Color LuckColor = Color.yellow;

    //Color the text in the desired color.
    public static string ColorString(string text, Color color)
    {
        return "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">" + text + "</color>";
    }

    //Sets the color depending on the selected preset
    public static string ColorStringFromPreset(string text, ColorPreset colorPreset)
    {
        switch (colorPreset)
        {
            case ColorPreset.Luck:          return ColorString(text, LuckColor);
            case ColorPreset.SpellPower:    return ColorString(text, SpellPowerColor);
        }
        return text;
    }

    //Sets the color depending on the selected preset and changes the number
    public static string ColorStringAndChangeNumber(UnitInfo caster, float number, ColorPreset colorPreset)
    {
        switch (colorPreset)
        {
            case ColorPreset.Luck:          return ColorString(Mathf.FloorToInt(number + (caster?.Stats?.LuckPercent ?? 0)).ToString(), LuckColor);
            case ColorPreset.SpellPower:    return ColorString(Mathf.FloorToInt(number * ((caster?.Stats?.SpellPowerPoint ?? 0)+1)).ToString(), SpellPowerColor);
        }
        return number.ToString();
    }

    //Converts KeyCode to string. Converts Alpha00-09 to numbers.
    public static string KeyCodeToString(KeyCode key)
    {
        for (int i = 0; i < 10; i++)
        {
            if (key == (KeyCode)(48 + i))
            {
                return i.ToString();
            }
        }
        return key.ToString();
    }

    //Returns text in brackets to upgrade the ability
    public static string AddAbilityUpgradeText(AbilityObject ability, string text)
    {
        if (ButtonManager.Instance.isUpgradeMode && ability.upgradeLimit > ability.level && ability.level != 0)
        {
            return " (" + text + ")";
        }
        else
        {
            return "";
        }
    }
    public static string AddAbilityUpgradeText(AbilityObject ability, float text)
    {
        if (ButtonManager.Instance.isUpgradeMode && ability.upgradeLimit > ability.level && ability.level != 0)
        {
            string symbol = "";
            if (text > 0)
            {
                symbol = "+";
            }
            else if (text < 0)
            {
                symbol = "-";
            }
            return " (" + symbol + text + ")";
        }
        else
        {
            return "";
        }
    }
    public static string AddAbilityUpgradeText(AbilityObject ability, int text)
    {
        if (ButtonManager.Instance.isUpgradeMode && ability.upgradeLimit > ability.level && ability.level != 0)
        {
            string symbol = "";
            if (text > 0)
            {
                symbol = "+";
            }
            else if (text < 0)
            {
                symbol = "-";
            }
            return " (" + symbol + text + ")";
        }
        else
        {
            return "";
        }
    }
}

public enum ColorPreset : byte
{
    None,
    SpellPower,
    Luck
}
