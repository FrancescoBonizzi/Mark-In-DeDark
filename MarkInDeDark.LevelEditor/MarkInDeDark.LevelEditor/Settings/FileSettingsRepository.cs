using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MarkInDeDark.LevelEditor.Settings
{
    public class FileSettingsRepository : ISettingsRepository
    {
        private readonly Dictionary<SettingsKeys, string> _settings;
        private readonly string _settingsFilePath;
        private const string _directoryName = "Mark in DeDark - Level editor";

        public FileSettingsRepository(string settingsFileName)
        {
            string settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _directoryName);
            if (!Directory.Exists(settingsDirectory))
                Directory.CreateDirectory(settingsDirectory);
            _settingsFilePath = Path.Combine(settingsDirectory, settingsFileName);

            if (!File.Exists(_settingsFilePath))
                File.WriteAllText(_settingsFilePath, string.Empty);

            string settingsRaw = File.ReadAllText(_settingsFilePath);

            _settings = new Dictionary<SettingsKeys, string>();

            if (!string.IsNullOrWhiteSpace(settingsRaw))
            {
                var settingsRows = settingsRaw.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var settingRow in settingsRows)
                {
                    if (string.IsNullOrWhiteSpace(settingRow))
                        continue;

                    string[] splittedSetting = settingRow.Split(':');
                    _settings.Add(
                        (SettingsKeys)Enum.Parse(typeof(SettingsKeys), splittedSetting[0]),
                        settingRow.Substring(splittedSetting[0].Length + 1)); // Perchè se mi baso sullo split non considero le stringhe che hanno i due punti!
                }
            }
        }

        public string GetValue(SettingsKeys key)
        {
            if (!_settings.ContainsKey(key))
                return null;

            return _settings[key];
        }

        public void SetValue(SettingsKeys key, string value)
        {
            _settings[key] = value;

            string settingsContent = string.Join(Environment.NewLine, _settings.Select(s => $"{s.Key}:{s.Value}"));

            File.WriteAllText(
                _settingsFilePath,
                settingsContent);
        }

        public bool? GetBoolValue(SettingsKeys key)
        {
            var value = GetValue(key);

            if (value != null)
            {
                return value == "true";
            }

            return null;
        }

        public void SetBoolValue(SettingsKeys key, bool value)
        {
            SetValue(key, value ? "true" : "false");
        }
    }
}
