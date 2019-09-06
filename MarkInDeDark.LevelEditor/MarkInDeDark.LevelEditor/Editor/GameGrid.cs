using MarkInDeDark.LevelEditor.Domain;

namespace MarkInDeDark.LevelEditor.Editor
{
    public class GameGrid
    {
        public int Rows { get; set; }
        public int Columns { get; set; }

        public Cell[,] Grid { get; set; }

        public GameGrid(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            ClearGrid();
        }

        public GameGrid(Cell[,] grid)
        {
            Grid = grid;
            Columns = grid.GetLength(0);
            Rows = grid.GetLength(1);
        }

        public void ClearGrid()
        {
            var emptyGrid = new Cell[Columns, Rows];

            for (int column = 0; column < Columns; ++column)
            {
                for (int row = 0; row < Rows; ++row)
                {
                    emptyGrid[column, row] = new Cell() { GroundType = GroundTypes.Buio };
                }
            }

            Grid = emptyGrid;
        }

        public void SetGroundType(int row, int column, GroundTypes type)
        {
            Grid[column, row].GroundType = type;
        }

        public void SetObjectType(int row, int column, ObjectTypes type, string quantityInfo = null)
        {
            Grid[column, row].ObjectType = type;
            Grid[column, row].ObjectTypeQuantityInfo = quantityInfo;
        }

        public void SetWallType(int row, int column, WallTypes type, WallPositions wallPosition)
        {
            switch (wallPosition)
            {
                case WallPositions.Top:
                    Grid[column, row].Top = type;
                    break;

                case WallPositions.Right:
                    Grid[column, row].Right = type;
                    break;

                case WallPositions.Left:
                    Grid[column, row].Left = type;
                    break;

                case WallPositions.Bottom:
                    Grid[column, row].Bottom = type;
                    break;
            }
        }

    }
}