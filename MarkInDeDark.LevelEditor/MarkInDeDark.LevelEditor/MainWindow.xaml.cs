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

        private CellTypes _currentSelectedCellType;

        public MainWindow()
        {
            InitializeComponent();
            NewGrid();
            btnBuio.IsChecked = true;
        }

#warning Lo stile dei pulsanti dev'essere centralizzato in qualche modo
#warning Sperimentare con i resource dictionary così trovo un UI più carina

        private void NewGrid()
        {
            _gameGrid = new GameGrid(25, 45);
            _presentation = new WpfCanvasEditorPresentation(_gameGrid, levelEditorCanvas, 30);
        }

        private void BtnNuovo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnApri_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnSalva_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCellType_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as RadioButton;
            _currentSelectedCellType = (CellTypes)Convert.ToInt32(button.Tag);
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
                _presentation.SetCellType(e.GetPosition(levelEditorCanvas), _currentSelectedCellType);
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                _presentation.SetCellType(e.GetPosition(levelEditorCanvas), CellTypes.Buio);

#warning TODO Mettere una gomma o qualcosa di simile
                levelEditorCanvas.Cursor = Cursors.AppStarting;
            }
            else if (e.RightButton == MouseButtonState.Released)
            {
                levelEditorCanvas.Cursor = Cursors.Pen;
            }
        }

    }
}
