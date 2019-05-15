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
        }

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
            _currentSelectedCellType = CellTypes.Dark;
            // TODO Deselezionare gli altri
            // https://stackoverflow.com/questions/2362641/how-to-get-a-group-of-toggle-buttons-to-act-like-radio-buttons-in-wpf
        }

        private void BtnTerra_Click(object sender, RoutedEventArgs e)
        {
            _currentSelectedCellType = CellTypes.Ground;
            // TODO Deselezionare gli altri
            // https://stackoverflow.com/questions/2362641/how-to-get-a-group-of-toggle-buttons-to-act-like-radio-buttons-in-wpf
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
                _presentation.SetCellType(e.GetPosition(levelEditorCanvas), CellTypes.Dark);
            }
        }
    }
}
