using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MarkInDeDark.Library.AI
{
    public class GoblinAI : PatrolAI
    {
        private readonly Player _player;

        public GoblinAI(
            GameGrid gameGrid,
            Player player)
            : base(new List<GameCell>(), gameGrid, null, false)
        {
            _player = player;
        }

        public override Vector2 GetNextDirection(Enemy character, TimeSpan elapsed)
        {
            _waypoints.Clear();
            _waypoints.Add(_gameGrid.GetCellAtPixel(_player.WorldCenter));
            _currentWaypointIndex = 0;

            return base.GetNextDirection(character, elapsed);
        }

        protected override bool ReachedDestination(Character character)
            => Vector2.Distance(character.WorldCenter, _currentDestination.Value) <= 50f;
    }
}