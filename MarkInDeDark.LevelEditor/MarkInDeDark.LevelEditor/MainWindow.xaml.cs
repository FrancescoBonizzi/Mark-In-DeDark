using MarkInDeDark.LevelEditor.Domain;
using MarkInDeDark.LevelEditor.Editor;
using MarkInDeDark.LevelEditor.Settings;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MarkInDeDark.LevelEditor
{
    public partial class MainWindow : Window
    {
        private GameGrid _gameGrid;
        private WpfCanvasEditorPresentation _presentation;

        private EditorActions _currentEditorAction;
        private GroundTypes _currentSelectedCellType;
        private WallTypes _currentSelectedWallType;
        private ObjectTypes _currentSelectedObjectType;

        private string _currentLevelFilePath;

        private const int _gridRows = 25;
        private const int _gridCols = 45;
        private const int _cellSize = 30;

        private readonly ISettingsRepository _settingsRepository;
        
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowState = WindowState.Maximized;

            _settingsRepository = new FileSettingsRepository("configuration.txt");

            NewGrid();
            btnBuio.IsChecked = true;
        }

        private void NewGrid()
        {
            _gameGrid = new GameGrid(_gridRows, _gridCols);
            _presentation = new WpfCanvasEditorPresentation(
                this,
                _gameGrid,
                levelEditorCanvas,
                _cellSize);
        }

        private void BtnNuovo_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                messageBoxText: "Se prosegui senza aver salvato perderai le modifiche fatte al file corrente. Proseguire?",
                caption: "Proseguire?",
                button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                NewGrid();
            }
        }

        private void BtnApri_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Mark in DeDark files (*.mark)|*.mark",
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var levelContent = File.ReadAllText(openFileDialog.FileName);
                _gameGrid = new GameGrid(_gridRows, _gridCols);

                new GameGridSerializer().DeserializeLevel(levelContent, _gameGrid);

                _presentation = new WpfCanvasEditorPresentation(
                    this,
                    _gameGrid,
                    levelEditorCanvas,
                    _cellSize);

                _currentLevelFilePath = openFileDialog.FileName;
            }
        }

        private void BtnSalva_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentLevelFilePath))
            {
                BtnSalvaCopia_Click(sender, e);
                return;
            }

            var serializedLevel = new GameGridSerializer().SerializeLevel(_gameGrid);
            File.WriteAllText(_currentLevelFilePath, serializedLevel);
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
                var serializedLevel = new GameGridSerializer().SerializeLevel(_gameGrid);
                File.WriteAllText(saveFileDialog.FileName, serializedLevel);
                _currentLevelFilePath = saveFileDialog.FileName;
                MessageBox.Show("Salvataggio completato con successo", "Tutto bene!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
                            || _currentSelectedObjectType == ObjectTypes.Taurus)
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

        private void BtnProvaInGame_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnCambiaSettingsGioco_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
