using MarkInDeDarkMono;
using MarkInDeDarkMono.Characters;
using MarkInDeDarkMono.Levels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDarkMono.AI
{
    public class PlayerHunterAI : IEnemyAI
    {
        private GameCell _oldDestionation;
        private List<CharacterActions> _currentActions;
        protected readonly Player _player;
        protected readonly GameGrid _gameGrid;
        protected readonly CandlesManager _candlesManager;

        public PlayerHunterAI(
            Player player,
            GameGrid gameGrid,
            CandlesManager candlesManager)
        {
            _oldDestionation = null;
            _player = player;
            _gameGrid = gameGrid;
            _candlesManager = candlesManager;
        }

        public virtual CharacterActions Update(
            Enemy character, 
            TimeSpan elapsed)
        {
            var actionsToNearCandle = KillCandleAI.SearchForNearCandlePath(character, _candlesManager);
            if (actionsToNearCandle != null)
            {
                character.SetNewAI(new KillCandleAI(actionsToNearCandle.Candle, actionsToNearCandle.ActionsToGetTheCandle, this));
                _currentActions = null; // Lo annullo così appena ritorna in questo stato ricalcola i waypoint per tornare dov'era prima
                return character.CurrentAI.Update(character, elapsed);
            }

            var destination = _gameGrid.GetCellAtPixel(_player.Position);
            
            if (_oldDestionation != destination || _currentActions == null || !_currentActions.Any())
            {
                _oldDestionation = destination;
                _currentActions = _gameGrid.FindActionsToPath(
                    character.Position, 
                    destination.TopLeftPosition).ToList();
            }

            if (_currentActions != null && _currentActions.Any())
            {
                if (!character.IsMoving) // Altrimenti le scodo tutte subito
                {
                    var nextAction = _currentActions.First();
                    _currentActions.RemoveAt(0);
                    return nextAction;
                }
            }

            return CharacterActions.Idle;
        }

    }
}