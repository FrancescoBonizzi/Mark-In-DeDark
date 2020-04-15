using MarkInDeDarkMono.Characters;
using System;

namespace MarkInDeDarkMono.AI
{
    public interface IEnemyAI
    {
        CharacterActions Update(Enemy character, TimeSpan elapsed);
    }
}
