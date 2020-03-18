using FbonizziMonoGameWindowsDesktop;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MarkInDeDarkMono.Windows
{
    public static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            const int width = 1422;
            const int height = 800;

            try
            {
                string forcedLevelPath = null;
                if (args.Length > 0)
                {
                    if (!File.Exists(args[0]))
                        throw new ArgumentException($"Il livello al path: {args[0]} non esiste!");

                    forcedLevelPath = args[0];
                }

                List<string> levelsToLoad = forcedLevelPath == null 
                    ? Directory.GetFiles("Content/Levels", "*.mark").ToList()
                    : new List<string>() { forcedLevelPath };

                using (var game = new MarkInDeDarkBootstrap(
                    textFileAssetsLoader: new WindowsTextFileImporter(),
                    settingsRepository: new TransientWindowsSettingsRepository(),
                    gameCulture: CultureInfo.CurrentCulture,
                    isPc: true,
                    levelPaths: levelsToLoad,
                    windowWidth: width,
                    windowHeight: height))
                {
                    if (forcedLevelPath != null)
                    {
                        game.ForceStartLevel(args[0]);
                    }

                    game.Run();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }
    }
}
