using MarkInDeDark.Library.Characters;
using System;

namespace MarkInDeDark.Library.AI
{
    public class IdleAI : IEnemyAI
    {
        public CharacterActions Update(
            Enemy character, 
            TimeSpan elapsed)
        {
            return CharacterActions.Idle;
        }
    }
}
