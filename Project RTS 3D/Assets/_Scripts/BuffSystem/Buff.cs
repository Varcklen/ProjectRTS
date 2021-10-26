using System.Text;
using UnityEngine;

namespace Project.BuffSystem
{
    [System.Serializable]
    public class Buff : IReturnDescriptionTooltip
    {
        public string name;
        public Sprite icon;
        [TextArea] public string description;
        public OutlineColor outlineColor;
        public bool isDispelled;

        [HideInInspector] public BuffData buffData;

        public void SetBuffFromData(BuffData buff)
        {
            Buff so = buff.buff;

            name = so.name;
            icon = so.icon;
            description = so.description;
            outlineColor = so.outlineColor;
            isDispelled = so.isDispelled;
            buffData = buff;
        }

        public Color GetColor(OutlineColor color)
        {
            switch (color)
            {
                case OutlineColor.White: return Color.white;
                case OutlineColor.Green: return Color.green;
                case OutlineColor.Yellow: return Color.yellow;
                case OutlineColor.Red: return Color.red;
            }
            return Color.blue;
        }

        public string GetColorText(OutlineColor color)
        {
            switch (color)
            {
                case OutlineColor.White: return "white";
                case OutlineColor.Green: return "green";
                case OutlineColor.Yellow: return "yellow";
                case OutlineColor.Red: return "red";
            }
            return "blue";
        }

        public string GetTooltipInfoText(DescriptionInfo descriptionInfo)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<size=35>").Append("<color=").Append(GetColorText(outlineColor)).Append(">").Append(name).Append("</color></size>").AppendLine();
            builder.Append(description);

            return builder.ToString();
        }
    }

    public enum OutlineColor : byte
    {
        White,
        Green,
        Yellow,
        Red
    }
}