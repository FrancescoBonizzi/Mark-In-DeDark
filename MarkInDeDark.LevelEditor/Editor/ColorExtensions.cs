using System.Windows.Media;

namespace MarkInDeDark.LevelEditor.Editor
{
    public static class ColorExtensions
    {
        public static Color Lighter(this Color color)
        {
            if (color == Colors.Black)
            {
                return Colors.LightGray;
            }

            return Color.FromRgb(
                (byte)(color.R * 0.8),
                (byte)(color.G * 0.8),
                (byte)(color.B * 0.8));
        }
    }
}
