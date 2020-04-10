namespace MarkInDeDark.GameGrid
{
    public class GameGridBundle
    {
        public int Rows { get; set; }
        public int Columns { get; set; }

        public Cell[,] Grid { get; set; }

        public int EnvironmentTexturePackId { get; set; }
        public int DirtyProbabilityPercentage { get; set; }

        public GameGridBundle(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            ClearGrid();
        }

        public GameGridBundle(Cell[,] grid)
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
                    emptyGrid[column, row] = new Cell() { GroundType = GroundTypes.NonCella };
                }
            }

            Grid = emptyGrid;
        }

        public void SetGroundType(int row, int column, GroundTypes type)
        {
            Grid[column, row].GroundType = type;
        }

        public void SetObjectType(int row, int column, ObjectTypes type, string additionalData = null)
        {
            Grid[column, row].ObjectType = type;
            Grid[column, row].ObjectTypeAdditionalData = additionalData;
        }

        public void SetStringCode(int row, int column, string stringCode)
        {
            Grid[column, row].StringCode = stringCode;
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

            if (type != WallTypes.Niente)
            {
                // Dato che non posso avere colori diversi per bordi della stessa cella, 
                // devo per forza sceglierne uno, altrimenti chi fa il livello si confonde
                // e devo imporlo anche agli altri.
                // (Può succedere che tu veda tutti verdi, ma in realtà sotto uno sia rosso)
                if (Grid[column, row].Top != WallTypes.Niente)
                    Grid[column, row].Top = type;

                if (Grid[column, row].Right != WallTypes.Niente)
                    Grid[column, row].Right = type;

                if (Grid[column, row].Left != WallTypes.Niente)
                    Grid[column, row].Left = type;

                if (Grid[column, row].Bottom != WallTypes.Niente)
                    Grid[column, row].Bottom = type;
            }
        }

    }
}