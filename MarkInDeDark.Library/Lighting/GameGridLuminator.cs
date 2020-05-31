using FbonizziMonoGame;
using MarkInDeDark.Library.Levels;
using System;
using System.Collections.Generic;

namespace MarkInDeDark.Library.Lighting
{
    public class GameGridLuminator
    {
        private readonly GameGrid _gameGrid;
        private readonly List<MarkLight> _lights;

        public GameGridLuminator(
            GameGrid gameGrid,
            List<MarkLight> lights)
        {
            _gameGrid = gameGrid;
            _lights = lights;
        }

        public void Luminate()
        {
            LuminateWalls();
            LuminatePillars();
        }

        private void LuminateWalls()
        {
            var allWalls = _gameGrid.AllWalls;
            foreach (var wall in allWalls)
            {
                wall.ObscurationOpacity = 1f; // Tutto oscurato
            }

            foreach (var l in _lights)
            {
                if (l.LightingMode == LightingMode.FivePerFive)
                    LuminateWalls5x5(l);
                else if (l.LightingMode == LightingMode.ThreePerThree)
                    LuminateWalls3x3(l);
            }
        }

        private void LuminateWalls5x5(MarkLight light)
        {
            if (!light.ShouldDraw)
                return;

            // Scelgo quali illuminare
            var lightCell = _gameGrid.GetCellAtPixel(light.Position);
            if (lightCell == null)
                return;

            //var xDelta = lightCell.CenterPosition.X - light.Position.X;


            //var maxDistance = 192f;
            //var positionReference = lightCell.CenterPosition;
            //if (lightCell.NorthCellWithoutLinks != null)
            //{
            //    positionReference = lightCell.NorthCellWithoutLinks.CenterPosition;
            //    maxDistance = 192f * 2f;
            //}

            //if (lightCell.NorthCellWithoutLinks?.NorthCellWithoutLinks != null)
            //{
            //    positionReference = lightCell.NorthCellWithoutLinks.NorthCellWithoutLinks.CenterPosition;
            //    maxDistance = 192f * 3f;
            //}

            //var maxDistance = 192f;
            //var yPositionReference = lightCell.TopLeftPosition.Y;
            //if (lightCell.NorthCellWithoutLinks != null)
            //{
            //    yPositionReference = lightCell.NorthCellWithoutLinks.TopLeftPosition.Y;
            //    maxDistance = 192f * 2f;
            //}

            //if (lightCell.NorthCellWithoutLinks?.NorthCellWithoutLinks != null)
            //{
            //    yPositionReference = lightCell.NorthCellWithoutLinks.NorthCellWithoutLinks.TopLeftPosition.Y;
            //    maxDistance = 192f * 3f;
            //}

            //var yDelta = Vector2.Distance(positionReference, light.Position);

            //var opacity = xDelta < yDelta ? yDelta : xDelta;

            // Scelgo dei due quello più vicino allo zero
            var opacity = 1;
            //   if (xDelta == 0 || (xDelta <= 1))
            //   {
            //       Debug.WriteLine("using y");
            //       opacity = GetOpacityForYDelta(yDelta);
            //   }

            float darkeningOpacityLevel1Walls = 0;
            float darkeningOpacityLevel3Walls = 0;
            float darkeningOpacityLevel2Walls = 0;

            //float darkeningOpacityLevel1Walls = 0;
            //float darkeningOpacityLevel3Walls = opacity;
            //float darkeningOpacityLevel2Walls = opacity / 2f;
            //if (darkeningOpacityLevel2Walls < 0)
            //    darkeningOpacityLevel2Walls = 0;

            //float darkeningOpacityLevel1Walls = 0;
            //float darkeningOpacityLevel3Walls = 0.8f;
            //float darkeningOpacityLevel2Walls = 0.4f;
            //if (darkeningOpacityLevel2Walls < 0)
            //    darkeningOpacityLevel2Walls = 0;

            // Versione 5x5
            // Se c'è subito un muro nord, lo illumino
            // - Le 5 celle che posso creare luce in su

            // 3 su a nord
            LightNorthCellWalls(lightCell, darkeningOpacityLevel1Walls);

            if (lightCell.NorthCellWithoutLinks != null)
                LightNorthCellWalls(lightCell.NorthCellWithoutLinks, darkeningOpacityLevel2Walls);

            if (lightCell.NorthCellWithoutLinks?.NorthCellWithoutLinks != null)
                LightNorthCellWalls(lightCell.NorthCellWithoutLinks.NorthCellWithoutLinks, darkeningOpacityLevel3Walls);

            // 3 su a quella est
            if (lightCell.EastCell != null)
                LightNorthCellWalls(lightCell.EastCell, darkeningOpacityLevel1Walls);

            if (lightCell.EastCell?.NorthCell != null)
                LightNorthCellWalls(lightCell.EastCell.NorthCell, darkeningOpacityLevel2Walls);

            if (lightCell.EastCell?.NorthCell?.NorthCell != null)
                LightNorthCellWalls(lightCell.EastCell?.NorthCell.NorthCell, darkeningOpacityLevel3Walls);

            // 3 su a quella a ovest
            if (lightCell.WestCell != null)
                LightNorthCellWalls(lightCell.WestCell, darkeningOpacityLevel1Walls);

            if (lightCell.WestCell?.NorthCell != null)
                LightNorthCellWalls(lightCell.WestCell.NorthCell, darkeningOpacityLevel2Walls);

            if (lightCell.WestCell?.NorthCell?.NorthCell != null)
                LightNorthCellWalls(lightCell.WestCell?.NorthCell?.NorthCell, darkeningOpacityLevel3Walls);

            //if (!LightNorthCellWalls(lightCell, opacityLevel1Walls))
            //    // Se non trovo ostacoli, vado ancora più su
            //    if (lightCell.NorthCell != null)
            //        if (!LightNorthCellWalls(lightCell.NorthCell, opacityLevel2Walls))
            //            if (lightCell.NorthCell.NorthCell != null)
            //                LightNorthCellWalls(lightCell.NorthCell.NorthCell, opacityLevel3Walls);

            if (lightCell.EastCell != null)
            {
                LightNorthCellWalls(lightCell.EastCell, darkeningOpacityLevel1Walls);
                if (lightCell.EastCell.EastCell != null)
                    LightNorthCellWalls(lightCell.EastCell.EastCell, darkeningOpacityLevel1Walls);
            }

            if (lightCell.WestCell != null)
            {
                LightNorthCellWalls(lightCell.WestCell, darkeningOpacityLevel1Walls);
                if (lightCell.WestCell.WestCell != null)
                    LightNorthCellWalls(lightCell.WestCell.WestCell, darkeningOpacityLevel1Walls);
            }

            if (lightCell.EastCell?.EastCell != null)
                // Se non trovo ostacoli, vado ancora più su
                if (!LightNorthCellWalls(lightCell.EastCell.EastCell, darkeningOpacityLevel1Walls))
                    if (lightCell.EastCell.EastCell.NorthCell != null)
                    {
                        // LightNorthCellWalls(lightCell.EastCell.EastCell.NorthCell);
                    }

            if (lightCell.NorthCell != null)
                if (!LightEastCellWalls(lightCell.NorthCell, darkeningOpacityLevel2Walls))
                    // Se non trovo ostacoli, vado ancora più a destra
                    if (lightCell.NorthCell.EastCell != null)
                        if (!LightEastCellWalls(lightCell.NorthCell.EastCell, darkeningOpacityLevel2Walls))
                            if (lightCell.NorthCell.EastCell.EastCell != null)
                                LightEastCellWalls(lightCell.NorthCell.EastCell.EastCell, darkeningOpacityLevel2Walls);

            if (lightCell.NorthCell?.NorthCell != null)
                if (!LightEastCellWalls(lightCell.NorthCell.NorthCell, darkeningOpacityLevel3Walls))
                    // Se non trovo ostacoli, vado ancora più a destra
                    if (lightCell.NorthCell.NorthCell.EastCell != null)
                        if (!LightEastCellWalls(lightCell.NorthCell.NorthCell.EastCell, darkeningOpacityLevel3Walls))
                            if (lightCell.NorthCell.NorthCell.EastCell.EastCell != null)
                                LightEastCellWalls(lightCell.NorthCell.NorthCell.EastCell.EastCell, darkeningOpacityLevel3Walls);

            if (lightCell.NorthCell != null)
                if (!LightWestCellWalls(lightCell.NorthCell, darkeningOpacityLevel2Walls))
                    // Se non trovo ostacoli, vado ancora più a destra
                    if (lightCell.NorthCell.WestCell != null)
                        if (!LightWestCellWalls(lightCell.NorthCell.WestCell, darkeningOpacityLevel2Walls))
                            if (lightCell.NorthCell.WestCell.WestCell != null)
                                LightWestCellWalls(lightCell.NorthCell.WestCell.WestCell, darkeningOpacityLevel2Walls);

            if (lightCell.NorthCell?.NorthCell != null)
                if (!LightWestCellWalls(lightCell.NorthCell.NorthCell, darkeningOpacityLevel3Walls))
                    // Se non trovo ostacoli, vado ancora più a destra
                    if (lightCell.NorthCell.NorthCell.WestCell != null)
                        if (!LightWestCellWalls(lightCell.NorthCell.NorthCell.WestCell, darkeningOpacityLevel3Walls))
                            if (lightCell.NorthCell.NorthCell.WestCell.WestCell != null)
                                LightWestCellWalls(lightCell.NorthCell.NorthCell.WestCell.WestCell, darkeningOpacityLevel3Walls);
            // - / Le 5 celle che posso creare luce in su

            // Se c'è subito un muro est, lo illumino
            if (!LightEastCellWalls(lightCell, darkeningOpacityLevel1Walls))
                // Se non trovo ostacoli, vado ancora più a destra
                if (lightCell.EastCell != null)
                    if (!LightEastCellWalls(lightCell.EastCell, darkeningOpacityLevel1Walls))
                        if (lightCell.EastCell.EastCell != null)
                            LightEastCellWalls(lightCell.EastCell.EastCell, darkeningOpacityLevel1Walls);

            // Se c'è subito un muro ovest, lo illumino
            if (!LightWestCellWalls(lightCell, darkeningOpacityLevel1Walls))
                // Se non trovo ostacoli, vado ancora più a sinistra
                if (lightCell.WestCell != null)
                    if (!LightWestCellWalls(lightCell.WestCell, darkeningOpacityLevel1Walls))
                        if (lightCell.WestCell.WestCell != null)
                            LightWestCellWalls(lightCell.WestCell.WestCell, darkeningOpacityLevel1Walls);
        }

