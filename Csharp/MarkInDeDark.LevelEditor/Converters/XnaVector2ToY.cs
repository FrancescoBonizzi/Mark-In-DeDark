using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MarkInDeDark.LevelEditor.Converters
{
    public class XnaVector2ToY : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Vector2)value).Y;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToSingle(value);
        }
    }
}
