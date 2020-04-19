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

        private Vector2? _currentDestination;
        private float _currentDestinationStartingDistance;
        private Vector2 _currentDestinationStart;

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

        private bool ReachedDestination(Character character)
        {
            return Vector2.Distance(character.WorldCenter, _currentDestinationStart) >= _currentDestinationStartingDistance;
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
                        if (_currentDestination == null || ReachedDestination(character))
                        {
                            _currentDestination = _pathToCandle.Dequeue();
                            if (_currentDestination != null)
                            {
                                _currentDestinationStart = character.WorldCenter;
                                _currentDestinationStartingDistance = Vector2.Distance(character.WorldCenter, _currentDestination.Value);
                            }
                        }
                    }

                    if (!_targetCandle.IsActive)
                    {
                        character.SetNewAI(_nextAI);
                        return Vector2.Zero;
                    }

                    if (!anyPathToFollow && ReachedDestination(character))
                    {
                        _currentKillCandleStatus = KillCandleStatus.Smashing;
                        _targetCandle.Die();
                        character.Attack();
                        return Vector2.Zero;
                    }

                    if (_currentDestination != null)
                        return Character.GetDirectionToPosition(character.WorldCenter, _currentDestination.Value);

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
