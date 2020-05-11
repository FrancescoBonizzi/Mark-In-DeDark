using MarkInDeDark.GameGridCommon;
using MarkInDeDark.Library.Extensions;
using MarkInDeDark.Library.Maths;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.Library.Levels
{
    public static class PillarFactory
    {
        public static List<Pillar> GeneratePillars(
            GameCell cell,
            Level level)
        {
            if (cell == null || cell.IsUnLinked)
                return new List<Pillar>();

            var pillars = new List<Pillar>();
            
            if (!cell.IsEastPassable)
                if (cell.Links[LinkDirection.NorthLink] != null)
                    if (cell.Links[LinkDirection.NorthLink].Links[LinkDirection.EastLink] != null)
                        if (!cell.Links[LinkDirection.NorthLink].Links[LinkDirection.EastLink].IsSouthPassable)
                            pillars.Add(GenerateNEPillar(cell, level));

            if (!cell.IsWestPassable)
                if (cell.Links[LinkDirection.NorthLink] != null)
                    if (cell.Links[LinkDirection.NorthLink].Links[LinkDirection.WestLink] != null)
                        if (!cell.Links[LinkDirection.NorthLink].Links[LinkDirection.WestLink].IsSouthPassable)
                            pillars.Add(GenerateNWPillar(cell, level));

            if (cell.Links[LinkDirection.NorthLink] != null)
                if (!cell.Links[LinkDirection.NorthLink].IsEastPassable)
                    if (cell.Links[LinkDirection.EastLink] != null)
                        if (!cell.Links[LinkDirection.EastLink].IsNorthPassable)
                            pillars.Add(GenerateNEPillar(cell, level));

            if (cell.Links[LinkDirection.NorthLink] != null)
                if (!cell.Links[LinkDirection.NorthLink].IsWestPassable)
                    if (cell.Links[LinkDirection.WestLink] != null)
                        if (!cell.Links[LinkDirection.WestLink].IsNorthPassable)
                            pillars.Add(GenerateNWPillar(cell, level));

            if (!cell.IsNorthPassable)
                if (cell.Links[LinkDirection.EastLink] != null)
                    if (!cell.Links[LinkDirection.EastLink].IsNorthDoor && (cell.Links[LinkDirection.EastLink].IsNorthPassable || cell.Links[LinkDirection.EastLink].IsNorthDestroyable))
                        if (!cell.IsNorthDestroyable || !cell.Links[LinkDirection.EastLink].IsNorthDestroyable)
                            if (cell.Links[LinkDirection.EastLink]?.Links[LinkDirection.NorthLink]?.IsSouthDoor == false)
                                pillars.Add(GenerateNEPillar(cell, level));

            if (!cell.IsNorthPassable)
                if (cell.Links[LinkDirection.WestLink] != null)
                    if (!cell.Links[LinkDirection.WestLink].IsNorthDoor && (cell.Links[LinkDirection.WestLink].IsNorthPassable || cell.Links[LinkDirection.WestLink].IsNorthDestroyable))
                        if (!cell.IsNorthDestroyable || !cell.Links[LinkDirection.WestLink].IsNorthDestroyable)
                            if (cell.Links[LinkDirection.WestLink]?.Links[LinkDirection.NorthLink]?.IsSouthDoor == false)
                                pillars.Add(GenerateNWPillar(cell, level));

            if (!cell.IsEastPassable)
                if (cell.Links[LinkDirection.NorthLink] != null)
                    if (cell.Links[LinkDirection.NorthLink].IsEastPassable || cell.Links[LinkDirection.NorthLink].IsEastDestroyable)
                        if (!cell.IsEastDestroyable || !cell.Links[LinkDirection.NorthLink].IsEastDestroyable)
                            pillars.Add(GenerateNEPillar(cell, level));

            if (!cell.IsEastPassable)
                if (cell.Links[LinkDirection.SouthLink] != null)
                    if (cell.Links[LinkDirection.SouthLink].IsEastPassable || cell.Links[LinkDirection.SouthLink].IsEastDestroyable)
                        if (!cell.IsEastDestroyable || !cell.Links[LinkDirection.SouthLink].IsEastDestroyable)
                            pillars.Add(GenerateSEPillar(cell, level));

            if (!cell.IsWestPassable)
                if (cell.Links[LinkDirection.SouthLink] != null)
                    if (cell.Links[LinkDirection.SouthLink].IsWestPassable || cell.Links[LinkDirection.SouthLink].IsWestDestroyable)
                        if (!cell.IsWestDestroyable || !cell.Links[LinkDirection.SouthLink].IsWestDestroyable)
                            pillars.Add(GenerateSWPillar(cell, level));

            if (!cell.IsNorthPassable && !cell.IsEastPassable)
                pillars.Add(GenerateNEPillar(cell, level));
            if (!cell.IsNorthPassable && !cell.IsWestPassable)
                pillars.Add(GenerateNWPillar(cell, level));
            if (!cell.IsSouthPassable && !cell.IsEastPassable)
                pillars.Add(GenerateSEPillar(cell, level));
            if (!cell.IsSouthPassable && !cell.IsWestPassable)
                pillars.Add(GenerateSWPillar(cell, level));

            return pillars.DistinctBy(p => p.PillarPosition).ToList();
        }

        private static Pillar GenerateNWPillar(GameCell cell, Level level)
        {
            var (Base, Normal) = Utilities.RandomBetween(level.RandomPillarSprites);
            return new Pillar(
                new Vector2(
                    cell.TopLeftPosition.X - level.RandomPillarSprites[0].Base.Width / 2f,
                    cell.TopLeftPosition.Y + WallFactory.NorthWallYOffset),
                Base,
                Normal,
                PillarPositions.NW);
        }

        private static Pillar GenerateNEPillar(GameCell cell, Level level)
        {
            var (Base, Normal) = Utilities.RandomBetween(level.RandomPillarSprites);
            return new Pillar(
                new Vector2(
                      cell.TopLeftPosition.X + cell.Width - level.RandomPillarSprites[0].Base.Width / 2f,
                      cell.TopLeftPosition.Y + WallFactory.NorthWallYOffset),
                Base,
                Normal,
                PillarPositions.NE);
        }

        private static Pillar GenerateSWPillar(GameCell cell, Level level)
        {
            var (Base, Normal) = Utilities.RandomBetween(level.RandomPillarSprites);
            return new Pillar(
                new Vector2(
                    cell.TopLeftPosition.X - level.RandomPillarSprites[0].Base.Width / 2f,
                    cell.TopLeftPosition.Y + WallFactory.SouthWallYOffset),
                Base,
                Normal,
                PillarPositions.SW);
        }

        private static Pillar GenerateSEPillar(GameCell cell, Level level)
        {
            var (Base, Normal) = Utilities.RandomBetween(level.RandomPillarSprites);
            return new Pillar(
                new Vector2(
                    cell.TopLeftPosition.X + cell.Width - level.RandomPillarSprites[0].Base.Width / 2f,
                    cell.TopLeftPosition.Y + WallFactory.SouthWallYOffset),
                Base,
                Normal,
                PillarPositions.SE);
        }
    }
}