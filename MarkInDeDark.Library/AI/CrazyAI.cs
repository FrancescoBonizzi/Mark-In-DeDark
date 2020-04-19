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
    public class CrazyAI : IEnemyAI
    {
        private readonly IEnemyAI _previousAI;

        private Vector2? _currentDestination;
        private readonly GameGrid _gameGrid;

        public CrazyAI(
            IEnemyAI previousAI,
            GameGrid gameGrid)
        {
            _previousAI = previousAI;
            _gameGrid = gameGrid;
        }

        private bool ReachedDestination(Character character)
        {
            // Qui utilizzo un modo di capire se "sei arrivato" più rozzo 
            // per far muovere il mostro in modo più casuale.
            // Il risultato è che si muove lungo le diagonali perché non ha un punto d'arrivo preciso
            return Vector2.Distance(character.WorldCenter, _currentDestination.Value) <= 50f;
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
                // Se può tornare a fare quello che faceva prima, lo fa
                if (!_previousAI.IsBlocked(character))
                {
                    character.SetNewAI(_previousAI);
                    return Vector2.Zero;
                }

                // Altrimenti si muove in destinazioni casuali
                _currentDestination = GetNextDestination(character);
            }

            if (_currentDestination == null)
                return Vector2.Zero;

            return Character.GetDirectionToPosition(character.WorldCenter, _currentDestination.Value);
        }

        public bool IsBlocked(Character character)
            => false;
    }

}
