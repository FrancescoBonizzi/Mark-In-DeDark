using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MarkInDeDark.Library.AI
{
    public class PatrolGhostAI : PatrolAI
    {
        public PatrolGhostAI(
            List<GameCell> patrolWaypoints,
            GameGrid gameGrid,
            Player player)
            : base(patrolWaypoints, gameGrid, null, player, false)
        {

        }

        protected override Queue<Vector2> FindPath(Vector2 from, Vector2 to)
            => new Queue<Vector2>(_gameGrid.FindPathWithoutConsideringWalls(from, to));

        public override bool IsBlocked(Character character)
            => false;
    }
}
