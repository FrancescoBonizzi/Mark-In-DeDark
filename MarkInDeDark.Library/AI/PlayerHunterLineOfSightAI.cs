
using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.Library.AI
{
    public class PlayerHunterLineOfSightAI : IEnemyAI
    {
        private readonly CandlesManager _candlesManager;
        private readonly Player _player;

        private readonly IEnemyAI _nextAI;
        private Queue<AIMovementAction> _currentActions;

        private readonly GameGrid _gameGrid;
        private GameCell _oldDestionation;

        private AIMovementAction _lastAction;
        private readonly float _originalSpeed;

        private TimeSpan _incredulityPhaseDuration = TimeSpan.FromMilliseconds(500);
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private int _currentIncredulityPhase = 0;

        private enum HuntingStatus
        {
            Following,
            Incredulity,
            Lost
        }

        private HuntingStatus _currentStatus;

        public PlayerHunterLineOfSightAI(
            Enemy skeletron,
            IEnemyAI nextAI,
            GameGrid gameGrid,
            CandlesManager candlesManager,
            Player player,
            SoundManager soundManager,
            GameConfig gameConfig,
            IEnumerable<AIMovementAction> firstPathToPlayer)
        {
            _nextAI = nextAI;
            _player = player;
            _gameGrid = gameGrid;
            _candlesManager = candlesManager;
            _originalSpeed = skeletron.Speed;

            skeletron.SetTemporaryColor(gameConfig.SkeletronRageColorRGB);
            skeletron.Speed = gameConfig.SkeletronHuntingSpeed;
            soundManager.Play("PatrolSeenYou");

            _currentActions = new Queue<AIMovementAction>(firstPathToPlayer);
            _currentStatus = HuntingStatus.Following;
        }

        private bool HasActionsToDo
            => _currentActions != null && _currentActions.Any();

        //public CharacterActions Update(
        //    Enemy enemy,
        //    TimeSpan elapsed)
        //{
        //    switch (_currentStatus)
        //    {
        //        case HuntingStatus.Following:

        //            var actionsToNearCandle = KillCandleAI.SearchForNearCandlePath(enemy, _candlesManager);
        //            if (actionsToNearCandle != null)
        //            {
        //                enemy.SetNewAI(new KillCandleAI(actionsToNearCandle.Candle, actionsToNearCandle.ActionsToGetTheCandle, this));
        //                _currentActions = null; // Lo annullo così appena ritorna in questo stato ricalcola i waypoint per tornare dov'era prima
        //                return enemy.CurrentAI.GetNextDirection(enemy, elapsed);
        //            }

        //            var destinationCell = _gameGrid.GetCellAtPixel(_player.Position);
        //            var enemyCell = _gameGrid.GetCellAtPixel(enemy.Position);

        //            // Se sono sulla stessa cella deve continuare a seguirlo
        //            if (destinationCell == enemyCell && _lastAction != null)
        //            {
        //                _currentActions = new Queue<AIMovementAction>();
        //                _currentActions.Enqueue(_lastAction);
        //            }
        //            // Se il player si è spostato rispetto a prima o se ho finito le azioni da fare...
        //            else if (_oldDestionation != destinationCell || !HasActionsToDo)
        //            {
        //                // ...cerco di capire se riesco comunque a raggiungerlo a vista
        //                var pathToPlayerIfFacingAndSameDirection = PatrolHunterLineOfSightAI.PathToPositionIfFacingAndSameDirection(
        //                    enemy, _player.Position, _gameGrid);

        //                if (pathToPlayerIfFacingAndSameDirection != null)
        //                {
        //                    _oldDestionation = destinationCell;
        //                    _currentActions = new Queue<AIMovementAction>(pathToPlayerIfFacingAndSameDirection);
        //                }
        //            }

        //            // Finchè ho azioni da fare, le eseguo
        //            if (HasActionsToDo)
        //            {
        //                _lastAction = _currentActions.Dequeue();
        //                return _lastAction.Action;
        //            }
        //            else
        //            {
        //                _currentStatus = HuntingStatus.Incredulity;
        //                return CharacterActions.Idle;
        //            }

        //        case HuntingStatus.Incredulity:

        //            if (_elapsedTime <= _incredulityPhaseDuration)
        //            {
        //                _elapsedTime += elapsed;
        //            }
        //            else
        //            {
        //                _elapsedTime = TimeSpan.Zero;
        //                _currentIncredulityPhase++;
        //            }

        //            if (_currentIncredulityPhase == 0)
        //                return CharacterActions.Left;
        //            else if (_currentIncredulityPhase == 1)
        //                return CharacterActions.Right;
        //            if (_currentIncredulityPhase == 2)
        //                return CharacterActions.Left;
        //            else if (_currentIncredulityPhase == 3)
        //                return CharacterActions.Right;
        //            else if (_currentIncredulityPhase == 4)
        //                return CharacterActions.Left;
        //            else if (_currentIncredulityPhase == 5)
        //                return CharacterActions.Right;

        //            _elapsedTime = TimeSpan.Zero;
        //            _currentStatus = HuntingStatus.Lost;
        //            return CharacterActions.Idle;

        //        case HuntingStatus.Lost:

        //            enemy.ResetNormalColor();
        //            enemy.Speed = _originalSpeed;
        //            if (_nextAI is PatrolAI)
        //                ((PatrolAI)_nextAI).ClearActions();
        //            return enemy.SetNewAI(_nextAI);
        //    }

        //    return CharacterActions.Idle;
        //}

        public Vector2 GetNextDirection(Enemy character, TimeSpan elapsed)
        {
            throw new NotImplementedException();
        }

        public bool IsBlocked(Character character)
        {
            throw new NotImplementedException();
        }
    }
}
