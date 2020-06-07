using FbonizziMonoGame.Sprites;
using MarkInDeDark.GameGridCommon;
using MarkInDeDark.Library.Maths;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MarkInDeDark.Library.Levels
{
    public static class WallFactory
    {
        public const int NorthWallYOffset = -96;
        public const int SouthWallYOffset = 96;
        public const int EntranceExitDoorYOffset = -96;

        // WARNING Prima usavo IsNorthPassable per gestire queste cose... speriamo bene
        public static List<IWall> GenerateWalls(
            Vector2 cellPosition,
            Cell cellConfig,
            Level level,
            Vector2 cellSize)
        {
            var walls = new List<IWall>();

            if (cellConfig.Top != WallTypes.Niente)
            {
                walls.Add(GenerateTopWall(cellPosition, cellConfig, level));
            }

            if (cellConfig.Bottom != WallTypes.Niente)
            {
                walls.Add(GenerateSouthWall(cellPosition, cellConfig, level));
            }

            if (cellConfig.Left != WallTypes.Niente)
            {
                walls.Add(GenerateWestWall(cellPosition, cellConfig, level));
            }

            if (cellConfig.Right != WallTypes.Niente)
            {
                walls.Add(GenerateEastWall(cellPosition, cellConfig, level, cellSize));
            }

            return walls;
        }

        private static IWall GenerateTopWall(
            Vector2 cellPosition,
            Cell cellConfig,
            Level level)
        {
            var topType = cellConfig.Top;
            var direction = LinkDirection.NorthLink;
            var cellSpriteCenter = level.GroundSpriteType1.Base.SpriteCenter;

            switch (topType)
            {
                case WallTypes.Occludente:
                case WallTypes.Speciale1:
                case WallTypes.Speciale2:
                case WallTypes.Speciale3:
                case WallTypes.Speciale4:
                case WallTypes.Speciale5:
                case WallTypes.Decal1:
                case WallTypes.Decal2:
                case WallTypes.Decal3:
                case WallTypes.Decal4:
                case WallTypes.Decal5:

                    var horizontalWallSprite = Utilities.RandomBetween(level.RandomHorizontalWallSprites);
                    (Sprite Base, Sprite Normal) decal = (null, null);

                    if (topType == WallTypes.Speciale1)
                        horizontalWallSprite = level.WallSpecial1HorizontalSprite;
                    else if (topType == WallTypes.Speciale2)
                        horizontalWallSprite = level.WallSpecial2HorizontalSprite;
                    else if (topType == WallTypes.Speciale3)
                        horizontalWallSprite = level.WallSpecial3HorizontalSprite;
                    else if (topType == WallTypes.Speciale4)
                        horizontalWallSprite = level.WallSpecial4HorizontalSprite;
                    else if (topType == WallTypes.Speciale5)
                        horizontalWallSprite = level.WallSpecial5HorizontalSprite;

                    if (topType == WallTypes.Decal1)
                        decal = level.Decal1;
                    else if (topType == WallTypes.Decal2)
                        decal = level.Decal2;
                    else if (topType == WallTypes.Decal3)
                        decal = level.Decal3;
                    else if (topType == WallTypes.Decal4)
                        decal = level.Decal4;
                    else if (topType == WallTypes.Decal5)
                        decal = level.Decal5;

                    return new WallFixed(
                        direction: direction,
                        position: new Vector2(cellPosition.X, cellPosition.Y + NorthWallYOffset),
                        sprite: horizontalWallSprite.Base,
                        normalSprite: horizontalWallSprite.Normal,
                        decal: decal.Base,
                        decalNormal: decal.Normal);

                case WallTypes.Fragile:
                    return new WallBreakable(
                        direction: direction,
                        position: new Vector2(cellPosition.X, cellPosition.Y + NorthWallYOffset),
                        primarySprite: level.WallHorizontalBreakableSprite.Base,
                        primarySpriteNormal: level.WallHorizontalBreakableSprite.Normal,
                        brokenSprite: level.WallHorizontalBrokenSprite.Base,
                        brokenSpriteNormal: level.WallHorizontalBrokenSprite.Normal);

                case WallTypes.Porta:
                    return new WallInteractiveDoor(
                        doorPosition: new Vector2(cellPosition.X, cellPosition.Y + NorthWallYOffset),
                        linkDirection: LinkDirection.NorthLink,
                        doorOpenAnimation: level.DoorOpenAnimation,
                        doorCloseAnimation: level.DoorCloseAnimation,
                        wallDoorArc: level.WallDoorArcSprite.Base,
                        wallDoorDown: level.WallDoorBaseSprite.Base,
                        wallDoorArcNormal: level.WallDoorArcSprite.Normal,
                        wallDoorDownNormal: level.WallDoorBaseSprite.Normal);

                case WallTypes.Ingresso:
                    return new WallEsteticDoor(
                        wallEsteticDoorType: WallEsteticDoorType.Entrance,
                        direction: direction,
                        doorPosition: new Vector2(cellPosition.X + cellSpriteCenter.X - (level.WallEntranceOpenHorizontalSprite.Base.Width / 2), cellPosition.Y + EntranceExitDoorYOffset),
                        arcPosition: new Vector2(cellPosition.X + cellSpriteCenter.X - (level.WallEntranceArcHorizontalSprite.Base.Width / 2), cellPosition.Y + EntranceExitDoorYOffset),
                        spriteCenterOpen: level.WallEntranceOpenHorizontalSprite.Base,
                        spriteCenterOpenNormal: level.WallEntranceOpenHorizontalSprite.Normal,
                        spriteCenterClosed: level.WallEntranceClosedHorizontalSprite.Base,
                        spriteCenterClosedNormal: level.WallEntranceClosedHorizontalSprite.Normal,
                        spriteArc: level.WallEntranceArcHorizontalSprite.Base,
                        spriteArcNormal: level.WallEntranceArcHorizontalSprite.Normal);

                case WallTypes.Uscita:
                    return new WallEsteticDoor(
                        wallEsteticDoorType: WallEsteticDoorType.Exit,
                        direction: direction,
                        doorPosition: new Vector2(cellPosition.X + cellSpriteCenter.X - (level.WallExitOpenHorizontalSprite.Base.Width / 2), cellPosition.Y + EntranceExitDoorYOffset),
                        arcPosition: new Vector2(cellPosition.X + cellSpriteCenter.X - (level.WallExitArcHorizontalSprite.Base.Width / 2), cellPosition.Y + EntranceExitDoorYOffset),
                        spriteCenterOpen: level.WallExitOpenHorizontalSprite.Base,
                        spriteCenterOpenNormal: level.WallExitOpenHorizontalSprite.Normal,
                        spriteCenterClosed: level.WallExitClosedHorizontalSprite.Base,
                        spriteCenterClosedNormal: level.WallExitClosedHorizontalSprite.Normal,
                        spriteArc: level.WallExitArcHorizontalSprite.Base,
                        spriteArcNormal: level.WallExitArcHorizontalSprite.Normal);
            }

            throw new NotImplementedException($"Muro di tipo non gestito: {topType}");
        }

        private static IWall GenerateSouthWall(
            Vector2 cellPosition,
            Cell cellConfig,
            Level level)
        {
            var bottomType = cellConfig.Bottom;
            var direction = LinkDirection.SouthLink;

            switch (bottomType)
            {
                case WallTypes.Occludente:
                case WallTypes.Speciale1:
                case WallTypes.Speciale2:
                case WallTypes.Speciale3:
                case WallTypes.Speciale4:
                case WallTypes.Speciale5:
                case WallTypes.Decal1:
                case WallTypes.Decal2:
                case WallTypes.Decal3:
                case WallTypes.Decal4:
                case WallTypes.Decal5:

                    var horizontalWallSprite = Utilities.RandomBetween(level.RandomHorizontalWallSprites);
                    (Sprite Base, Sprite Normal) decal = (null, null);

                    if (bottomType == WallTypes.Speciale1)
                        horizontalWallSprite = level.WallSpecial1HorizontalSprite;
                    else if (bottomType == WallTypes.Speciale2)
                        horizontalWallSprite = level.WallSpecial2HorizontalSprite;
                    else if (bottomType == WallTypes.Speciale3)
                        horizontalWallSprite = level.WallSpecial3HorizontalSprite;
                    else if (bottomType == WallTypes.Speciale4)
                        horizontalWallSprite = level.WallSpecial4HorizontalSprite;
                    else if (bottomType == WallTypes.Speciale5)
                        horizontalWallSprite = level.WallSpecial5HorizontalSprite;

                    if (bottomType == WallTypes.Decal1)
                        decal = level.Decal1;
                    else if (bottomType == WallTypes.Decal2)
                        decal = level.Decal2;
                    else if (bottomType == WallTypes.Decal3)
                        decal = level.Decal3;
                    else if (bottomType == WallTypes.Decal4)
                        decal = level.Decal4;
                    else if (bottomType == WallTypes.Decal5)
                        decal = level.Decal5;

                    return new WallFixed(
                        direction: direction,
                        position: new Vector2(cellPosition.X, cellPosition.Y + SouthWallYOffset),
                        sprite: horizontalWallSprite.Base,
                        normalSprite: horizontalWallSprite.Normal,
                        decal: decal.Base,
                        decalNormal: decal.Normal);

                case WallTypes.Fragile:
                    return new WallBreakable(
                        direction: direction,
                        position: new Vector2(cellPosition.X, cellPosition.Y + SouthWallYOffset),
                        primarySprite: level.WallHorizontalBreakableSprite.Base,
                        primarySpriteNormal: level.WallHorizontalBreakableSprite.Normal,
                        brokenSprite: level.WallHorizontalBrokenSprite.Base,
                        brokenSpriteNormal: level.WallHorizontalBrokenSprite.Normal);

                case WallTypes.Porta:
                    return new WallInteractiveDoor(
                        doorPosition: new Vector2(cellPosition.X, cellPosition.Y + SouthWallYOffset),
                        linkDirection: LinkDirection.SouthLink,
                        doorOpenAnimation: level.DoorOpenAnimation,
                        doorCloseAnimation: level.DoorCloseAnimation,
                        wallDoorArc: level.WallDoorArcSprite.Base,
                        wallDoorDown: level.WallDoorBaseSprite.Base,
                        wallDoorArcNormal: level.WallDoorArcSprite.Normal,
                        wallDoorDownNormal: level.WallDoorBaseSprite.Normal);

                case WallTypes.Ingresso:
                    throw new ApplicationException("Porta d'ingresso a sud di una cella non consentita");

                case WallTypes.Uscita:
                    throw new ApplicationException("Porta d'uscita a sud di una cella non consentita");
            }

            throw new NotImplementedException($"Muro di tipo non gestito: {bottomType}");
        }

        private static IWall GenerateEastWall(
            Vector2 cellPosition,
            Cell cellConfig,
            Level level,
            Vector2 cellSize)
        {
            var wallPosition = new Vector2(
                cellPosition.X + cellSize.X - level.RandomVerticalWallSprites[0].Base.Width / 2f,
                cellPosition.Y - 48);

            if (cellConfig.Right == WallTypes.Fragile)
            {
                return new WallBreakable(
                    direction: LinkDirection.EastLink,
                    position: wallPosition,
                    primarySprite: level.WallVerticalBreakableSprite.Base,
                    primarySpriteNormal: level.WallVerticalBreakableSprite.Normal,
                    brokenSprite: level.WallVerticalBrokenASprite.Base,
                    brokenSpriteNormal: level.WallVerticalBrokenASprite.Normal,
                    brokenSpriteB: level.WallVerticalBrokenBSprite.Base,
                    brokenSpriteBNormal: level.WallVerticalBrokenBSprite.Normal);
            }
            else
            {
                var (Base, Normal) = Utilities.RandomBetween(level.RandomVerticalWallSprites);
                return new WallFixed(
                    direction: LinkDirection.EastLink,
                    position: wallPosition,
                    sprite: Base,
                    normalSprite: Normal);
            }
        }

        private static IWall GenerateWestWall(
            Vector2 cellPosition,
            Cell cellConfig,
            Level level)
        {
            var wallPosition = new Vector2(
                cellPosition.X - level.RandomVerticalWallSprites[0].Base.Width / 2f,
                cellPosition.Y - 48);
            
            if (cellConfig.Left == WallTypes.Fragile)
            {
                return new WallBreakable(
                    direction: LinkDirection.WestLink,
                    position: wallPosition,
                    primarySprite: level.WallVerticalBreakableSprite.Base,
                    primarySpriteNormal: level.WallVerticalBreakableSprite.Normal,
                    brokenSprite: level.WallVerticalBrokenASprite.Base,
                    brokenSpriteNormal: level.WallVerticalBrokenASprite.Normal,
                    brokenSpriteB: level.WallVerticalBrokenBSprite.Base,
                    brokenSpriteBNormal: level.WallVerticalBrokenBSprite.Normal);
            }
            else
            {
                var (Base, Normal) = Utilities.RandomBetween(level.RandomVerticalWallSprites);
                return new WallFixed(
                    direction: LinkDirection.WestLink,
                    position: wallPosition,
                    sprite: Base,
                    normalSprite: Normal);
            }

        }
    }

}
