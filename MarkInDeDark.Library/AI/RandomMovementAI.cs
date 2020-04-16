using FbonizziMonoGame;
using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Levels;
using System;

namespace MarkInDeDark.Library.AI
{
    /// <summary>
    /// Quest'AI fa continua a far muovere a caso il personaggio 
    /// finché non ha modo di eseguire le proprie azioni dell'AI principale
    /// </summary>
    public class RandomMovementAI : IEnemyAI
    {
        private readonly IEnemyAI _previousAI;
        private readonly CharacterActions[] _possibleActions;

        public RandomMovementAI(IEnemyAI previousAI)
        {
            _previousAI = previousAI;
            _possibleActions = new CharacterActions[]
            {
                CharacterActions.Up,
                CharacterActions.Down,
                CharacterActions.Left,
                CharacterActions.Right
            };
        }

        public CharacterActions Update(Enemy character, TimeSpan elapsed)
        {
            var previousAIDestination = _previousAI.Update(character, elapsed);
            if (previousAIDestination != CharacterActions.Idle)
            {
                character.SetNewAI(_previousAI);
                return previousAIDestination;
            }

            return _possibleActions[Numbers.RandomBetween(0, _possibleActions.Length)];
        }
    }

}
