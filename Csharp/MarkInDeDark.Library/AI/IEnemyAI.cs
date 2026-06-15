using MarkInDeDark.Library.Characters;
using System;

namespace MarkInDeDark.Library.AI
{
    public interface IEnemyAI
    {
        CharacterActions Update(Enemy character, TimeSpan elapsed);
    }
}
