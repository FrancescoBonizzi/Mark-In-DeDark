using MarkInDeDark.GameGrid;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MarkInDeDark.LevelEditor.Editor
{
    public class WpfCanvasEditorPresentation
    {
        private readonly GameGridBundle _gameGrid;
        private readonly Canvas _canvas;
        private readonly MainWindow _window;

        private readonly int _cellSize;
        private Border[,] _canvasCell;
        private (int Column, int Row)? _lastHoveredCell;

        public WpfCanvasEditorPresentation(
            MainWindow window,
            GameGridBundle gameGrid,
            Canvas canvas,
            int cellSize)
        {
            _gameGrid = gameGrid;
            _cellSize = cellSize;
            _canvas = canvas;
            _window = window;

            _window.BtnObjectType_Player.IsEnabled = true;
            _window.BtnObjectType_Key.IsEnabled = true;
            _window.BtnObjectType_Exit.IsEnabled = true;
            _window.BtnObjectType_Goblin.IsEnabled = true;

            _window.cmbTexturePack.SelectedIndex = 0;
            _window.numDirtyProbability.Value = 10;

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

                    UIElement cellContent = new TextBlock();
                    if (editorCell.ObjectType != null)
                    {
                        cellContent = EditorTypesObjectsConverter.Convert(editorCell.ObjectType.Value, editorCell.ObjectTypeAdditionalData);

                        switch (editorCell.ObjectType.Value)
                        {
                            case ObjectTypes.Giocatore:
                                _window.BtnObjectType_Player.IsEnabled = false;
                                break;

                            case ObjectTypes.Chiave:
                                _window.BtnObjectType_Key.IsEnabled = false;
                                break;

                            case ObjectTypes.Uscita:
                                _window.BtnObjectType_Exit.IsEnabled = false;
                                break;

                            case ObjectTypes.Goblin:
                                _window.BtnObjectType_Goblin.IsEnabled = false;
                                break;
                        }
                    }

                    var cellBorder = new Border()
                    {
                        Child = cellContent,
                        Background = EditorTypesColorConverter.Convert(editorCell.GroundType),
                        Margin = new Thickness(column * _cellSize, row * _cellSize, 0, 0),
                        Width = _cellSize,
                        Height = _cellSize
                    };

                    _canvasCell[column, row] = cellBorder;
                    _canvas.Children.Add(cellBorder);

                    if (editorCell.Top != WallTypes.Niente)
                        SetWallType(editorCell.Top, WallPositions.Top, row, column);
                    if (editorCell.Right != WallTypes.Niente)
                        SetWallType(editorCell.Right, WallPositions.Right, row, column);
                    if (editorCell.Bottom != WallTypes.Niente)
                        SetWallType(editorCell.Bottom, WallPositions.Bottom, row, column);
                    if (editorCell.Left != WallTypes.Niente)
                        SetWallType(editorCell.Left, WallPositions.Left, row, column);
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

        public void SetObjectType(Point where, ObjectTypes type, string quantityInfo)
        {
            int column = (int)where.X / _cellSize;
            int row = (int)where.Y / _cellSize;

            if (column < 0 || column >= _gameGrid.Columns
                || row < 0 || row >= _gameGrid.Rows)
            {
                return;
            }

            var previousObjectType = _gameGrid.Grid[column, row].ObjectType;
            var previousGroundType = _gameGrid.Grid[column, row].GroundType;

            _gameGrid.SetObjectType(row, column, type, quantityInfo);

            var canvasCell = _canvasCell[column, row];

            // Mantengo gli altri tipi di ground type con il colore corretto, in modo da poter mettere una chiave su una trappola per esempio
            if (previousGroundType == GroundTypes.NonCella || previousGroundType == GroundTypes.Macerie)
            {
                canvasCell.Background = EditorTypesColorConverter.Convert(GroundTypes.Terra);
            }

            canvasCell.Child = EditorTypesObjectsConverter.Convert(type, quantityInfo);

            // Gli oggetti che possono esistere solo in una quantità disattivano il pulsante di inserimento 
            // una volta inserto l'oggetto
            switch (type)
            {
                case ObjectTypes.Cancella:

                    if (previousObjectType != null)
                    {
                        switch (previousObjectType.Value)
                        {
                            case ObjectTypes.Giocatore:
                                _window.BtnObjectType_Player.IsEnabled = true;
                                break;

                            case ObjectTypes.Chiave:
                                _window.BtnObjectType_Key.IsEnabled = true;
                                break;

                            case ObjectTypes.Uscita:
                                _window.BtnObjectType_Exit.IsEnabled = true;
                                break;

                            case ObjectTypes.Goblin:
                                _window.BtnObjectType_Goblin.IsEnabled = true;
                                break;
                        }
                    }

                    break;

                case ObjectTypes.Giocatore:
                    _window.BtnObjectType_Player.IsEnabled = false;
                    if (previousObjectType != null)
                    {
                        switch (previousObjectType.Value)
                        {
                            case ObjectTypes.Chiave:
                                _window.BtnObjectType_Key.IsEnabled = true;
                                break;

                            case ObjectTypes.Uscita:
                                _window.BtnObjectType_Exit.IsEnabled = true;
                                break;

                            case ObjectTypes.Goblin:
                                _window.BtnObjectType_Goblin.IsEnabled = true;
                                break;
                        }
                    }

                    break;

                case ObjectTypes.Chiave:
                    _window.BtnObjectType_Key.IsEnabled = false;
                    if (previousObjectType != null)
                    {
                        switch (previousObjectType.Value)
                        {
                            case ObjectTypes.Giocatore:
                                _window.BtnObjectType_Player.IsEnabled = true;
                                break;

                            case ObjectTypes.Uscita:
                                _window.BtnObjectType_Exit.IsEnabled = true;
                                break;

                            case ObjectTypes.Goblin:
                                _window.BtnObjectType_Goblin.IsEnabled = true;
                                break;
                        }
                    }

                    break;

                case ObjectTypes.Uscita:
                    _window.BtnObjectType_Exit.IsEnabled = false;
                    if (previousObjectType != null)
                    {
                        switch (previousObjectType.Value)
                        {
                            case ObjectTypes.Giocatore:
                                _window.BtnObjectType_Player.IsEnabled = true;
                                break;

                            case ObjectTypes.Chiave:
                                _window.BtnObjectType_Key.IsEnabled = true;
                                break;

                            case ObjectTypes.Goblin:
                                _window.BtnObjectType_Goblin.IsEnabled = true;
                                break;
                        }
                    }
                    break;

                case ObjectTypes.Goblin:
                    _window.BtnObjectType_Goblin.IsEnabled = false;
                    if (previousObjectType != null)
                    {
                        switch (previousObjectType.Value)
                        {
                            case ObjectTypes.Giocatore:
                                _window.BtnObjectType_Player.IsEnabled = true;
                                break;

                            case ObjectTypes.Chiave:
                                _window.BtnObjectType_Key.IsEnabled = true;
                                break;

                            case ObjectTypes.Uscita:
                                _window.BtnObjectType_Exit.IsEnabled = true;
                                break;
                        }
                    }
                    break;

                case ObjectTypes.Zombie:
                case ObjectTypes.Scheletro:
                case ObjectTypes.Taurus:
                case ObjectTypes.Ghost:
                case ObjectTypes.Bubu:
                case ObjectTypes.Candele:

                    if (previousObjectType != null)
                    {
                        switch (previousObjectType.Value)
                        {
                            case ObjectTypes.Giocatore:
                                _window.BtnObjectType_Player.IsEnabled = true;
                                break;

                            case ObjectTypes.Chiave:
                                _window.BtnObjectType_Key.IsEnabled = true;
                                break;

                            case ObjectTypes.Uscita:
                                _window.BtnObjectType_Exit.IsEnabled = true;
                                break;

                            case ObjectTypes.Goblin:
                                _window.BtnObjectType_Goblin.IsEnabled = true;
                                break;
                        }
                    }

                    break;
            }
        }

        public void ClearHover()
        {
            // ...e rimozione dell'hover
            if (_lastHoveredCell != null)
            {
                var lastHoveredCellGroundType = _gameGrid.Grid[_lastHoveredCell.Value.Column, _lastHoveredCell.Value.Row].GroundType;
                _canvasCell[_lastHoveredCell.Value.Column, _lastHoveredCell.Value.Row].Background =
                    EditorTypesColorConverter.Convert(lastHoveredCellGroundType);
            }
        }

        public void SetHover(Point where)
        {
            int column = (int)where.X / _cellSize;
            int row = (int)where.Y / _cellSize;

            if (column < 0 || column >= _gameGrid.Columns
                || row < 0 || row >= _gameGrid.Rows)
            {
                return;
            }

            // Gestione hover...
            if (_lastHoveredCell != (column, row))
            {
                var thisCellBackground = _canvasCell[column, row].Background as SolidColorBrush;
                _canvasCell[column, row].Background = new SolidColorBrush(thisCellBackground.Color.Lighter());

                // ...e rimozione dell'hover
                ClearHover();

                _lastHoveredCell = (column, row);
            }
        }

        public void SetWallOfCurrentHoveredCellType(WallTypes type, WallPositions wallPosition)
        {
            if (_lastHoveredCell == null)
            {
                return;
            }

            int column = _lastHoveredCell.Value.Column;
            int row = _lastHoveredCell.Value.Row;

            SetWallType(type, wallPosition, row, column);
        }

        public void SetWallType(
            WallTypes type,
            WallPositions wallPosition,
            int row,
            int column)
        {
            _gameGrid.SetWallType(row, column, type, wallPosition);

            var currentBorderThickness = _canvasCell[column, row].BorderThickness;
            int strokeWidth = type == WallTypes.Niente ? 0 : 4;

            _canvasCell[column, row].BorderThickness = new Thickness(
                left: wallPosition == WallPositions.Left ? strokeWidth : currentBorderThickness.Left,
                top: wallPosition == WallPositions.Top ? strokeWidth : currentBorderThickness.Top,
                right: wallPosition == WallPositions.Right ? strokeWidth : currentBorderThickness.Right,
                bottom: wallPosition == WallPositions.Bottom ? strokeWidth : currentBorderThickness.Bottom);

            if (type != WallTypes.Niente)
            {
                _canvasCell[column, row].BorderBrush = EditorTypesColorConverter.Convert(type);
            }
        }
    }
}