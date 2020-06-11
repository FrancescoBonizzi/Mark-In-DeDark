namespace MarkInDeDark.LevelEditor.Converters
{
    public static class XnaColorsConverter
    {
        public static System.Windows.Media.Color ToWindowsColor(this Microsoft.Xna.Framework.Color xnaColor)
        {
            return new System.Windows.Media.Color()
            {
                R = xnaColor.R,
                G = xnaColor.G,
                B = xnaColor.B,
                A = xnaColor.A
            };
        }

        public static Microsoft.Xna.Framework.Color ToXnaColor(this System.Windows.Media.Color windowsColor)
        {
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
