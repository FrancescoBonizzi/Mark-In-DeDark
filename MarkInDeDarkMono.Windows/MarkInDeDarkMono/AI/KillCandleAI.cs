using Mark.Engine;
using MarkInDeDarkMono.Characters;
using MarkInDeDarkMono.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDarkMono.AI
{
    public class KillCandleAI : IEnemyAI
    {
        private readonly Queue<CharacterActions> _actionsToCandle;
        private readonly IEnemyAI _nextAI;
        private readonly Candle _targetCandle;

        public enum KillCandleStatus
        {
            WalkingToCandle,
            Smashing,
            Ended
        }

        private KillCandleStatus _currentKillCandleStatus;

        public KillCandleAI(
            Candle targetCandle,
            IEnumerable<CharacterActions> actionsToCandle,
            IEnemyAI nextAI)
        {
            _targetCandle = targetCandle;
            targetCandle.AreSomeMonsterIsFollowingMe = true;
            _actionsToCandle = new Queue<CharacterActions>(actionsToCandle);
            _nextAI = nextAI;

            _currentKillCandleStatus = KillCandleStatus.WalkingToCandle;
        }

        public CharacterActions Update(
            Enemy character,
            TimeSpan elapsed)
        {
            switch (_currentKillCandleStatus)
            {
                case KillCandleStatus.WalkingToCandle:

                    bool anyPathToFollow = _actionsToCandle.Any();

                    // .IsActive perché se la candela viene distrutta da qualcun altro,
                    // torno indietro
                    if (anyPathToFollow && _targetCandle.IsActive)
                    {
                        return _actionsToCandle.Dequeue();
                    }
                    else if (!_targetCandle.IsActive)
                    {
                        return character.SetNewAI(_nextAI);
                    }
                    else if (!anyPathToFollow)
                    {
                        _currentKillCandleStatus = KillCandleStatus.Smashing;
                        _targetCandle.Die();
                        character.Attack();
                        return CharacterActions.Idle;
                    }

                    break;

                case KillCandleStatus.Smashing:

                    if (!_targetCandle.IsActive)
                    {
                        character.StopAttacking();
                        return character.SetNewAI(_nextAI);
                    }

                    break;

                case KillCandleStatus.Ended:
                    return character.SetNewAI(_nextAI);
            }

            return CharacterActions.Idle;
        }

        public static SearchForNearCandlePathResult SearchForNearCandlePath(
            Enemy character,
            CandlesManager candlesManager)
        {
            var pathToNearestCandle = candlesManager.GetNearestCandleActionsPath(
                character.Position,
                character.NCellsCandleFindingRange.Value);

            if (pathToNearestCandle.Key == null || !pathToNearestCandle.Value.Any())
                return null;

            return new SearchForNearCandlePathResult(
                pathToNearestCandle.Key,
                pathToNearestCandle.Value);
        }
    }
}
