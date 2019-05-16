using MarkInDeDark.LevelEditor.Domain;
using MarkInDeDark.LevelEditor.Editor;
using System.Windows;
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

        private void BtnBuio_Click(object sender, RoutedEventArgs e)
        {
            _currentSelectedCellType = CellTypes.Buio;
        }

        private void BtnTerra_Click(object sender, RoutedEventArgs e)
        {
            _currentSelectedCellType = CellTypes.Terra;
        }

        private void BtnMacerie_Click(object sender, RoutedEventArgs e)
        {
            _currentSelectedCellType = CellTypes.Macerie;
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
