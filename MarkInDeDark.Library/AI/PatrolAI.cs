using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private float _currentDestinationStartingDistance;
        private Vector2 _currentDestinationStart;

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
            return Vector2.Distance(character.WorldCenter, _currentDestinationStart) >= _currentDestinationStartingDistance;
        }

        public bool IsBlocked(Character character)
        {
            foreach (var w in _waypoints)
            {
                var pathToThisWaypoint = _gameGrid.FindPathV2(
                    character.WorldCenter,
                    w.CenterPosition);

                if (pathToThisWaypoint.Count == 0)
                    return true;
            }

            return false;
        }

        public virtual Vector2 GetNextDirection(
            Enemy character,
            TimeSpan elapsed)
        {
            // Se è un mostro cercatore di candele...
            if (_searchesCandles)
            {
                var pathToTheNearestCandle = KillCandleAI.SearchForNearCandlePath(character, _candlesManager);
                // ...e c'è una candela nei paraggi...
                if (pathToTheNearestCandle != null)
                {
                    // ...passo all'AI di ricerca candele
                    character.SetNewAI(new KillCandleAI(pathToTheNearestCandle.Candle, pathToTheNearestCandle.PathToTheCandle, this));

                    // Lo annullo così appena ritorna in questo stato ricalcola i waypoint per tornare dov'era prima
                    _currentDestination = null;
                    _pathToCurrentWaypoint = null;

                    return character.CurrentAI.GetNextDirection(character, elapsed);
                }
            }

            // Se non ho azioni o ho raggiunto la destinazione, passo alla successiva
            if (_currentDestination == null || ReachedDestination(character))
            {
                _currentDestination = GetNextDestination(character);
                if (_currentDestination != null)
                {
                    _currentDestinationStart = character.WorldCenter;
                    _currentDestinationStartingDistance = Vector2.Distance(character.WorldCenter, _currentDestination.Value);
                }
            }

            // Se ho un'azione da compiere, la eseguo
            if (_currentDestination != null && _pathToCurrentWaypoint.Count != 0 && !IsBlocked(character))
            {
                return Character.GetDirectionToPosition(character.WorldCenter, _currentDestination.Value);
            }

            // Se non ho niente da fare, e non sono già in modalità RandomMovementAI 
            // faccio muovere a caso il personaggio (se lo continuo a reimpostare vado in StackOverflow)
            if (!(character.CurrentAI is CrazyAI))
            {
                _currentDestination = null;
                _pathToCurrentWaypoint = null;
                character.SetNewAI(new CrazyAI(this, _gameGrid));
                return character.CurrentAI.GetNextDirection(character, elapsed);
            }

            return Vector2.Zero;
        }

    }

}

