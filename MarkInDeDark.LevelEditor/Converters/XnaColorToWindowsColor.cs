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
            return new System.Windows.Media.Color()
            {
                R = xnaColor.R,
                G = xnaColor.G,
                B = xnaColor.B,
                A = xnaColor.A
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var windowsColor = (System.Windows.Media.Color)value;
            return new Microsoft.Xna.Framework.Color()
            {
                R = windowsColor.R,
                G = windowsColor.G,
                B = windowsColor.B,
                A = windowsColor.A
            };
        }
    }
}
