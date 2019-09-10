using FbonizziMonoGameWindowsDesktop;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MarkInDeDarkMono.Windows
{
    public static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            const int width = 1422;
            const int height = 800;

            using (var game = new MarkInDeDarkBootstrap(
                new WindowsTextFileImporter(),
                new TransientWindowsSettingsRepository(),
                CultureInfo.CreateSpecificCulture("en-US"),
                true,
                Directory.GetFiles("Content/Levels", "*.mark").ToList(),
                width,
                height))
            {
                if (args.Length > 0)
                {
                    if (!File.Exists(args[0]))
                        throw new ArgumentException($"Il livello al path: {args[0]} non esiste!");
                    
                    game.ForceStartLevel(args[0]);
                }

                game.Run();
            }
        }
    }
}
