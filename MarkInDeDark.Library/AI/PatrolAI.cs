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
        protected GameGrid _gameGrid;

        protected readonly List<Vector2> _waypoints;
        protected Queue<Vector2> _pathToCurrentWaypoint;
        protected int _currentWaypointIndex;

        protected Vector2? _currentDestination;
        protected float _currentDestinationStartingDistance;
        protected Vector2 _currentDestinationStart;

        private readonly bool _searchesCandles;

        public PatrolAI(
            List<GameCell> patrolWaypoints,
            GameGrid gameGrid,
            CandlesManager candlesManager,
            bool searchesCandles)
        {
            _candlesManager = candlesManager;
            _waypoints = patrolWaypoints.Select(p => p.CenterPosition).ToList();
            _gameGrid = gameGrid;
            _searchesCandles = searchesCandles;

            // 0 è la sua posizione di partenza
            _currentWaypointIndex = 1;
            _pathToCurrentWaypoint = new Queue<Vector2>();
        }

        public void ClearActions()
        {
            _pathToCurrentWaypoint?.Clear();
            _currentWaypointIndex = 0;
            _currentDestination = null;
        }

        protected Vector2? GetNextDestination(Character character)
        {
            // Se non ho più azioni per il waypoint, le calcolo per il prossimo
            if (_pathToCurrentWaypoint == null || !_pathToCurrentWaypoint.Any())
            {
                _pathToCurrentWaypoint = FindPath(
                    character.WorldCenter, 
                    _waypoints[_currentWaypointIndex]);

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

        protected virtual Queue<Vector2> FindPath(Vector2 from, Vector2 to)
            => new Queue<Vector2>(_gameGrid.FindPathV2(from, to));

        protected virtual bool ReachedDestination(Character character)
            => Vector2.Distance(character.WorldCenter, _currentDestinationStart) >= _currentDestinationStartingDistance;

        public virtual bool IsBlocked(Character character)
        {
            foreach (var w in _waypoints)
            {
                var pathToThisWaypoint = _gameGrid.FindPathV2(
                    character.WorldCenter,
                    w);

                if (pathToThisWaypoint.Count == 0)
                    return true;
            }

            return false;
        }

        protected void SetCurrentDestination(Vector2 destination, Character character)
        {
            _currentDestination = destination;
            _currentDestinationStart = character.WorldCenter;
            _currentDestinationStartingDistance = Vector2.Distance(
                _currentDestinationStart,
                _currentDestination.Value);
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
            var reachedDestination = ReachedDestination(character);
            if (_currentDestination == null || reachedDestination)
            {
                if (reachedDestination && _currentDestination != null)
                {
                    character.MainFigureDrawingInfos.Position = _currentDestination.Value;
                }

                var newDestination = GetNextDestination(character);
                if (newDestination != null)
                {
                    SetCurrentDestination(newDestination.Value, character);
                }
            }
            
            // Se ho un'azione da compiere, la eseguo
            if (_currentDestination != null && !IsBlocked(character))
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

