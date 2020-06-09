using FbonizziMonoGame;
using MarkInDeDark.Library.Characters;
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
        private bool _attackPhase = false;
        private TimeSpan _phaseDuration = TimeSpan.FromMilliseconds(500);
        private TimeSpan _elapsedTime = TimeSpan.Zero;

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
            if (_attackPhase)
            {
                if (_elapsedTime <= _phaseDuration)
                {
                    _elapsedTime += elapsed;
                }
                else
                {
                    _elapsedTime = TimeSpan.Zero;
                    _attackPhase = false;
                }

                character.Attack();
                return CharacterActions.Idle;
            }
            else
            {
                // Ogni volta che arrivo qui il mostro ha raggiunto una cella.
                // quindi switccho sull'attack phase
                _attackPhase = true;

                // Lo faccio solo nella fase in cui dovrebbe muoversi, altrimenti blocco l'attack phase troppo presto
                // perché un waypoint lo trova se è all'interno della stanza e ritorna al PatrolAI,
                // resettando il flag dell'attack phase
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

}
