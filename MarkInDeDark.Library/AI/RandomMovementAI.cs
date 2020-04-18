using FbonizziMonoGame;
using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MarkInDeDark.Library.AI
{
    /// <summary>
    /// Quest'AI fa continua a far muovere a caso il personaggio 
    /// finché non ha modo di eseguire le proprie azioni dell'AI principale
    /// </summary>
    public class RandomMovementAI : IEnemyAI
    {
        private readonly IEnemyAI _previousAI;
        
        private Vector2? _currentDestination;
        private GameGrid _gameGrid;

        public RandomMovementAI(
            IEnemyAI previousAI,
            GameGrid gameGrid)
        {
            _previousAI = previousAI;
            _gameGrid = gameGrid;
        }

        private bool ReachedDestination(Character character)
        {
            return Vector2.Distance(character.WorldCenter, _currentDestination.Value) <= 2f;
        }

        private Vector2? GetNextDestination(Character character)
        {
            var currentCell = _gameGrid.GetCellAtPixel(character.WorldCenter);

            // Scelgo una cella casualmente intorno a me
            var randomDirection = Numbers.RandomBetween(0, 4);
            if (randomDirection == 0)
                return currentCell.NorthCell?.CenterPosition;
            else if (randomDirection == 1)
                return currentCell.SouthCell?.CenterPosition;
            else if (randomDirection == 2)
                return currentCell.EastCell?.CenterPosition;
            else if (randomDirection == 3)
                return currentCell.WestCell?.CenterPosition;

            return null;
        }

        public Vector2 GetNextDirection(Enemy character, TimeSpan elapsed)
        {
            if (_currentDestination == null || ReachedDestination(character))
            {
                var previousAIDirection = _previousAI.GetNextDirection(character, elapsed);

                // Se può tornare a fare quello che faceva prima, lo fa
                if (previousAIDirection != Vector2.Zero)
                {
                    character.SetNewAI(_previousAI);
                    return previousAIDirection;
                }

                // Altrimenti si muove in destinazioni casuali
                _currentDestination = GetNextDestination(character);
            }

            if (_currentDestination == null)
                return Vector2.Zero;

            return Character.GetDirectionToPosition(character.WorldCenter, _currentDestination.Value);
        }
    }

}
