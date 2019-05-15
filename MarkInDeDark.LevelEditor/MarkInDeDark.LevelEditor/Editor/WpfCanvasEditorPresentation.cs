using MarkInDeDark.LevelEditor.Domain;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace MarkInDeDark.LevelEditor.Editor
{
    public class WpfCanvasEditorPresentation
    {
        private readonly GameGrid _gameGrid;
        private readonly Canvas _canvas;
        private readonly int _cellSize;

        private Rectangle[,] _canvasCell;

        public WpfCanvasEditorPresentation(
            GameGrid gameGrid,
            Canvas canvas,
            int cellSize)
        {
            _gameGrid = gameGrid;
            _cellSize = cellSize;
            _canvas = canvas;

            DrawAll();
        }

        private void DrawAll()
        {
            _canvas.Children.Clear();
            _canvas.Width = _gameGrid.Columns * _cellSize;
            _canvas.Height = _gameGrid.Rows * _cellSize;
            _canvasCell = new Rectangle[_gameGrid.Columns, _gameGrid.Rows];

            for (int column = 0; column < _gameGrid.Columns; ++column)
            {
                for (int row = 0; row < _gameGrid.Rows; ++row)
                {
                    var cell = DrawSingleCell(row, column);
                    _canvas.Children.Add(cell);
                }
            }
        }

        private Rectangle DrawSingleCell(int row, int column)
        {
            var cellType = _gameGrid.Grid[column, row];

            var cell = new Rectangle
            {
                Fill = CellTypeColorConverter.Convert(cellType.CellType),
                Width = _cellSize,
                Height = _cellSize,
                Margin = new Thickness(column * _cellSize, row * _cellSize, 0, 0)
            };

            _canvasCell[column, row] = cell;
            return cell;
        }

        public void SetCellType(Point where, CellTypes type)
        {
            int column = (int)where.X / _cellSize;
            int row = (int)where.Y / _cellSize;

            _gameGrid.SetCellType(row, column, type);
            DrawSingleCell(row, column);
#warning Con DrawAll funziona, ma io voglio disegnarne solo uno!
        }
    }
}
