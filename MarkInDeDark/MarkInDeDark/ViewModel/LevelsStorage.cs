using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mark.Engine;
using System.Windows;
using System.IO.IsolatedStorage;
using System.ComponentModel;

namespace MarkInDeDark.ViewModel
{
    public class LevelsStorage
    {
        public ObservableCollection<Level> LevelsList { get; private set; }
        private IsolatedStorageSettings settings;
        public ObservableSingleObject<int> nSecretsFound { get; set; }

        public LevelsStorage()
        {
            settings = IsolatedStorageSettings.ApplicationSettings;
            LevelsList = new ObservableCollection<Level>();
            nSecretsFound = new ObservableSingleObject<int>();
            nSecretsFound.Value = 0;

#if LITE_VERSION

            AddLiteLevels();

#elif FULL_VERSION

            AddNewLevel("1", 10, 0, 4, 0.2f); // Ok
            AddNewLevel("2", 11, 0, 11, 0.4f); // Ok
            AddNewLevel("3", 12, 0, 10, 0.6f); // Ok 
            AddNewLevel("4", 13, 0, 10, 0.9f); // Ok

            AddNewLevel("5", 14, 1, 5, 0.2f); // Ok
            AddNewLevel("6", 15, 1, 8, 0.4f); // Ok
            AddNewLevel("7", 16, 1, 10, 0.6f); // Ok 
            AddNewLevel("8", 17, 1, 4, 0.9f); // Ok 

            AddNewLevel("9", 18, 2, 8, 0.2f); // Ok
            AddNewLevel("10", 19, 2, 6, 0.4f); // Ok 
            AddNewLevel("11", 20, 2, 15, 0.6f); // Ok
            AddNewLevel("12", 21, 2, 15, 0.9f); // Ok 

            AddNewLevel("13", 22, 3, 15, 0.2f); // Ok
            AddNewLevel("14", 23, 3, 4, 0.4f); // Ok
            AddNewLevel("15", 24, 3, 4, 0.6f); // Ok
            AddNewLevel("16", 25, 3, 4, 0.9f); // Ok

            AddNewLevel("17", 26, 4, 8, 0.2f); // Ok 
            AddNewLevel("18", 27, 4, 12, 0.4f); // Ok 
            AddNewLevel("19", 28, 4, 8, 0.6f); // Ok
            AddNewLevel("20", 29, 4, 10, 0.9f); // Ok

            AddNewLevel("21", 30, 5, 10, 0.2f); // Ok
            AddNewLevel("22", 31, 5, 10, 0.4f); // Ok
            AddNewLevel("23", 32, 5, 10, 0.6f); // Ok
            AddNewLevel("24", 33, 5, 10, 0.9f); // Ok

            AddNewLevel("25", 34, 6, 10, 0.2f); // Ok
            AddNewLevel("26", 35, 6, 8, 0.4f); // Ok
            AddNewLevel("27", 36, 6, 8, 0.6f); // Ok
            AddNewLevel("28", 37, 6, 10, 0.9f); // Ok

            AddNewLevel("29", 38, 6, 15, 0.2f); // Ok
            AddNewLevel("30", 39, 6, 20, 0.4f); // Ok

            // Metto i lite in fondo
            AddLiteLevels();   

#endif

            int bonus_candles = nSecretsFound.Value / 5;

            // Regalo una candela in più per ogni 5 segreti trovati
            foreach (Level L in LevelsList)
                L.StartingCandlesNumber += bonus_candles;
        }

        private void AddLiteLevels()
        {
            AddNewLevel("Lite 1", 0, 0, 4, 0.2f); // Ok
            AddNewLevel("Lite 2", 1, 0, 4, 0.4f); // Ok
            AddNewLevel("Lite 3", 2, 0, 6, 0.6f); // Ok
            AddNewLevel("Lite 4", 3, 0, 9, 0.9f); // OK

            AddNewLevel("Lite 5", 4, 1, 4, 0.2f); // Ok
            AddNewLevel("Lite 6", 5, 1, 4, 0.4f); // Ok 
            AddNewLevel("Lite 7", 6, 1, 5, 0.6f); // Ok 
            AddNewLevel("Lite 8", 7, 1, 4, 0.9f); // Ok 

            AddNewLevel("Lite 9", 8, 2, 10, 0.2f); // Ok
            AddNewLevel("Lite 10", 9, 2, 2, 0.4f); // Ok
        }

