namespace MarkInDeDark.LevelEditor.Settings
{
    public interface ISettingsRepository
    {
        string GetValue(SettingsKeys key);
        void SetValue(SettingsKeys key, string value);
        bool? GetBoolValue(SettingsKeys key);
        void SetBoolValue(SettingsKeys key, bool value);
    }
}
