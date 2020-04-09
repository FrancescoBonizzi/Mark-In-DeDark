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

        public static void CleanUpGameGridBundle(GameGridBundle levelEditorGrid)
        {
            // Salto l'ultima riga perché non ha di sicuro la cella sotto
            for (int j = 0; j < levelEditorGrid.Rows - 1; ++j)
            {
                for (int i = 0; i < levelEditorGrid.Columns; ++i)
                {
                    var cell = levelEditorGrid.Grid[i, j];
                    var southCell = SouthCell(levelEditorGrid.Grid, i, j);
                    if (southCell.GroundType != GroundTypes.NonCella)
                    {
                        if (cell.Bottom == WallTypes.Occludente || cell.Bottom == WallTypes.Fragile || cell.Bottom == WallTypes.Porta)
                        {
                            southCell.Top = cell.Bottom;
                            cell.Bottom = WallTypes.Niente;
                        }
                    }
                }
            }

            // Salto l'ultima colonna perché non ha di sicuro la cella a destra
            for (int i = 0; i < levelEditorGrid.Columns - 1; ++i)
            {
                for (int j = 0; j < levelEditorGrid.Rows; ++j)
                {
                    var cell = levelEditorGrid.Grid[i, j];
                    var eastCell = EastCell(levelEditorGrid.Grid, i, j);
                    if (eastCell.GroundType != GroundTypes.NonCella)
                    {
                        if (cell.Right == WallTypes.Occludente || cell.Right == WallTypes.Fragile || cell.Right == WallTypes.Porta)
                        {
                            eastCell.Left = cell.Right;
                            cell.Right = WallTypes.Niente;
                        }
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

                        foreach (var cellAngle in cell.Pillars)
                        {
                            foreach (var nearCell in nearCells)
                            {
                                if (nearCell != null)
                                {
                                    if (nearCell.Pillars.Count != 0)
                                    {
                                        for (int w = nearCell.Pillars.Count - 1; w >= 0; --w)
                                        {
                                            var nearCellAngle = nearCell.Pillars.ElementAt(w);
                                            if (cellAngle.Value.DrawingInfos.Position.IsEqualsConvertedToInt(nearCellAngle.Value.DrawingInfos.Position))
                                            {
                                                nearCell.Pillars.Remove(nearCellAngle.Key);
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
