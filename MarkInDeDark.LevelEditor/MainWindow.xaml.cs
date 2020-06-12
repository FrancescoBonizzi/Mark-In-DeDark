using MarkInDeDark.Common;
using MarkInDeDark.GameGridCommon;
using MarkInDeDark.LevelEditor.Domain;
using MarkInDeDark.LevelEditor.Editor;
using MarkInDeDark.LevelEditor.Settings;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using WPFClient.Extensions;

namespace MarkInDeDark.LevelEditor
{
    public partial class MainWindow : Window
    {
        private const string _baseWindowTitle = "Mark in DeDark Level editor";

        private GameGridBundle _gameGrid;
        private WpfCanvasEditorPresentation _presentation;
        private TextFileStringsManager _traslationManager;

        private EditorActions _currentEditorAction;
        private GroundTypes _currentSelectedCellType;
        private WallTypes _currentSelectedWallType;
        private ObjectTypes _currentSelectedObjectType;

        private const int _defaultGridRows = 25;
        private const int _defaultGridCols = 45;
        private const int _cellSize = 30;

        private string _currentLevelFilePath;

        private readonly ISettingsRepository _settingsRepository;

        private string _currentLevelSerializedLastSave;

        public MainWindow()
        {
            InitializeComponent();
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowState = WindowState.Maximized;

            _settingsRepository = new FileSettingsRepository("configuration.txt");
        }

        private void NewGrid()
        {
            _gameGrid = new GameGridBundle(_defaultGridRows, _defaultGridCols);
            _presentation = new WpfCanvasEditorPresentation(
                this,
                _gameGrid,
                levelEditorCanvas,
                _cellSize,
                _traslationManager);
            Title = $"{_baseWindowTitle} - Nuovo";
            _currentLevelSerializedLastSave = null;
        }

        private void BtnNuovo_Click(object sender, RoutedEventArgs e)
        {
            if (CanOverwriteCurrentMap())
            {
                NewGrid();
            }
        }

        public bool CanOverwriteCurrentMap()
        {
            if (_gameGrid.IsEmpty)
                return true;

            var currentLevelSerialized = SerializeLevel();
            if (_currentLevelSerializedLastSave != currentLevelSerialized)
            {
                if (MessageBox.Show(
                      messageBoxText: "Se prosegui senza aver salvato perderai le modifiche fatte al file corrente. Proseguire?",
                      caption: "Proseguire?",
                      button: MessageBoxButton.YesNo, icon: MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return false;
                }
            }

            return true;
        }

        private void StartWaiting()
        {
            Cursor = Cursors.Wait;
            gridLoader.Visibility = Visibility.Visible;
            gridLoader.FadeIn(maxOpacity: 0.8, durationMilliseconds: 300);
        }

        private void StopWaiting()
        {
            Cursor = Cursors.Arrow;
            gridLoader.FadeOut(
                onCompleted: () => gridLoader.Visibility = Visibility.Collapsed,
                durationMilliseconds: 300);
        }

        private async void BtnApri_Click(object sender, RoutedEventArgs e)
        {
            if (!CanOverwriteCurrentMap())
                return;

            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Mark in DeDark files (*.mark)|*.mark",
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == false)
                return;
            _currentLevelFilePath = openFileDialog.FileName;

            StartWaiting();

            // Per far partire realmente il loader
            await Task.Delay(1000);
            await Task.Run(() =>
            {
                var levelContent = File.ReadAllText(openFileDialog.FileName);
                var gameGridSerializerManager = new GameGridDeserializerManager(levelContent);
                _gameGrid = gameGridSerializerManager.DeserializeLevel();
            });

            Dispatcher.Invoke(() =>
            {
                try
                {
                    _presentation = new WpfCanvasEditorPresentation(
                        this,
                        _gameGrid,
                        levelEditorCanvas,
                        _cellSize,
                        _traslationManager);

                    Title = $"{_baseWindowTitle} - {Path.GetFileNameWithoutExtension(openFileDialog.FileName)}";

                    // Lo riserializzo così se c'è un cambio di versione del serializzatore l'utente non se ne accorge
                    _currentLevelSerializedLastSave = SerializeLevel();
                }
                finally
                {
                    StopWaiting();
                }
            }, DispatcherPriority.Background);
        }

