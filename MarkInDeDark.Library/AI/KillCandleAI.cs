using MarkInDeDark.Library.Characters;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.Library.AI
{
    public class KillCandleAI : IEnemyAI
    {
        private readonly Queue<Vector2> _pathToCandle;
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
            List<Vector2> actionsToCandle,
            IEnemyAI nextAI)
        {
            _targetCandle = targetCandle;
            targetCandle.AreSomeMonsterIsFollowingMe = true;
            _pathToCandle = new Queue<Vector2>(actionsToCandle);
            _nextAI = nextAI;

            _currentKillCandleStatus = KillCandleStatus.WalkingToCandle;
        }

        public virtual Vector2 GetNextDirection(
            Enemy character,
            TimeSpan elapsed)
        {
            switch (_currentKillCandleStatus)
            {
                case KillCandleStatus.WalkingToCandle:

                    bool anyPathToFollow = _pathToCandle.Any();

                    // .IsActive perché se la candela viene distrutta da qualcun altro,
                    // torno indietro
                    if (anyPathToFollow && _targetCandle.IsActive)
                    {
                        return _pathToCandle.Dequeue();
                    }
                    else if (!_targetCandle.IsActive)
                    {
                        character.SetNewAI(_nextAI);
                        return Vector2.Zero;
                    }
                    else if (!anyPathToFollow)
                    {
                        _currentKillCandleStatus = KillCandleStatus.Smashing;
                        _targetCandle.Die();
                        character.Attack();
                        return Vector2.Zero;
                    }

                    break;

                case KillCandleStatus.Smashing:

                    if (!_targetCandle.IsActive)
                    {
                        character.StopAttacking();
                        character.SetNewAI(_nextAI);
                        return Vector2.Zero;
                    }

                    break;

                case KillCandleStatus.Ended:
                    character.SetNewAI(_nextAI);
                    return Vector2.Zero;
            }

            return Vector2.Zero;
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

        public bool IsBlocked(Character character)
            => false;
    }
}
