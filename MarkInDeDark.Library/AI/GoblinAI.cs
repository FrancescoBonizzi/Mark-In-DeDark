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
            Player player,
            CandlesManager candlesManager)
            : base(new List<GameCell>(), gameGrid, candlesManager, true)
        {
            _player = player;
        }

        public override Vector2 GetNextDirection(Enemy character, TimeSpan elapsed)
        {
            _waypoints.Clear();
            _waypoints.Add(_player.WorldCenter);
            _currentWaypointIndex = 0;
            SetCurrentDestination(_player.WorldCenter, character);

            return base.GetNextDirection(character, elapsed);
        }
    }
}