        private float GetOpacityForXDelta(float xDistance, float maxDistance)
        {
            // TODO Gestire originalOpacity

            var toReturn = Numbers.MapValueFromIntervalToInterval(
                Math.Abs(xDistance),
                0f, maxDistance,
                0f, 1f);

            // if (xDistance < 0)
            //     toReturn = 1 - toReturn;

            if (toReturn > 1f)
                return 1f;

            if (toReturn < 0f)
                return 0f;

            return toReturn;
        }

        // Illumina i muri a nord della cella se ci sono. Se illumina qualcosa ritorna true
        private bool LightNorthCellWalls(GameCell lightCell, float opacity)
        {
            if (lightCell.Walls.ContainsKey(LinkDirection.NorthLink))
            {
                lightCell.Walls[LinkDirection.NorthLink].ObscurationOpacity = opacity;
                return true;
            }
            else
            {
                // Controllo il muro sud nella cella nord
                if (lightCell.NorthCellWithoutLinks?.Walls.ContainsKey(LinkDirection.SouthLink) == true)
                {
                    lightCell.NorthCellWithoutLinks.Walls[LinkDirection.SouthLink].ObscurationOpacity = opacity;
                    return true;
                }
            }

            return false;
        }

        private bool LightEastCellWalls(GameCell lightCell, float opacity)
        {
            if (lightCell.Walls.ContainsKey(LinkDirection.EastLink))
            {
                lightCell.Walls[LinkDirection.EastLink].ObscurationOpacity = opacity;
                return true;
            }
            else
            {
                // Controllo il muro ovest nella cella est
                if (lightCell.EastCellWithoutLinks?.Walls.ContainsKey(LinkDirection.WestLink) == true)
                {
                    lightCell.EastCellWithoutLinks.Walls[LinkDirection.WestLink].ObscurationOpacity = opacity;
                    return true;
                }
                else // else fondamentale, altrimenti illumino muri che dovrebbero bloccare la luce
                     // Controllo il muro nord nella cella est
                {
                    if (lightCell.EastCellWithoutLinks?.Walls.ContainsKey(LinkDirection.NorthLink) == true)
                    {
                        lightCell.EastCellWithoutLinks.Walls[LinkDirection.NorthLink].ObscurationOpacity = opacity;
                        return true;
                    }
                    if (lightCell.EastCellWithoutLinks?.NorthCellWithoutLinks?.Walls.ContainsKey(LinkDirection.SouthLink) == true)
                    {
                        lightCell.EastCellWithoutLinks.NorthCellWithoutLinks.Walls[LinkDirection.SouthLink].ObscurationOpacity = opacity;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool LightWestCellWalls(GameCell lightCell, float opacity)
        {
            if (lightCell.Walls.ContainsKey(LinkDirection.WestLink))
            {
                lightCell.Walls[LinkDirection.WestLink].ObscurationOpacity = opacity;
                return true;
            }
            else
            {
                // Controllo il muro est nella cella ovest
                if (lightCell.WestCellWithoutLinks?.Walls.ContainsKey(LinkDirection.EastLink) == true)
                {
                    lightCell.WestCellWithoutLinks.Walls[LinkDirection.EastLink].ObscurationOpacity = opacity;
                    return true;
                }
                else // else fondamentale, altrimenti illumino muri che dovrebbero bloccare la luce
                {
                    if (lightCell.WestCellWithoutLinks?.Walls.ContainsKey(LinkDirection.NorthLink) == true)
                    {
                        lightCell.WestCellWithoutLinks.Walls[LinkDirection.NorthLink].ObscurationOpacity = opacity;
                        return true;
                    }
                    if (lightCell.WestCellWithoutLinks?.NorthCellWithoutLinks?.Walls.ContainsKey(LinkDirection.SouthLink) == true)
                    {
                        lightCell.WestCellWithoutLinks.NorthCellWithoutLinks.Walls[LinkDirection.SouthLink].ObscurationOpacity = opacity;
                        return true;
                    }
                }
            }

            return false;
        }

        private void LuminateWalls3x3(MarkLight light)
        {
            if (!light.ShouldDraw)
                return;

            // Scelgo quali illuminare
            var lightCell = _gameGrid.GetCellAtPixel(light.Position);
            if (lightCell == null)
                return;

            // Versione 3x3
            // Se c'è subito un muro nord, lo illumino
            if (lightCell.Walls.ContainsKey(LinkDirection.NorthLink))
            {
                lightCell.Walls[LinkDirection.NorthLink].ObscurationOpacity = 0f;
            }
            else
            {
                // Controllo il muro sud nella cella nord
                if (lightCell.NorthCellWithoutLinks?.Walls.ContainsKey(LinkDirection.SouthLink) == true)
                    lightCell.NorthCellWithoutLinks.Walls[LinkDirection.SouthLink].ObscurationOpacity = 0f;
            }

            // Se c'è subito un muro est, lo illumino
            if (lightCell.Walls.ContainsKey(LinkDirection.EastLink))
                lightCell.Walls[LinkDirection.EastLink].ObscurationOpacity = 0f;
            else
            {
                // Controllo il muro ovest nella cella est
                if (lightCell.EastCellWithoutLinks?.Walls.ContainsKey(LinkDirection.WestLink) == true)
                    lightCell.EastCellWithoutLinks.Walls[LinkDirection.WestLink].ObscurationOpacity = 0f;
                else // else fondamentale, altrimenti illumino muri che dovrebbero bloccare la luce
                     // Controllo il muro nord nella cella est
                {
                    if (lightCell.EastCellWithoutLinks?.Walls.ContainsKey(LinkDirection.NorthLink) == true)
                        lightCell.EastCellWithoutLinks.Walls[LinkDirection.NorthLink].ObscurationOpacity = 0f;
                    if (lightCell.EastCellWithoutLinks?.NorthCellWithoutLinks?.Walls.ContainsKey(LinkDirection.SouthLink) == true)
                        lightCell.EastCellWithoutLinks.NorthCellWithoutLinks.Walls[LinkDirection.SouthLink].ObscurationOpacity = 0f;
                }
            }

            // Se c'è subito un muro ovest, lo illumino
            if (lightCell.Walls.ContainsKey(LinkDirection.WestLink))
                lightCell.Walls[LinkDirection.WestLink].ObscurationOpacity = 0f;
            else
            {
                // Controllo il muro est nella cella ovest
                if (lightCell.WestCellWithoutLinks?.Walls.ContainsKey(LinkDirection.EastLink) == true)
                    lightCell.WestCellWithoutLinks.Walls[LinkDirection.EastLink].ObscurationOpacity = 0f;
                else // else fondamentale, altrimenti illumino muri che dovrebbero bloccare la luce
                {
                    if (lightCell.WestCellWithoutLinks?.Walls.ContainsKey(LinkDirection.NorthLink) == true)
                        lightCell.WestCellWithoutLinks.Walls[LinkDirection.NorthLink].ObscurationOpacity = 0f;
                    if (lightCell.WestCellWithoutLinks?.NorthCellWithoutLinks?.Walls.ContainsKey(LinkDirection.SouthLink) == true)
                        lightCell.WestCellWithoutLinks.NorthCellWithoutLinks.Walls[LinkDirection.SouthLink].ObscurationOpacity = 0f;
                }
            }
        }

        private void LuminatePillars()
        {
            // TODO
        }
    }
}
