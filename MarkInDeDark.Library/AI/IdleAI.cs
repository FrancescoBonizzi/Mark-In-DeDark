using MarkInDeDark.Library.Characters;
using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDark.Library.AI
{
    public class IdleAI : IEnemyAI
    {
        public Vector2 GetNextDirection(Enemy character, TimeSpan elapsed)
            => Vector2.Zero;
    }
}
