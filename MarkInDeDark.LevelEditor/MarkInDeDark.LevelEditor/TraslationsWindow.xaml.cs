using MarkInDeDark.Common;
using MarkInDeDark.LevelEditor.Domain;
using MarkInDeDark.LevelEditor.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MarkInDeDark.LevelEditor
{
    public partial class TraslationsWindow : Window
    {
        private readonly TextFileStringsManager _traslationManager;
        private readonly ISettingsRepository _settingsRepository;

        public Dictionary<StringsTraslationLanguages, Dictionary<string, TraslatedStringValue>> TraslatedStrings { get; }

        public TraslationsWindow()
        {
            InitializeComponent();
            DataContext = this;

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
                var traslationsFilePath = Path.Combine(gameExecutableDirectory, "Content", "Translations.txt");
                _traslationManager = new TextFileStringsManager(traslationsFilePath);

                // Devo fare tutto questo giro dell'oca perché non si riesce a mettere in binding direttamente dictionary string
                TraslatedStrings = new Dictionary<StringsTraslationLanguages, Dictionary<string, TraslatedStringValue>>();
                foreach (var language in _traslationManager.GetAllTranslations().Traslations)
                {
                    var newTraslation = new Dictionary<string, TraslatedStringValue>();
                    foreach (var traslation in language.Value)
                        newTraslation.Add(traslation.Key, new TraslatedStringValue() { Text = traslation.Value });
                    TraslatedStrings.Add(language.Key, newTraslation);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

            tabControl.SelectedIndex = 0;
        }

        private void btnSalvaTutto_Click(object sender, RoutedEventArgs e)
        {
            // Per committare nella griglia il valore
            FocusManager.SetFocusedElement(this, null);
            Keyboard.ClearFocus();

            // Devo fare tutto questo giro dell'oca perché non si riesce a mettere in binding direttamente dictionary string
            var newTranslationData = new Dictionary<StringsTraslationLanguages, Dictionary<string, string>>();
            foreach(var language in TraslatedStrings)
            {
                var languageData = new Dictionary<string, string>();
                foreach(var traslation in language.Value)
                    languageData.Add(traslation.Key, traslation.Value.Text);
                newTranslationData.Add(language.Key, languageData);
            }

            _traslationManager.SetTranslations(newTranslationData);
            _traslationManager.Save();
            MessageBox.Show("Salvataggio completato con successo", "Tutto bene!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show(
                 messageBoxText: "Se prosegui senza aver salvato perderai le modifiche fatte alle traduzioni. Proseguire?",
                 caption: "Proseguire?",
                 button: MessageBoxButton.YesNo, icon: MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
