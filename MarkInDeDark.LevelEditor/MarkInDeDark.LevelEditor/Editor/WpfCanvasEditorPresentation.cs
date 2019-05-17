using MarkInDeDark.LevelEditor.Domain;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MarkInDeDark.LevelEditor.Editor
{
    public class WpfCanvasEditorPresentation
    {
        private readonly GameGrid _gameGrid;
        private readonly Canvas _canvas;
        private readonly int _cellSize;

        private Border[,] _canvasCell;

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
            _canvasCell = new Border[_gameGrid.Columns, _gameGrid.Rows];

            for (int column = 0; column < _gameGrid.Columns; ++column)
            {
                for (int row = 0; row < _gameGrid.Rows; ++row)
                {
                    var editorCell = _gameGrid.Grid[column, row];

#warning Per quando metterò il testo
                    var cellContent = new TextBlock
                    {
                        Width = _cellSize,
                        Height = _cellSize
                    };

                    var cellBorder = new Border()
                    {
                        Child = cellContent,
                        Background = EditorTypesColorConverter.Convert(editorCell.GroundType),
                        Margin = new Thickness(column * _cellSize, row * _cellSize, 0, 0),
                        BorderBrush = new SolidColorBrush(Colors.Gray),
                        BorderThickness = new Thickness(2)
                    };

                    _canvasCell[column, row] = cellBorder;
                    _canvas.Children.Add(cellBorder);
                }
            }
        }

        public void SetGroundType(Point where, GroundTypes type)
        {
            int column = (int)where.X / _cellSize;
            int row = (int)where.Y / _cellSize;

            if (column < 0 || column >= _gameGrid.Columns
                || row < 0 || row >= _gameGrid.Rows)
            {
                return;
            }

            _gameGrid.SetGroundType(row, column, type);
            _canvasCell[column, row].Background = EditorTypesColorConverter.Convert(type);
        }

        public void SetWallType(Point where, WallTypes type)
        {
            double gridX = where.X / _cellSize;
            double gridY = where.Y / _cellSize;

            int column = (int)gridX;
            int row = (int)gridY;

            if (column < 0 || column >= _gameGrid.Columns
                || row < 0 || row >= _gameGrid.Rows)
            {
                return;
            }

            const int cellIntervals = 2;
            const double cellIntervalWidth = 1.0 / cellIntervals;
            const double cellIntervalHeight = 1.0 / cellIntervals;
            WallPositions wallPosition;

            if (gridX <= column + cellIntervalWidth)
            {
                wallPosition = WallPositions.Left;
            }
            else if (gridX >= column + (cellIntervalWidth * (cellIntervals - 1)))
            {
                wallPosition = WallPositions.Right;
            }
            else if (gridY <= row + cellIntervalHeight)
            {
                wallPosition = WallPositions.Top;
            }
            else if (gridY > row + (cellIntervalHeight * (cellIntervals - 1)))
            {
                wallPosition = WallPositions.Bottom;
            }
            else
            {
                return;
            }

            _gameGrid.SetWallType(row, column, type, wallPosition);

            var currentBorderThickness = _canvasCell[column, row].BorderThickness;

            const int strokeWidth = 4;
            switch (type)
            {
                case WallTypes.Niente:
                    _canvasCell[column, row].BorderThickness = new Thickness(0);
                    break;

                case WallTypes.Occludente:
                case WallTypes.Fragile:
                    _canvasCell[column, row].BorderThickness = new Thickness(
                        left: wallPosition == WallPositions.Left ? strokeWidth : currentBorderThickness.Left,
                        top: wallPosition == WallPositions.Top ? strokeWidth : currentBorderThickness.Top,
                        right: wallPosition == WallPositions.Right ? strokeWidth : currentBorderThickness.Right,
                        bottom: wallPosition == WallPositions.Bottom ? strokeWidth : currentBorderThickness.Bottom);
                    break;
            }

            _canvasCell[column, row].BorderBrush = EditorTypesColorConverter.Convert(type);

            System.Diagnostics.Debug.WriteLine($"Changed a cell: {column}, {row} - {wallPosition}");

        }
    }
}