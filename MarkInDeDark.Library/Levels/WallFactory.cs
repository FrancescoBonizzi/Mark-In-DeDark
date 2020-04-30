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
        // WARNING Prima usavo IsNorthPassable per gestire queste cose... speriamo bene
        public static List<IWall> GenerateWalls(
            Vector2 cellPosition,
            Cell cellConfig,
            Level level,
            AssetsLoader assetsLoader,
            Vector2 cellSize,
            Action doorActionOnClose)
        {
            var walls = new List<IWall>();

            if (cellConfig.Top != WallTypes.Niente)
            {
                walls.Add(GenerateTopWall(cellPosition, cellConfig, level, assetsLoader, doorActionOnClose));
            }

            if (cellConfig.Bottom != WallTypes.Niente)
            {
                walls.Add(GenerateSouthWall(cellPosition, cellConfig, level, assetsLoader, doorActionOnClose));
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
            Level level,
            AssetsLoader assetsLoader,
            Action doorActionOnClose)
        {
            var topType = cellConfig.Top;
            var direction = LinkDirection.NorthLink;

            switch (topType)
            {
                case WallTypes.Occludente:
                case WallTypes.Speciale1:
                case WallTypes.Decal1:
                case WallTypes.Decal2:
                case WallTypes.Decal3:
                case WallTypes.Decal4:
                case WallTypes.Decal5:

                    Sprite horizontalWallSprite = Utilities.RandomBetween(level.RandomHorizontalWallSprites);
                    Sprite decal = null;

                    if (topType == WallTypes.Fragile)
                        horizontalWallSprite = level.WallHorizontalDestroyableSprite;
                    else if (topType == WallTypes.Speciale1)
                        horizontalWallSprite = level.WallSpecialHorizontalSprite;

                    if (topType == WallTypes.Decal1)
                        decal = assetsLoader.Sprites["Grounds/Decals/decal_1"];
                    else if (topType == WallTypes.Decal2)
                        decal = assetsLoader.Sprites["Grounds/Decals/decal_2"];
                    else if (topType == WallTypes.Decal3)
                        decal = assetsLoader.Sprites["Grounds/Decals/decal_3"];
                    else if (topType == WallTypes.Decal4)
                        decal = assetsLoader.Sprites["Grounds/Decals/decal_4"];
                    else if (topType == WallTypes.Decal5)
                        decal = assetsLoader.Sprites["Grounds/Decals/decal_5"];

                    return new WallFixed(
                        direction: direction,
                        position: new Vector2(cellPosition.X, cellPosition.Y - horizontalWallSprite.Height + 78f),
                        sprite: horizontalWallSprite,
                        decal: decal);

                case WallTypes.Fragile:
                    return new WallBreakable(
                        direction: direction,
                        position: new Vector2(cellPosition.X, cellPosition.Y - level.WallHorizontalDestroyableSprite.Height + 78f),
                        primarySprite: level.WallHorizontalDestroyableSprite,
                        brokenSprite: level.WallHorizontalBrokenSprite);

                case WallTypes.Porta:
                    return new WallInteractiveDoor(
                        doorPosition: new Vector2(cellPosition.X, cellPosition.Y - level.WallDoorBaseSprite.Height + 80),
                        linkDirection: LinkDirection.NorthLink,
                        doorOpenAnimation: level.DoorOpenAnimation,
                        doorCloseAnimation: level.DoorCloseAnimation,
                        wallDoorArc: level.WallDoorArcSprite,
                        wallDoorDown: level.WallDoorBaseSprite,
                        onClose: () => doorActionOnClose());

                case WallTypes.Ingresso:
#warning TODO La posizione è da configurare
                    return new WallEsteticDoor(
                        direction: direction,
                        position: new Vector2(cellPosition.X, cellPosition.Y - level.WallDoorBaseSprite.Height + 80),
                        spriteCenterOpen: level.WallEntranceOpenHorizontalSprite,
                        spriteCenterClosed: level.WallEntranceClosedHorizontalSprite,
                        spriteArc: level.WallEntranceArcHorizontalSprite);

                case WallTypes.Uscita:
#warning TODO La posizione è da configurare
                    return new WallEsteticDoor(
                        direction: direction,
                        position: new Vector2(cellPosition.X, cellPosition.Y - level.WallDoorBaseSprite.Height + 80),
                        spriteCenterOpen: level.WallExitOpenHorizontalSprite,
                        spriteCenterClosed: level.WallExitClosedHorizontalSprite,
                        spriteArc: level.WallExitArcHorizontalSprite);
            }

            throw new NotImplementedException($"Muro di tipo non gestito: {topType}");
        }

        /*
                     // TODO da mettere nel WallFactory
            int shakeIntensity = 3;

            if (IsNorthDoor)
            {
               
            }

            if (IsSouthDoor)
            {
                Doors.Add(LinkDirection.SouthLink, new WallInteractiveDoor(
                    doorPosition: new Vector2(_cellDrawingInfos.Position.X, _cellDrawingInfos.Position.Y + _height - 80),
                    linkDirection: LinkDirection.SouthLink,
                    doorOpenAnimation: levelConfig.DoorOpenAnimation,
                    doorCloseAnimation: levelConfig.DoorCloseAnimation,
                    wallDoorArc: _wallDoorArc,
                    wallDoorDown: _wallDoorDown,
                    onClose: () => _game.StartCameraShake(shakeIntensity)));
            }
            */

        private static IWall GenerateSouthWall(
            Vector2 cellPosition,
            Cell cellConfig,
            Level level,
            AssetsLoader assetsLoader,
            Action doorActionOnClose)
        {
            var bottomType = cellConfig.Bottom;
            var direction = LinkDirection.NorthLink;

            switch (bottomType)
            {
                case WallTypes.Occludente:
                case WallTypes.Speciale1:
                case WallTypes.Decal1:
                case WallTypes.Decal2:
                case WallTypes.Decal3:
                case WallTypes.Decal4:
                case WallTypes.Decal5:

                    Sprite horizontalWallSprite = Utilities.RandomBetween(level.RandomHorizontalWallSprites);
                    Sprite decal = null;

                    if (bottomType == WallTypes.Fragile)
                        horizontalWallSprite = level.WallHorizontalDestroyableSprite;
                    else if (bottomType == WallTypes.Speciale1)
                        horizontalWallSprite = level.WallSpecialHorizontalSprite;

                    if (bottomType == WallTypes.Decal1)
                        decal = assetsLoader.Sprites["Grounds/Decals/decal_1"];
                    else if (bottomType == WallTypes.Decal2)
                        decal = assetsLoader.Sprites["Grounds/Decals/decal_2"];
                    else if (bottomType == WallTypes.Decal3)
                        decal = assetsLoader.Sprites["Grounds/Decals/decal_3"];
                    else if (bottomType == WallTypes.Decal4)
                        decal = assetsLoader.Sprites["Grounds/Decals/decal_4"];
                    else if (bottomType == WallTypes.Decal5)
                        decal = assetsLoader.Sprites["Grounds/Decals/decal_5"];

                    return new WallFixed(
                        direction,
                        new Vector2(cellPosition.X, cellPosition.Y - horizontalWallSprite.Height + 115f),
                        horizontalWallSprite,
                        decal);

                case WallTypes.Fragile:
                    return new WallBreakable(
                        direction: direction,
                        position: new Vector2(cellPosition.X, cellPosition.Y - level.WallHorizontalDestroyableSprite.Height + 115f),
                        primarySprite: level.WallHorizontalDestroyableSprite,
                        brokenSprite: level.WallHorizontalBrokenSprite);

                case WallTypes.Porta:
                    return new WallInteractiveDoor(
                        doorPosition: new Vector2(cellPosition.X, cellPosition.Y + level.WallDoorBaseSprite.Height - 80),
                        linkDirection: LinkDirection.NorthLink,
                        doorOpenAnimation: level.DoorOpenAnimation,
                        doorCloseAnimation: level.DoorCloseAnimation,
                        wallDoorArc: level.WallDoorArcSprite,
                        wallDoorDown: level.WallDoorBaseSprite,
                        onClose: () => doorActionOnClose());

                case WallTypes.Ingresso:
#warning TODO La posizione è da configurare
                    return new WallEsteticDoor(
                        direction: direction,
                        position: new Vector2(cellPosition.X, cellPosition.Y),
                        spriteCenterOpen: level.WallEntranceOpenHorizontalSprite,
                        spriteCenterClosed: level.WallEntranceClosedHorizontalSprite,
                        spriteArc: level.WallEntranceArcHorizontalSprite);

                case WallTypes.Uscita:
#warning TODO La posizione è da configurare
                    return new WallEsteticDoor(
                        direction: direction,
                        position: new Vector2(cellPosition.X, cellPosition.Y),
                        spriteCenterOpen: level.WallExitOpenHorizontalSprite,
                        spriteCenterClosed: level.WallExitClosedHorizontalSprite,
                        spriteArc: level.WallExitArcHorizontalSprite);
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
                cellPosition.X + cellSize.X - level.RandomVerticalWallSprites[0].Width / 2f, 
                cellPosition.Y - 30);

            if (cellConfig.Right == WallTypes.Fragile)
            {
                return new WallBreakable(
                    direction: LinkDirection.EastLink,
                    position: wallPosition,
                    primarySprite: level.WallVerticalDestroyableSprite,
                    brokenSprite: level.WallVerticalBrokenASprite,
                    brokenSpriteB: level.WallVerticalBrokenBSprite);
            }
            else
            {
                return new WallFixed(
                    direction: LinkDirection.EastLink,
                    position: wallPosition,
                    sprite: Utilities.RandomBetween(level.RandomVerticalWallSprites));
            }
        }

        private static IWall GenerateWestWall(
            Vector2 cellPosition,
            Cell cellConfig,
            Level level)
        {
            var wallPosition = new Vector2(
                cellPosition.X - level.RandomVerticalWallSprites[0].Width / 2f, 
                cellPosition.Y - 30);

            if (cellConfig.Left == WallTypes.Fragile)
            {
                return new WallBreakable(
                    direction: LinkDirection.EastLink,
                    position: wallPosition,
                    primarySprite: level.WallVerticalDestroyableSprite,
                    brokenSprite: level.WallVerticalBrokenASprite,
                    brokenSpriteB: level.WallVerticalBrokenBSprite);
            }
            else
            {
                return new WallFixed(
                    direction: LinkDirection.EastLink,
                    position: wallPosition,
                    sprite: Utilities.RandomBetween(level.RandomVerticalWallSprites));
            }

        }
    }

}
