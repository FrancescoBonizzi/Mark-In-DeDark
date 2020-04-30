using MarkInDeDark.Library.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.Library.Levels
{
    public static class GameGridCleaner
    {
        public static void CleanPillarsAndWallsGrid(GameGrid grid)
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
                                        if (wall.Value.Position.IsEqualsConvertedToInt(
                                            nearCellWall.Value.Position))
                                        {
                                            // Fix balordo per evitare di perdere l'info sulla rompibilità
                                            if (!(nearCellWall.Value is WallBreakable))
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
                                            if (cellAngle.Value.DrawingInfos.Position.IsEqualsConvertedToInt(
                                                nearCellAngle.Value.DrawingInfos.Position))
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
