using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.Library.AI
{
    public class PatrolAI : IEnemyAI
    {
        protected readonly CandlesManager _candlesManager;
        protected readonly Player _player;
        protected GameGrid _gameGrid;
        protected SoundManager _soundManager;

        private readonly List<GameCell> _waypoints;
        private Queue<Vector2> _pathToCurrentWaypoint;
        private int _currentWaypointIndex;
        private Vector2? _currentDestination;
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
            _pathToCurrentWaypoint = new Queue<Vector2>();
        }

        public void ClearActions()
            => _pathToCurrentWaypoint?.Clear();

        private Vector2? GetNextDestination(Character character)
        {
            // Se non ho più azioni per il waypoint, le calcolo per il prossimo
            if (_pathToCurrentWaypoint == null || !_pathToCurrentWaypoint.Any())
            {
                _pathToCurrentWaypoint = new Queue<Vector2>(_gameGrid.FindPathV2(
                    character.WorldCenter,
                    _waypoints[_currentWaypointIndex].CenterPosition));

                // Preparo il prossimo waypoint per quando terminerò le azioni del corrente
                _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Count;
            }

            // Se non ho un'azione corrente da eseguire, ma ho la lista pronta, ne scodo una
            if (_pathToCurrentWaypoint.Any())
            {
                return _pathToCurrentWaypoint.Dequeue();
            }

            return null;
        }

        private bool ReachedDestination(Character character)
        {
            return Vector2.Distance(character.WorldCenter, _currentDestination.Value) <= 2f;
        }

        public virtual Vector2 GetNextDirection(
            Enemy character,
            TimeSpan elapsed)
        {
            // Se deve valutare se prendere le candele, lo valuto
            //if (_searchesCandles)
            //{
            //    var actionsToNearCandle = KillCandleAI.SearchForNearCandlePath(character, _candlesManager);
            //    if (actionsToNearCandle != null)
            //    {
            //        // Passo all'AI di ricerca candele
            //        character.SetNewAI(new KillCandleAI(actionsToNearCandle.Candle, actionsToNearCandle.ActionsToGetTheCandle, this));
            //        _actionsToCurrentWaypoint = null; // Lo annullo così appena ritorna in questo stato ricalcola i waypoint per tornare dov'era prima
            //        return character.CurrentAI.Update(character, elapsed);
            //    }
            //}

            // Se non ho azioni o ho raggiunto la destinazione, passo alla successiva
            if (_currentDestination == null || ReachedDestination(character))
            {
                _currentDestination = GetNextDestination(character);
            }

            // Se ho un'azione da compiere, la eseguo
            if (_currentDestination != null)
            {
                return Character.GetDirectionToPosition(character.WorldCenter, _currentDestination.Value);
            }

            // Se non ho niente da fare, e non sono già in modalità RandomMovementAI 
            // faccio muovere a caso il personaggio (se lo continuo a reimpostare vado in StackOverflow)
            if (!(character.CurrentAI is RandomMovementAI))
            {
                character.SetNewAI(new RandomMovementAI(this, _gameGrid));
                return character.CurrentAI.GetNextDirection(character, elapsed);
            }

            return Vector2.Zero;
        }

    }

}

