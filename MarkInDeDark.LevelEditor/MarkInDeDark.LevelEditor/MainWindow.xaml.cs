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
        }

        private void BtnWallType_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as RadioButton;
            _currentSelectedWallType = (WallTypes)Convert.ToInt32(button.Tag);
            _currentEditorAction = EditorActions.Wall;
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

                    case EditorActions.Wall:
                        _presentation.SetWallType(e.GetPosition(levelEditorCanvas), _currentSelectedWallType);
                        break;
                }
            }
        }

    }
}
