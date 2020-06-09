using FbonizziMonoGame;
using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDark.Library.Extensions
{
    public static class ZIndexCalculator
    {
        public const float GroundLayerDepth = 0f;
        public const float GroundDirtyLayerDepth = 0.0000001f;
        public const float GroundBloodLayerDepth = 0.0000002f;
        public const float GroundObjectsLayerDepth = 0.0000003f;
        public const float ShadowsLayerDepth = 0.0000004f;
        public const float OverAllLayerDepth = 1f;

        /// <summary>
        /// Le cose più in alto sono disegnate prima
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private static float CalculateDependingOnHeight(float y)
            => Numbers.MapValueFromIntervalToInterval(y, 0, GameGrid.GridMaxHeight, 0f, 1f);

        public static float ForWall(Vector2 position)
            => ForceMax1(CalculateDependingOnHeight(position.Y));

        // I pilastri devono sempre stare sopra ai muri, anche se hanno la stessa Y
        public static float ForPillars(Vector2 position)
            => ForceMax1(CalculateDependingOnHeight(position.Y) + 0.001f);

        // Deve stare sopra ai pilastri
        public static float ForSpecialExternalWall(Vector2 position)
            => CalculateDependingOnHeight(position.Y) + 0.002f;

        public static float ForGenericObject(Vector2 position)
            => ForceMax1(CalculateDependingOnHeight(position.Y));

        // Le candele devono stare sopra agli altri oggetti e i personaggi
        public static float ForCandle(Vector2 position)
            => ForceMax1(CalculateDependingOnHeight(position.Y) + 0.0001f);

        public static float ForShadow()
            => ShadowsLayerDepth;

        public static float ForCharacter(Character character, Vector2 position)
        {
            if (character.IsDead || character.IsDying)
                return ShadowsLayerDepth + 0.0000001f; // Appena sopra le ombre, se è morto

            var startingZIndex = CalculateDependingOnHeight(position.Y);

            switch(character)
            {
                case Player _:
                    startingZIndex += 0.00050f;
                    break;

                case Zombie _:
                    startingZIndex += 0.00051f;
                    break;

                case Ghost _:
                    startingZIndex += 0.00052f;
                    break;

                case Skeletron _:
                    startingZIndex += 0.00053f;
                    break;

                case Goblin _:
                    startingZIndex += 0.00054f;
                    break;

                case Bubu _:
                    startingZIndex += 0.00055f;
                    break;

                case Taurus _:
                    startingZIndex += 0.00056f;
                    break;

                default:
                    throw new NotImplementedException($"Impostare un layer depth per questo personaggio: {character.GetType().Name}");
            }

            return ForceMax1(startingZIndex);
        }

        private static float ForceMax1(float value)
            => value > 1f ? 1f : value;
    }
}
