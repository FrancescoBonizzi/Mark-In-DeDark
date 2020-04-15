using MarkInDeDarkMono.Characters;
using System;

namespace MarkInDeDarkMono.AI
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
