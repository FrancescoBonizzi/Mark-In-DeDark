using MarkInDeDark.Common;
using MarkInDeDark.LevelEditor.Settings;
using MarkInDeDarkMono.Common;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace MarkInDeDark.LevelEditor
{
    public partial class GameConfigEditorWindow : Window
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly string _gameConfigFilePath;

        public GameConfigEditorWindow()
        {
            InitializeComponent();

            try
            {
                _settingsRepository = new FileSettingsRepository("configuration.txt");
                var gameExecutablePath = _settingsRepository.GetValue(SettingsKeys.GameExecutablePath);
                if (gameExecutablePath == null)
                {
                    MessageBox.Show("Devi prima selezionare l'eseguibile del gioco!", "Attenzione");
                    Close();
                    return;
                }

                var gameExecutableDirectory = Path.GetDirectoryName(gameExecutablePath);
                _gameConfigFilePath = Path.Combine(gameExecutableDirectory, "Content", "GameConfig.txt");

                if (!File.Exists(_gameConfigFilePath))
                {
                    MessageBox.Show($"Il file di configurazione al path {_gameConfigFilePath} non esiste", "Attenzione");
                    Close();
                    return;
                }

                var gameConfigRaw = File.ReadAllText(_gameConfigFilePath);
                propertyGrid.SelectedObject = new GameConfig(gameConfigRaw);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void btnAnnulla_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnSalva_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var gameConfig = propertyGrid.SelectedObject as GameConfig;
                var serializedGameConfig = GameConfigSerializer.Serialize(gameConfig);
                File.WriteAllText(_gameConfigFilePath, serializedGameConfig);
                MessageBox.Show("Salvataggio completato con successo", "Tutto bene!", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSalvaInAltroFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Text file (*.txt)|*.txt",
                    RestoreDirectory = true
                };

                if (saveFileDialog.ShowDialog() == false)
                    return;

                var gameConfig = propertyGrid.SelectedObject as GameConfig;
                var serializedGameConfig = GameConfigSerializer.Serialize(gameConfig);
                File.WriteAllText(saveFileDialog.FileName, serializedGameConfig);
                MessageBox.Show("Salvataggio completato con successo", "Tutto bene!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
