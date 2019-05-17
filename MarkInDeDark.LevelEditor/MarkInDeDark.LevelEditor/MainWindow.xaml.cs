using MarkInDeDark.LevelEditor.Domain;
using MarkInDeDark.LevelEditor.Editor;
using System;
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

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            NewGrid();
            btnBuio.IsChecked = true;
        }

#warning Lo stile dei pulsanti dev'essere centralizzato in qualche modo

        private void NewGrid()
        {
            _gameGrid = new GameGrid(25, 45);
            _presentation = new WpfCanvasEditorPresentation(_gameGrid, levelEditorCanvas, 30);
        }

        private void BtnNuovo_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnApri_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnSalva_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
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
    }
}
