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

        private readonly List<GameCell> _waypoints;
        private Queue<CharacterActions> _currentActions;

        private int _currentWaypointIndex;
        protected GameGrid _gameGrid;
        protected SoundManager _soundManager;

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
        }

        public void ClearActions()
            => _currentActions.Clear();

        private bool HasActionsToDo
            => _currentActions != null && _currentActions.Any();

        //public virtual CharacterActions Update(
        //    Enemy character,
        //    TimeSpan elapsed)
        //{
        //    // Se non ho niente da fare, cerco la posizione per il prossimo waypoint
        //    if (!HasActionsToDo)
        //    {
        //        _currentActions = new Queue<CharacterActions>(_gameGrid.FindActionsToPathWithoutConsideringWalls(
        //            character.Position,
        //            _waypoints[_currentWaypointIndex].TopLeftPosition));

        //        // Preparo il prossimo waypoint per quando terminerò le azioni del corrente
        //        _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Count;
        //    }

        //    // Se devo ancora terminare la lista di azioni, eseguo la prossima
        //    if (HasActionsToDo)
        //    {
        //        return _currentActions.Dequeue();
        //    }

        //    return CharacterActions.Idle;
        //}

        public Vector2 GetNextDirection(Enemy character, TimeSpan elapsed)
        {
            throw new NotImplementedException();
        }
    }
}
