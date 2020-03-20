using MarkInDeDarkMono.Common;
using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace MarkInDeDark.Common
{
    public static class GameConfigSerializer
    {
        public static string Serialize(GameConfig gameConfig)
        {
            var stringBuilder = new StringBuilder();

            foreach (var p in typeof(GameConfig).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (p.PropertyType == typeof(float))
                {
                    var value = ((float)p.GetValue(gameConfig)).ToString(CultureInfo.InvariantCulture);
                    stringBuilder.AppendLine($"{p.Name}:{value}");
                }
                else if (p.PropertyType == typeof(int))
                {
                    var value = (int)p.GetValue(gameConfig);
                    stringBuilder.AppendLine($"{p.Name}:{value}");
                }
                else if (p.PropertyType == typeof(ClassVector2))
                {
                    var value = ((ClassVector2)p.GetValue(gameConfig));
                    var valueX = value.X.ToString(CultureInfo.InvariantCulture);
                    var valueY = value.Y.ToString(CultureInfo.InvariantCulture);
                    stringBuilder.AppendLine($"{p.Name}:{valueX}-{valueY}");
                }
                else if (p.PropertyType == typeof(Color))
                {
                    var value = ((Color)p.GetValue(gameConfig));
                    stringBuilder.AppendLine($"{p.Name}:{value.R}-{value.G}-{value.B}");
                }
                else if (p.PropertyType == typeof(bool))
                {
                    var value = (bool)p.GetValue(gameConfig) ? 1 : 0;
                    stringBuilder.AppendLine($"{p.Name}:{value}");
                }
                else
                {
                    throw new NotImplementedException($"Tipo {p.PropertyType.Name} non gestito nella serializzazione");
                }
            }

            return stringBuilder.ToString();
        }
    }
}
