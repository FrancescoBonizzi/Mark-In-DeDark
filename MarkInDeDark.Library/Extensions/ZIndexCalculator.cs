using FbonizziMonoGame;
using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace MarkInDeDark.Library.Extensions
{
    // TODO Un altro modo furbo per risolvere tutti questi problemi è di creare 
    // dei veri e propri layer di disegno con gli oggetti statici. 
    // Così all'interno del layer disegno Deferred e poi do una depth a tutto il layer e ciao
    public static class ZIndexCalculator
    {
        /// <summary>
        /// Le cose più in alto sono disegnate prima
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private static float CalculateDependingOnHeight(float y)
            => Numbers.MapValueFromIntervalToInterval(y, 0, GameGrid.GridMaxHeight, 0f, 1f);

        public const float GroundLayerDepth = 0f;
        
        // Non si possono sovrapporre
        public static float ForGroundDirty => 0.0000001f;

        private static float _groundBloodLayerDepth = 0.0000001f;
        public static float ForGroundBlood // Ce ne stanno 9
        {
            // Funzione perché si possono sovrapporre
            get => ForceMax1(_groundBloodLayerDepth += 0.0000001f);
        }

        // Non si possono sovrapporre
        public static float ForGroundObjects => 0.000002f;

        public static float ForWall(Vector2 position)
            => ForceMax1(CalculateDependingOnHeight(position.Y));

        // I pilastri devono sempre stare sopra ai muri, anche se hanno la stessa Y
        public static float ForPillars(Vector2 position)
            => ForceMax1(CalculateDependingOnHeight(position.Y) + 0.001f);

        // Sopra a tutti i personaggi, ma sotto i pilastri
        public static float ForDoorArc(Vector2 position)
            => 1f;

        public static float ForSpecialExternalWall(Vector2 position)
            => CalculateDependingOnHeight(position.Y) + 0.00099f;

        public static float ForGenericObject(Vector2 position)
            => ForceMax1(CalculateDependingOnHeight(position.Y));

        // Le candele devono stare sopra agli altri oggetti
        public static float ForCandle(Vector2 position)
            => ForceMax1(CalculateDependingOnHeight(position.Y) + 0.0001f);

        public static float ForCharacter(Character character, Vector2 position)
        {
            if (character.IsDead || character.IsDying)
                return ForGroundBlood; // Appena sopra al sangue, se è morto

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