        private void BtnSalva_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentLevelFilePath))
            {
                BtnSalvaCopia_Click(sender, e);
                return;
            }

            var serializedLevel = SerializeLevel();
            SaveLevel(_currentLevelFilePath, serializedLevel);
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
                SaveLevel(saveFileDialog.FileName, SerializeLevel());
                _currentLevelFilePath = saveFileDialog.FileName;
                Title = $"{_baseWindowTitle} - {Path.GetFileNameWithoutExtension(saveFileDialog.FileName)}";
                MessageBox.Show("Salvataggio completato con successo", "Tutto bene!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private string SerializeLevel()
        {
            _gameGrid.DirtyProbabilityPercentage = numDirtyProbability.Value ?? 0;
            _gameGrid.EnvironmentTexturePackId = Convert.ToInt32((cmbTexturePack.SelectedItem as ComboBoxItem).Tag);
            return new GameGridSerializerV4().SerializeLevel(_gameGrid);
        }

        private void SaveLevel(string filePath, string serializedLevel)
        {
            File.WriteAllText(filePath, serializedLevel);
            _currentLevelSerializedLastSave = serializedLevel;
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

        private void BtnText_Click(object sender, RoutedEventArgs e)
        {
            _currentEditorAction = EditorActions.Text;
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
                var mousePosition = e.GetPosition(levelEditorCanvas);

                switch (_currentEditorAction)
                {
                    case EditorActions.Text:
                        {
                            var cellText = _presentation.GetTextOrEmpty(mousePosition);
                            var inputDialog = new InputDialog(
                                $"Inserimento testo",
                                "Inserisci il testo che vuoi mostrare alla pressione della cella:",
                                cellText);
                            if (inputDialog.ShowDialog() == false)
                            {
                                return;
                            }
                            _presentation.SetText(mousePosition, inputDialog.InputString);
                            MessageBox.Show("Ricordati di andare a tradurre nelle altre lingue il testo!", "Traduzione", MessageBoxButton.OK, MessageBoxImage.Information);
                            _traslationManager.Save();
                        }
                        break;

                    case EditorActions.Ground:
                        _presentation.SetGroundType(mousePosition, _currentSelectedCellType);
                        break;

                    case EditorActions.Object:

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
                            || _currentSelectedObjectType == ObjectTypes.Ghost
                            || _currentSelectedObjectType == ObjectTypes.Bubu)
                        {
                            string pathTemplate = $"{_currentSelectedObjectType.ToString().ToUpper()[0]}-*-*";

                            var inputDialog = new InputDialog(
                                $"Inserimento nuovo {_currentSelectedObjectType}",
                                "Inserisci la stringa per identificare il suo path:",
                                pathTemplate);
                            if (inputDialog.ShowDialog() == false)
                            {
                                return;
                            }

                            quantityInfo = inputDialog.InputString;
                        }

                        _presentation.SetObjectType(mousePosition, _currentSelectedObjectType, quantityInfo, _traslationManager);

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
            else // E' un hover
            {
                // L'effetto hover lo faccio solo con i muri perché ti serve capire in quale cella sei
                if (_currentEditorAction == EditorActions.Wall)
                {
                    // Effetto hover
                    _presentation.SetHoverEffect(e.GetPosition(levelEditorCanvas));
                }
            }
        }

        private void LevelEditorCanvas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F)
            {
                // Terra 
                BtnGroundType_Ground.IsChecked = true;
                _currentSelectedCellType = GroundTypes.Terra;
                _currentEditorAction = EditorActions.Ground;
                _presentation.ClearHover();
                e.Handled = true;
            }
            else if (e.Key == Key.W)
            {
                // Muro standard
                BtnWallType_Standard.IsChecked = true;
                _currentSelectedWallType = WallTypes.Occludente;
                _currentEditorAction = EditorActions.Wall;
                _presentation.ClearHover();
                e.Handled = true;
            }
            else if (e.Key == Key.D)
            {
                // No muro
                BtnWallType_No.IsChecked = true;
                _currentSelectedWallType = WallTypes.Niente;
                _currentEditorAction = EditorActions.Wall;
                _presentation.ClearHover();
                e.Handled = true;
            }
            else if (e.Key == Key.C)
            {
                // Cancella 
                BtnObjectTypeCancel.IsChecked = true;
                _currentSelectedObjectType = ObjectTypes.Cancella;
                _currentEditorAction = EditorActions.Object;
                _presentation.ClearHover();
                e.Handled = true;
            }
            else if (e.Key == Key.R)
            {
                // Terra riempimento
                BtnGroundType_Ground8.IsChecked = true;
                _currentSelectedCellType = GroundTypes.Riempimento;
                _currentEditorAction = EditorActions.Ground;
                _presentation.ClearHover();
                e.Handled = true;
            }

            if (_currentEditorAction == EditorActions.Wall)
            {
                if (e.Key == Key.Up)
                {
                    _presentation.SetWallOfCurrentHoveredCellType(_currentSelectedWallType, WallPositions.Top);
                    e.Handled = true;
                }
                else if (e.Key == Key.Right)
                {
                    _presentation.SetWallOfCurrentHoveredCellType(_currentSelectedWallType, WallPositions.Right);
                    e.Handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    _presentation.SetWallOfCurrentHoveredCellType(_currentSelectedWallType, WallPositions.Bottom);
                    e.Handled = true;
                }
                else if (e.Key == Key.Left)
                {
                    _presentation.SetWallOfCurrentHoveredCellType(_currentSelectedWallType, WallPositions.Left);
                    e.Handled = true;
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
            SaveLevel(randomFilePath, SerializeLevel());

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
            if (!CanOverwriteCurrentMap())
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

        private void btnTraduzioni_Click(object sender, RoutedEventArgs e)
        {
            var t = new TraslationsWindow(_traslationManager);
            t.ShowDialog();
            // Non devo fare il refresh delle stringhe nella mappa perché
            // non gli faccio modificare quelle autogenerate
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Non ti faccio usare niente se non selezionare il path del gioco,
            // perchè da quello dipendono le traduzioni e il config
            string gameExecutablePath;
            do
            {
                gameExecutablePath = _settingsRepository.GetValue(SettingsKeys.GameExecutablePath);
                if (gameExecutablePath == null)
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
            } while (gameExecutablePath == null);

            var gameExecutableDirectory = Path.GetDirectoryName(gameExecutablePath);
            var traslationsFilePath = Path.Combine(gameExecutableDirectory, "Assets", "Translations.json");
            if (!File.Exists(traslationsFilePath))
                throw new ArgumentException($"Il file delle traduzioni al path {traslationsFilePath} non esiste", "Attenzione");

            var saveFullPath = Path.GetFullPath(traslationsFilePath);
            var rawTranslations = File.ReadAllText(saveFullPath);
            _traslationManager = new TextFileStringsManager(rawTranslations)
            {
                DefaultPathForSave = saveFullPath
            };

            NewGrid();
            btnNonCella.IsChecked = true;
        }

        private void btnNormalMapRegulations_Click(object sender, RoutedEventArgs e)
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
            SaveLevel(randomFilePath, SerializeLevel());

            var w = new InGameParametersWindow(gameExecutablePath);
            w.Show();

            //using (var process = new Process())
            //{
            var process = new Process();
            process.StartInfo.FileName = gameExecutablePath;
            process.StartInfo.Arguments = $"\"{randomFilePath}\" \"NormalMapRegulations\""; /// Il secondo parametro non ha valore, basta che ci sia
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(gameExecutablePath);

            process.Start();
            //  process.Exited += Process_Exited;
            //
            // }

            // File.Delete(randomFilePath);
        }

    }
}
