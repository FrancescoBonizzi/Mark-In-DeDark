using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.Library.AI
{
    public class PatrolGhostAI : IEnemyAI
    {
        protected readonly Player _player;
        protected GameGrid _gameGrid;
        protected SoundManager _soundManager;

        private readonly List<GameCell> _waypoints;
        private Queue<Vector2> _pathToCurrentWaypoint;
        private int _currentWaypointIndex;

        private Vector2? _currentDestination;
        private float _currentDestinationStartingDistance;
        private Vector2 _currentDestinationStart;

        public PatrolGhostAI(
            List<GameCell> patrolWaypoints,
            GameGrid gameGrid,
            Player player,
            SoundManager soundManager)
        {
            _soundManager = soundManager;
            _waypoints = patrolWaypoints;
            _player = player;
            _gameGrid = gameGrid;

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
                _pathToCurrentWaypoint = new Queue<Vector2>(_gameGrid.FindPathWithoutConsideringWalls(
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

        // Il ghost non è mai bloccato
        public bool IsBlocked(Character character)
            => false;

        public virtual Vector2 GetNextDirection(
            Enemy character,
            TimeSpan elapsed)
        {
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
            if (_currentDestination != null)
            {
                return Character.GetDirectionToPosition(character.WorldCenter, _currentDestination.Value);
            }

            return Vector2.Zero;
        }

    }
}
