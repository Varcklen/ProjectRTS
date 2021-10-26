using Project.AbilitySystem;
using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Project.AbilitySystem
{
    //Basic data for the abilities
    [Serializable]
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ObjectType), true)]
#endif
    public class Ability : IReturnDescriptionTooltip
    {
        [Header("Core")]
        public string name = "New Ability";
        public bool isHide = false;
        [ConditionalHide("isHide", true, true)] public Sprite icon;
        [TextArea] public string description;

        [Header("Parameters")]
        public bool isUpgraded;
        //Range is not work with "ConditionalHide". It needs to be improved.
        [ConditionalHide("isUpgraded", true), Range(1, 100)] public int upgradeLimit = 1;
        [Range(0, 15)] public int buttonPosition;
        [ConditionalHide("isHide", true, true)] public int cost;
        [ConditionalHide("isHide", true, true)] public CostType costType;
        [ConditionalHide("isHide", true, true)] public AbilityTarget abilityTarget;
        [ConditionalHide("isHide", true, true)] public float delay;
        [ConditionalHide("isHide", true, true)] public float cooldown;

        //We need to configure so that, depending on the target type, these variables are displayed or not displayed.
        public ObjectType targetType;//For Target & Area
        public float distance;//For Target & Area
        public float area;//For Area

        public Action<UseParams> actionUse;

        [HideInInspector] public Component component;// { private set; get; }
        [HideInInspector] public AbilityData abilityData;
        [HideInInspector] public bool isPassive = false;
        [HideInInspector] public int level = 1;

        public void SetAbilityFromData(AbilityData ability, UnitInfo caster, AbilityObject child)
        {
            Ability so = ability.ability;

            name = so.name;
            icon = so.icon;
            description = so.description;
            isPassive = so.isPassive;
            isHide = so.isHide;
            abilityTarget = so.abilityTarget;
            distance = so.distance;
            area = so.area;
            cooldown = so.cooldown;
            delay = so.delay;
            targetType = so.targetType;
            if (cost < 0) cost = 0;
            buttonPosition = so.buttonPosition;
            cost = so.cost;
            costType = so.costType;
            isUpgraded = so.isUpgraded;
            if (isUpgraded)
            {
                level = 0;
            }
            else
            {
                level = so.level;
            }
            upgradeLimit = so.upgradeLimit;

            abilityData = ability;
            actionUse = ability.Use;
            AbilityInitialization(ability, caster, child);
        }

        //Debug method
        public void CheckData()
        {
            Debug.Log("name:" + name);
            Debug.Log("icon:" + icon);
            Debug.Log("description:" + description);
            Debug.Log("isHide:" + isHide);
            Debug.Log("isPassive:" + isPassive);
            Debug.Log("actionUse:" + actionUse);
            Debug.Log("abilityTarget:" + abilityTarget);
            Debug.Log("distance:" + distance);
            Debug.Log("area:" + area);
            Debug.Log("cooldown:" + cooldown);
            Debug.Log("delay:" + delay);
            Debug.Log("component:" + component);
        }

        //Returns a description based on the type of cost.
        public string GetCostTypeName()
        {
            switch (costType)
            {
                case CostType.Mana:     return "mana.";
                case CostType.Health:   return "health.";
                case CostType.Gold:     return "gold.";
            }
            return "";
        }

        //Returns a valid description of the second.
        public string GetSecondTime(float cooldown)
        {
            if (cooldown == 1) return " second.";
            return " seconds.";
        }

        //Returns the description of the ability
        public string GetTooltipInfoText(DescriptionInfo descriptionInfo)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<size=35>").Append(name);
            if (isUpgraded)
            {
                builder.Append(" [").Append(level).Append("]");
            }
            builder.Append("</size>").AppendLine();
            if (descriptionInfo.key != KeyCode.None)
            {
                builder.Append("<color=red>Use:</color> ").Append(ColorText.KeyCodeToString(descriptionInfo.key)).AppendLine();
            }
            builder.Append(ReplaceTextInTooltip(description)).AppendLine();
            if (cooldown > 0)
                builder.Append("<color=yellow>Cooldown:</color> ").Append(cooldown).Append(GetSecondTime(cooldown)).AppendLine();
            if (cost > 0)
                builder.Append("<color=blue>Cost:</color> ").Append(cost).Append(" ").Append(GetCostTypeName());

            return builder.ToString();
        }

        //Starts the initialization of the ability
        public void AbilityInitialization(AbilityData ability, ObjectInfo caster, AbilityObject child)
        {
            if (caster != null)
            {
                ability.Init(caster, child);
            }
        }

        public string ReplaceTextInTooltip(string description)
        {
            //Debug.Log("ability: " + name);
            //Debug.Log("component: " + component);
            if (component == null)
            {
                return description;
            }
            if (component is IReplaceTextInAbilityTooltip myInterface)
            {
                return myInterface.ReplaceTextForAbility(description);
            }
            else
            {
                return description;
            }
        }
    }

    //Allows you to dynamically change descriptions in abilities.
    public interface IReplaceTextInAbilityTooltip
    {
        string ReplaceTextForAbility(string description);
    }

    public enum AbilityTarget : byte
    {
        NoTarget,
        Target,
        Area
    }

    public enum CostType : byte
    {
        Mana,
        Health,
        Gold
    }
}