        #region Utilities

        private void AddNewLevel(string TextToShow, int LevelIndex, int TexturesIndex, int NCandles, float Prob)
        {
            string LevelName = "Livello" + LevelIndex + ".txt";
            LevelsList.Add(
               new Level(LevelName, TextToShow,
                   GetMemoryCompleted(LevelName),
                   GetSecretTaken(LevelName),
                   Loader.cells_[TexturesIndex],
                   Loader.dirty_[TexturesIndex],
                   Loader.wall_[TexturesIndex],
                   Loader.angle_[TexturesIndex],
                   Loader.macerie_[TexturesIndex],
                   Loader.secrets_.Count > LevelIndex + 1 ? Loader.secrets_[LevelIndex + 1] : Loader.secrets_[Helper.random_.Next(Loader.secrets_.Count)],
                   Prob,
                   NCandles,
                   GetMemoryRecordUsedCandles(LevelName),
                   GetMemoryRecordCompletionTime(LevelName)));
        }

        public void SetCompleted(string Level)
        {
            SetMemoryValue<bool>(Level, "IsCompleted", true);
            LevelsList.Where(l => l.Filename == Level).FirstOrDefault().Completed.Value = true;
        }

        public void SetRecordUsedCandles(string Level, int UsedCandles)
        {
            Level level = LevelsList.Where(l => l.Filename == Level).FirstOrDefault();
            if (UsedCandles < level.RecordUsedCandles.Value || level.RecordUsedCandles.Value == -1)
            {
                level.RecordUsedCandles.Value = UsedCandles;
                SetMemoryValue<int>(Level, "RecordUsedCandles", UsedCandles);
            }
        }

        public void SetRecordCompletionTime(string Level, int CompletionMinutes)
        {
            Level level = LevelsList.Where(l => l.Filename == Level).FirstOrDefault();
            if (CompletionMinutes < level.RecordCompletionMinutes.Value || level.RecordCompletionMinutes.Value == -1)
            {
                level.RecordCompletionMinutes.Value = CompletionMinutes;
                SetMemoryValue<int>(Level, "RecordCompletionTime", CompletionMinutes);
            }
        }

        public void SetSecretTaken(string Level)
        {
            SetMemoryValue<bool>(Level, "SecretTaken", true);
            LevelsList.Where(l => l.Filename == Level).FirstOrDefault().HasSecret.Value = true;
        }

        private bool GetSecretTaken(string Level)
        {
            bool s = GetMemoryValue<bool>(Level, "SecretTaken", false);
            if (s)
            {
                ++nSecretsFound.Value;
            }

            return s;
        }

        private bool GetMemoryCompleted(string Level)
        {
            return GetMemoryValue<bool>(Level, "IsCompleted", false);
        }

        private int GetMemoryRecordUsedCandles(string Level)
        {
            return GetMemoryValue<int>(Level, "RecordUsedCandles", -1);
        }

        // Secondo me al posto di DateTime è meglio TimeSpan
        private int GetMemoryRecordCompletionTime(string Level)
        {
            return GetMemoryValue<int>(Level, "RecordCompletionTime", -1);
        }

        private T GetMemoryValue<T>(string Level, string Key, T DefaultValue)
        {
            string entryName = Level + Key;

            if (!settings.Contains(entryName))
            {
                settings.Add(entryName, DefaultValue);
                settings.Save();
                return DefaultValue;
            }

            return ((T)settings[entryName]);
        }

        private void SetMemoryValue<T>(string Level, string Key, T Value)
        {
            string entryName = Level + Key;

            if (!settings.Contains(entryName))
            {
                settings.Add(entryName, Value);
            }
            else
            {
                settings[entryName] = Value;
            }

            settings.Save();
        }

        #endregion
    }
}
