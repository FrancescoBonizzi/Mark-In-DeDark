using FbonizziMonoGameWindowsDesktop;
using MarkInDeDark.Library;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MarkInDeDark.Windows
{
    public static class Program
    {
        private static MarkInDeDarkBootstrap _game;

        [STAThread]
        private static void Main(string[] args)
        {
            const int width = 1422;
            const int height = 800;

            string levelSettingsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MarkInDeDark");

            if (!Directory.Exists(levelSettingsDirectory))
            {
                Directory.CreateDirectory(levelSettingsDirectory);
            }

            string levelSettingsPath = Path.Combine(
                levelSettingsDirectory,
                "settings.txt");

            try
            {
                string forcedLevelPath = null;
                if (args.Length > 0)
                {
                    if (!File.Exists(args[0]))
                    {
                        throw new ArgumentException($"Il livello al path: {args[0]} non esiste!");
                    }

                    forcedLevelPath = args[0];
                }

                List<string> levelsToLoad = forcedLevelPath == null
                    ? Directory.GetFiles("Assets/Levels", "*.mark").ToList()
                    : new List<string>() { forcedLevelPath };

                using (_game = new MarkInDeDarkBootstrap(
                    textFileAssetsLoader: new WindowsTextFileImporter(),
                    settingsRepository: new FileWindowsSettingsRepository(levelSettingsPath),
                    gameCulture: CultureInfo.CurrentCulture,
                    levelPaths: levelsToLoad,
                    windowWidth: width,
                    windowHeight: height))
                {
                    if (forcedLevelPath != null)
                    {
                        _game.ForceStartLevel(args[0]);
                    }

                    _game.Window.Title = "Mark in DeDark";
                    _game.Window.AllowUserResizing = false;
                    _game.Deactivated += Game_Deactivated;
                    _game.Activated += Game_Activated;
                    _game.Run();
                    _game.Deactivated -= Game_Deactivated;
                    _game.Activated -= Game_Activated;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private static void Game_Activated(object sender, EventArgs e)
            => _game.Resume();

        private static void Game_Deactivated(object sender, EventArgs e)
            => _game.Pause();
    }
}
