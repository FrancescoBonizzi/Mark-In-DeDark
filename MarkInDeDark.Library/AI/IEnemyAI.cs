using MarkInDeDark.Library.Characters;
using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDark.Library.AI
{
    public interface IEnemyAI
    {
        Vector2 GetNextDirection(Enemy character, TimeSpan elapsed);
    }
}
