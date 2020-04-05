using MarkInDeDark.Common;
using MarkInDeDark.LevelEditor.Domain;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace MarkInDeDark.LevelEditor
{
    public partial class TraslationsWindow : Window
    {
        private readonly TextFileStringsManager _traslationManager;

        public Dictionary<StringsTraslationLanguages, Dictionary<string, TraslatedStringValue>> TraslatedStrings { get; }

        public TraslationsWindow(TextFileStringsManager traslationManager)
        {
            _traslationManager = traslationManager ?? throw new ArgumentNullException(nameof(traslationManager));
            TraslatedStrings = _traslationManager.GetAllTranslations().Traslations;

            InitializeComponent();
            DataContext = this;

            tabControl.SelectedIndex = 0;
        }

        private void btnSalvaTutto_Click(object sender, RoutedEventArgs e)
        {
            // Per committare nella griglia il valore
            FocusManager.SetFocusedElement(this, null);
            Keyboard.ClearFocus();

            try
            {
                _traslationManager.Save();
                MessageBox.Show("Salvataggio completato con successo", "Tutto bene!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void btnSalvaAltroFile_Click(object sender, RoutedEventArgs e)
        {
            // Per committare nella griglia il valore
            FocusManager.SetFocusedElement(this, null);
            Keyboard.ClearFocus();

            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Json file (*.json)|*.json",
                    RestoreDirectory = true
                };

                if (saveFileDialog.ShowDialog() == false)
                    return;

                _traslationManager.Save(saveFileDialog.FileName);
                MessageBox.Show("Salvataggio completato con successo", "Tutto bene!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
