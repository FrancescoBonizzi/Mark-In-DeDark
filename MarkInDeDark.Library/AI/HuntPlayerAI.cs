using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDark.Library.AI
{
    public class HuntPlayerAI : IEnemyAI
    {
        protected readonly Player _player;
        protected readonly CandlesManager _candlesManager;
        protected readonly GameGrid _gameGrid;

        public HuntPlayerAI(
            GameGrid gameGrid,
            Player player,
            CandlesManager candlesManager)
        {
            _player = player;
            _candlesManager = candlesManager;
            _gameGrid = gameGrid;
        }

        protected Vector2? GetNextDestination(Character character)
        {
            var pathToPlayer = _gameGrid.FindPathOfVectors(character.WorldCenter, _player.WorldCenter);
            if (pathToPlayer.Count > 1)
            {
                // Lo 0 sono io
                return pathToPlayer[1];
            }

            if (pathToPlayer.Count == 0)
            {
                return null;
            }

            return _player.WorldCenter;
        }

        public virtual Vector2 GetNextDirection(Enemy character, TimeSpan elapsed)
        {
            // Ricerca candele...
            var pathToTheNearestCandle = KillCandleAI.SearchForNearCandlePath(character, _candlesManager);
            // ...se c'è una candela nei paraggi...
            if (pathToTheNearestCandle != null)
            {
                // ...passo all'AI di ricerca candele
                character.SetNewAI(new KillCandleAI(pathToTheNearestCandle.Candle, pathToTheNearestCandle.PathToTheCandle, this));
                return character.CurrentAI.GetNextDirection(character, elapsed);
            }

            // Inseguimento del player
            var currentDestination = GetNextDestination(character);
            
            // Se ho un'azione da compiere, la eseguo
            if (currentDestination != null)
            {
                return Character.GetDirectionToPosition(character.WorldCenter, currentDestination.Value);
            }

            // Se non ho niente da fare, e non sono già in modalità RandomMovementAI 
            // faccio muovere a caso il personaggio (se lo continuo a reimpostare vado in StackOverflow)
            if (!(character.CurrentAI is CrazyAI))
            {
                character.SetNewAI(new CrazyAI(this, _gameGrid));
                return character.CurrentAI.GetNextDirection(character, elapsed);
            }

            return Vector2.Zero;
        }

        public bool IsBlocked(Character character)
            => GetNextDestination(character) == null;
    }
}