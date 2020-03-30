using Mark.Engine;
using MarkInDeDarkMono.Characters;
using MarkInDeDarkMono.Levels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDarkMono.AI
{
    public class PatrolAI : IEnemyAI
    {
        protected readonly CandlesManager _candlesManager;
        protected readonly Player _player;

        private readonly List<GameCell> _waypoints;
        private Queue<CharacterActions> _currentActions;

        private int _currentWaypointIndex;
        protected GameGrid _gameGrid;
        protected SoundManager _soundManager;

        private readonly bool _searchesCandles;

        public PatrolAI(
            List<GameCell> patrolWaypoints,
            GameGrid gameGrid,
            CandlesManager candlesManager,
            Player player,
            SoundManager soundManager,
            bool searchesCandles)
        {
            _soundManager = soundManager;
            _candlesManager = candlesManager;
            _waypoints = patrolWaypoints;
            _player = player;
            _gameGrid = gameGrid;
            _searchesCandles = searchesCandles;

            // 0 è la sua posizione di partenza
            _currentWaypointIndex = 1;
        }

        public void ClearActions()
            => _currentActions.Clear();

        private bool HasActionsToDo
            => _currentActions != null && _currentActions.Any();

        public virtual CharacterActions Update(
            Enemy character,
            TimeSpan elapsed)
        {
            if (_searchesCandles)
            {
                var actionsToNearCandle = KillCandleAI.SearchForNearCandlePath(character, _candlesManager);
                if (actionsToNearCandle != null)
                {
                    character.SetNewAI(new KillCandleAI(actionsToNearCandle.Candle, actionsToNearCandle.ActionsToGetTheCandle, this));
                    _currentActions = null; // Lo annullo così appena ritorna in questo stato ricalcola i waypoint per tornare dov'era prima
                    return character.CurrentAI.Update(character, elapsed);
                }
            }

            // Se non ho niente da fare, cerco la posizione per il prossimo waypoint
            if (!HasActionsToDo)
            {
                _currentActions = new Queue<CharacterActions>(_gameGrid.FindActionsToPath(
                    character.Position,
                    _waypoints[_currentWaypointIndex].TopLeftPosition));

                // Preparo il prossimo waypoint per quando terminerò le azioni del corrente
                _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Count;
            }

            // Se devo ancora terminare la lista di azioni, eseguo la prossima
            if (HasActionsToDo)
            {
                return _currentActions.Dequeue();
            }

            return CharacterActions.Idle;
        }

    }

}

