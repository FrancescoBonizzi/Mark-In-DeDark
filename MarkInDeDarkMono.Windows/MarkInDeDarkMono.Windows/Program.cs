using FlowsoftGamesWindows;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MarkInDeDarkMono.Windows
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            int width = 800;
            int height = 480;

            using (var game = new MarkInDeDarkBootstrap(
                new WindowsTextFileImporter(),
                new TransientWindowsSettingsRepository(),
                CultureInfo.CreateSpecificCulture("en-US"),
                true,
                Directory.GetFiles("Content/Levels", "*.txt").Select(f => Path.GetFileName(f))))
            {
                game.SetGraphicsDeviceViewportSize(width, height);
                game.Run();
            }
        }
    }
}
