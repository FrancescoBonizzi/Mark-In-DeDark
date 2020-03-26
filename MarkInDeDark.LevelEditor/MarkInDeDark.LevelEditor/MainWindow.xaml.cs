using MarkInDeDark.GameGrid;
using MarkInDeDark.LevelEditor.Domain;
using MarkInDeDark.LevelEditor.Editor;
using MarkInDeDark.LevelEditor.Settings;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace MarkInDeDark.LevelEditor
{
    public partial class MainWindow : Window
    {
        private const string _baseWindowTitle = "Mark in DeDark Level editor";

        private GameGridBundle _gameGrid;
        private WpfCanvasEditorPresentation _presentation;
        private BusyIndicator _busyIndicator;

        private EditorActions _currentEditorAction;
        private GroundTypes _currentSelectedCellType;
        private WallTypes _currentSelectedWallType;
        private ObjectTypes _currentSelectedObjectType;

        private const int _defaultGridRows = 25;
        private const int _defaultGridCols = 45;
        private const int _cellSize = 30;

        private string _currentLevelFilePath;

        private readonly ISettingsRepository _settingsRepository;

        public MainWindow()
        {
            InitializeComponent();
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowState = WindowState.Maximized;

            _settingsRepository = new FileSettingsRepository("configuration.txt");

            NewGrid();
            btnBuio.IsChecked = true;
        }

        private void NewGrid()
        {
            _gameGrid = new GameGridBundle(_defaultGridRows, _defaultGridCols);
            _presentation = new WpfCanvasEditorPresentation(
                this,
                _gameGrid,
                levelEditorCanvas,
                _cellSize);
            Title = $"{_baseWindowTitle} - Nuovo";
        }

        private void BtnNuovo_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                messageBoxText: "Se prosegui senza aver salvato perderai le modifiche fatte al file corrente. Proseguire?",
                caption: "Proseguire?",
                button: MessageBoxButton.YesNo, icon: MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                NewGrid();
            }
        }

        private void StartWaiting()
        {
            Opacity = 0.5;
            Effect = new BlurEffect();
            _busyIndicator = new BusyIndicator();
            _busyIndicator.Show();
        }

        private void StopWaiting()
        {
            Opacity = 1;
            Effect = null;
            _busyIndicator.Close();
            _busyIndicator = null;
        }

        private void BtnApri_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                  messageBoxText: "Se prosegui senza aver salvato perderai le modifiche fatte al file corrente. Proseguire?",
                  caption: "Proseguire?",
                  button: MessageBoxButton.YesNo, icon: MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Mark in DeDark files (*.mark)|*.mark",
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == false)
                return;

            StartWaiting();

            Dispatcher.Invoke(() =>
            {
                try
                {
                    var levelContent = File.ReadAllText(openFileDialog.FileName);
                    var gameGridSerializerManager = new GameGridDeserializerManager(levelContent);
                    _gameGrid = gameGridSerializerManager.DeserializeLevel();

                    numDirtyProbability.Value = _gameGrid.DirtyProbabilityPercentage;
                    cmbTexturePack.SelectedIndex = _gameGrid.EnvironmentTexturePackId;

                    _presentation = new WpfCanvasEditorPresentation(
                        this,
                        _gameGrid,
                        levelEditorCanvas,
                        _cellSize);

                    Title = $"{_baseWindowTitle} - {Path.GetFileNameWithoutExtension(openFileDialog.FileName)}";
                }
                finally
                {
                    StopWaiting();
                }
            }, DispatcherPriority.Background);

            _currentLevelFilePath = openFileDialog.FileName;
        }

        private void BtnSalva_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentLevelFilePath))
            {
                BtnSalvaCopia_Click(sender, e);
                return;
            }

            SerializeAndSave(_currentLevelFilePath);
            MessageBox.Show("Salvataggio completato con successo", "Tutto bene!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnSalvaCopia_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Mark in DeDark files (*.mark)|*.mark",
                RestoreDirectory = true
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                SerializeAndSave(saveFileDialog.FileName);
                _currentLevelFilePath = saveFileDialog.FileName;
                Title = $"{_baseWindowTitle} - {Path.GetFileNameWithoutExtension(saveFileDialog.FileName)}";
                MessageBox.Show("Salvataggio completato con successo", "Tutto bene!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SerializeAndSave(string filePath)
        {
            _gameGrid.DirtyProbabilityPercentage = numDirtyProbability.Value ?? 0;
            _gameGrid.EnvironmentTexturePackId = cmbTexturePack.SelectedIndex;
            var serializedLevel = new GameGridSerializerV3().SerializeLevel(_gameGrid);
            File.WriteAllText(filePath, serializedLevel);
        }

        private void BtnGroundType_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as RadioButton;
            _currentSelectedCellType = (GroundTypes)Convert.ToInt32(button.Tag);
            _currentEditorAction = EditorActions.Ground;
            _presentation.ClearHover();
        }

        private void BtnWallType_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as RadioButton;
            _currentSelectedWallType = (WallTypes)Convert.ToInt32(button.Tag);
            _currentEditorAction = EditorActions.Wall;
            _presentation.ClearHover();
        }

        private void BtnObjectType_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as RadioButton;
            _currentSelectedObjectType = (ObjectTypes)Convert.ToInt32(button.Tag);
            _currentEditorAction = EditorActions.Object;
            _presentation.ClearHover();
        }

        private void LevelEditorCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                LevelEditorCanvas_MouseMove(sender, e);
            }
        }

        private void LevelEditorCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                switch (_currentEditorAction)
                {
                    case EditorActions.Ground:
                        _presentation.SetGroundType(e.GetPosition(levelEditorCanvas), _currentSelectedCellType);
                        break;

                    case EditorActions.Object:

                        var mousePosition = e.GetPosition(levelEditorCanvas);

                        string quantityInfo = null;
                        if (_currentSelectedObjectType == ObjectTypes.Candele)
                        {
                            var inputDialog = new InputDialog("Inserimento nuovo oggetto", "Quante candele deve avere il pacco di candele?");
                            if (inputDialog.ShowDialog() == false)
                            {
                                return;
                            }

                            if (!int.TryParse(inputDialog.InputString, out int inputNumber))
                            {
                                MessageBox.Show(
                                    $"'{inputDialog.InputString}' non è un numero :-)",
                                    "Erroraccio!",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            }

                            quantityInfo = inputNumber.ToString();
                        }
                        else if (_currentSelectedObjectType == ObjectTypes.Zombie
                            || _currentSelectedObjectType == ObjectTypes.Scheletro
                            || _currentSelectedObjectType == ObjectTypes.Taurus
                            || _currentSelectedObjectType == ObjectTypes.Ghost)
                        {
                            string pathTemplate = $"{_currentSelectedObjectType.ToString().ToUpper()[0]}-*-*";

                            var inputDialog = new InputDialog(
                                $"Inserimento nuovo {_currentSelectedObjectType.ToString()}",
                                "Inserisci la stringa per identificare il suo path:",
                                pathTemplate);
                            if (inputDialog.ShowDialog() == false)
                            {
                                return;
                            }

                            quantityInfo = inputDialog.InputString;
                        }

                        _presentation.SetObjectType(mousePosition, _currentSelectedObjectType, quantityInfo);

                        // Gli oggetti che possono esistere solo in una quantità disattivano il pulsante di inserimento 
                        // una volta inserto l'oggetto
                        if (_currentSelectedObjectType == ObjectTypes.Chiave
                            || _currentSelectedObjectType == ObjectTypes.Giocatore
                            || _currentSelectedObjectType == ObjectTypes.Uscita
                            || _currentSelectedObjectType == ObjectTypes.Goblin)
                        {
                            _currentEditorAction = EditorActions.Ground;
                            _currentSelectedCellType = GroundTypes.Terra;
                            BtnGroundType_Ground.IsChecked = true;
                        }

                        break;
                }
            }
            else if (_currentEditorAction == EditorActions.Wall)
            {
                // Effetto hover
                _presentation.SetHover(e.GetPosition(levelEditorCanvas));
            }
        }

        private void LevelEditorCanvas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_currentEditorAction == EditorActions.Wall)
            {
                if (e.Key == Key.Up)
                {
                    _presentation.SetWallOfCurrentHoveredCellType(_currentSelectedWallType, WallPositions.Top);
                }
                else if (e.Key == Key.Right)
                {
                    _presentation.SetWallOfCurrentHoveredCellType(_currentSelectedWallType, WallPositions.Right);
                }
                else if (e.Key == Key.Down)
                {
                    _presentation.SetWallOfCurrentHoveredCellType(_currentSelectedWallType, WallPositions.Bottom);
                }
                else if (e.Key == Key.Left)
                {
                    _presentation.SetWallOfCurrentHoveredCellType(_currentSelectedWallType, WallPositions.Left);
                }
            }
        }

        private void HelpWalls_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Seleziona un tipo di muro, posizionati col mouse su una cella, poi utilizzare i tasti freccia della tastiera per posizionare o togliere un muro", "Aiutino!");
        }

        private void HelpGround_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Seleziona un tipo di terreno e premi col mouse su una cella per impostarlo", "Aiutino!");
        }

        private void HelpObjects_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Seleziona un oggetto e premi col mouse su una cella per impostarlo", "Aiutino!");
        }

        private void HelpEnemies_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Seleziona un nemico e premi col mouse su una cella per impostarlo", "Aiutino!");
        }

        private void HelpTexture_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Da qui puoi impostare alcuni parametri relativi alle texture specifici per questo livello", "Aiutino!");
        }

        private void BtnProvaInGame_Click(object sender, RoutedEventArgs e)
        {
            var gameExecutablePath = _settingsRepository.GetValue(SettingsKeys.GameExecutablePath);

            if (gameExecutablePath == null)
            {
                MessageBox.Show("Devi prima selezionare l'eseguibile del gioco!", "Attenzione");
                return;
            }

            if (!File.Exists(gameExecutablePath))
            {
                MessageBox.Show("L'eseguibile del gioco non esiste più, reimpostalo!", "Attenzione");
                return;
            }

            string randomFilePath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".mark";
            SerializeAndSave(randomFilePath);

            using (var process = new Process())
            {
                process.StartInfo.FileName = gameExecutablePath;
                process.StartInfo.Arguments = $"\"{randomFilePath}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(gameExecutablePath);

                process.Start();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    var errors = process.StandardError.ReadToEnd();
                    MessageBox.Show(errors, "Errore!");
                }
            }

            File.Delete(randomFilePath);
        }

        private void BtnCambiaSettingsGioco_Click(object sender, RoutedEventArgs e)
        {
            var g = new GameConfigEditorWindow();
            g.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show(
               messageBoxText: "Se non hai salvato, perderai le modifiche. Chiudere comunque?",
               caption: "Chiusura applicazione",
               button: MessageBoxButton.YesNo, icon: MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        private void BtnImpostaPathGioco_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Mark in DeDark game (*.exe)|*.exe",
                RestoreDirectory = true,
                Title = "Percorso del gioco",
                CheckFileExists = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _settingsRepository.SetValue(SettingsKeys.GameExecutablePath, openFileDialog.FileName);
            }
        }
    }
}
