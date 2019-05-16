using MarkInDeDark.LevelEditor.Domain;

namespace MarkInDeDark.LevelEditor.Editor
{
    public class GameGrid
    {
        public int Rows { get; }
        public int Columns { get; }

        public Cell[,] Grid { get; private set; }

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
                    emptyGrid[column, row] = new Cell() { CellType = CellTypes.Buio };
                }
            }

            Grid = emptyGrid;
        }

        public void SetCellType(int row, int column, CellTypes type)
        {
            if (column >= 0 && column < Columns
                && row >= 0 && row < Rows)
            {
                Grid[column, row].CellType = type;
            }
        }
    }
}
