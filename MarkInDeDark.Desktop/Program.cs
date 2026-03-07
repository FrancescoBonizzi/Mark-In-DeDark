using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using FbonizziMonoGame.Implementations;
using MarkInDeDark.Library;

namespace MarkInDeDark.Desktop
{
    public static class Program
    {
        private static MarkInDeDarkBootstrap _game;

        private static void Main(string[] args)
        {
            const int width = 1422;
            const int height = 800;

            string levelSettingsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MarkInDeDark"
            );

            if (!Directory.Exists(levelSettingsDirectory))
            {
                Directory.CreateDirectory(levelSettingsDirectory);
            }

            string levelSettingsPath = Path.Combine(levelSettingsDirectory, "settings.txt");

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

                bool normalMapParametersRegulation = args.Length > 1;

                List<string> levelsToLoad =
                    forcedLevelPath == null
                        ? Directory.GetFiles("Assets/Levels", "*.mark").ToList()
                        : new List<string>() { forcedLevelPath };

                using (
                    _game = new MarkInDeDarkBootstrap(
                        textFileAssetsLoader: new WindowsTextFileImporter(),
                        settingsRepository: new FileWindowsSettingsRepository(levelSettingsPath),
                        gameCulture: CultureInfo.CurrentCulture,
                        levelPaths: levelsToLoad,
                        windowWidth: width,
                        windowHeight: height
                    )
                )
                {
                    _game.TargetElapsedTime = TimeSpan.FromTicks(
                        (long)(TimeSpan.TicksPerSecond * (1d / 60d))
                    );
                    _game.IsFixedTimeStep = true;
                    _game.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
                    _game.GraphicsDeviceManager.GraphicsProfile = Microsoft
                        .Xna
                        .Framework
                        .Graphics
                        .GraphicsProfile
                        .Reach;
                    _game.GraphicsDeviceManager.ApplyChanges();

                    if (forcedLevelPath != null)
                    {
                        _game.ForceStartLevel(args[0]);
                    }

                    _game.NormalMapRegulationMode = normalMapParametersRegulation;

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
                Console.Error.WriteLine(ex.ToString());
                throw;
            }
        }

        private static void Game_Activated(object sender, EventArgs e) => _game.Resume();

        private static void Game_Deactivated(object sender, EventArgs e) => _game.Pause();
    }
}
