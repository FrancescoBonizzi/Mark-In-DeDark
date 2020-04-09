using MarkInDeDark.GameGrid;
using MarkInDeDarkMono.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDarkMono.Levels
{
    public static class GameGridCleaner
    {
        private static Cell NorthCell(Cell[,] grid, int i, int j)
            => grid[i, j - 1];

        public static Cell SouthCell(Cell[,] grid, int i, int j)
            => grid[i, j + 1];

        public static Cell EastCell(Cell[,] grid, int i, int j)
            => grid[i + 1, j];

        public static Cell WestCell(Cell[,] grid, int i, int j)
            => grid[i - 1, j];

        public static void SetAlwaysSouthWall(GameGridBundle levelEditorGrid)
        {
            // Parto dalla prima riga perché non ha di sicuro la cella sopra
            for (int j = 1; j < levelEditorGrid.Rows; ++j)
            {
                for (int i = 0; i < levelEditorGrid.Columns; ++i)
                {
                    var cell = levelEditorGrid.Grid[i, j];
                    var topCell = NorthCell(levelEditorGrid.Grid, i, j);

                    if (cell.Top == WallTypes.Occludente || cell.Top == WallTypes.Fragile)
                    {
                        topCell.Bottom = cell.Top;
                        cell.Top = WallTypes.Niente;
                    }
                }
            }
        }

        public static void CleanAnglesAndWallsGrid(GameGrid grid)
        {
            // Elimino le tutti i muri doppi gli angoli doppi
            // Per ogni angolo devo vedere se quell'angolo o quel muro esiste già uguale vicino
            for (int i = 0; i < GameGrid.HorizontalCellsNumber; ++i)
            {
                for (int j = 0; j < GameGrid.VerticalCellsNumber; ++j)
                {
                    var cell = grid.GetCellByIndex(i, j);

                    if (cell == null)
                        continue;

                    var nearCells = new List<GameCell>()
                    {
                        cell.NorthCellWithoutLinks,
                        cell.SouthCellWithoutLinks,
                        cell.EastCellWithoutLinks,
                        cell.WestCellWithoutLinks,
                        cell.NorthCellWithoutLinks?.EastCellWithoutLinks,
                        cell.NorthCellWithoutLinks?.WestCellWithoutLinks,
                        cell.SouthCellWithoutLinks?.EastCellWithoutLinks,
                        cell.SouthCellWithoutLinks?.WestCellWithoutLinks,
                    };

                    foreach (var wall in cell.Walls)
                    {
                        foreach (var nearCell in nearCells)
                        {
                            if (nearCell != null)
                            {
                                if (nearCell.Walls.Count != 0)
                                {
                                    for (int w = nearCell.Walls.Count - 1; w >= 0; --w)
                                    {
                                        var nearCellWall = nearCell.Walls.ElementAt(w);
                                        if (wall.Value.DrawingInfos.Position.IsEqualsConvertedToInt(nearCellWall.Value.DrawingInfos.Position))
                                        {
                                            // Fix balordo per evitare di perdere l'info sulla rompibilità
                                            if (!nearCellWall.Value.IsDestroyable)
                                                nearCell.Walls.Remove(nearCellWall.Key);
                                        }
                                    }
                                }
                            }
                        }

                        foreach (var cellAngle in cell.Angles)
                        {
                            foreach (var nearCell in nearCells)
                            {
                                if (nearCell != null)
                                {
                                    if (nearCell.Angles.Count != 0)
                                    {
                                        for (int w = nearCell.Angles.Count - 1; w >= 0; --w)
                                        {
                                            var nearCellAngle = nearCell.Angles.ElementAt(w);
                                            if (cellAngle.Value.Position.IsEqualsConvertedToInt(nearCellAngle.Value.Position))
                                            {
                                                nearCell.Angles.Remove(nearCellAngle.Key);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }

    }
}
