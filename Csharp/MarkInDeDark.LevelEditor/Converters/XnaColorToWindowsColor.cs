using System;
using System.Globalization;
using System.Windows.Data;

namespace MarkInDeDark.LevelEditor.Converters
{
    public class XnaColorToWindowsColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var xnaColor = (Microsoft.Xna.Framework.Color)value;
            return xnaColor.ToWindowsColor();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var windowsColor = (System.Windows.Media.Color)value;
            return windowsColor.ToXnaColor();
        }
    }
}
