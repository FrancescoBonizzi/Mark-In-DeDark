using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDark.Library.AI
{
    public class TaurusChargingAI : IEnemyAI
    {
        private readonly GameGrid _gameGrid;

        private readonly CharacterActions _directionAction;
        private readonly Vector2 _directionVector;

        private readonly float _normalSpeed;
        private readonly IEnemyAI _nextAI;

        private readonly Taurus _taurus;
        private readonly CandlesManager _candlesManager;
        private readonly MarkInDeDarkGame _game;

        private TimeSpan _chargeTimeToStartSeconds;
        private TimeSpan _stunTimeSeconds;
        private TimeSpan _elapsedTime = TimeSpan.Zero;

        private ChargeStatus _currentChargeStatus;

        private enum ChargeStatus
        {
            Starting,
            Charge,
            Stunned
        }

        public TaurusChargingAI(
            CharacterActions directionAction,
            GameGrid gameGrid,
            Taurus taurus,
            SoundManager soundManager,
            CandlesManager candlesManager,
            IEnemyAI nextAI,
            MarkInDeDarkGame game)
        {
            _gameGrid = gameGrid;
            _directionAction = directionAction;

            _normalSpeed = taurus.Speed;
            taurus.Speed = taurus.TaurusChargeSpeed;
            _currentChargeStatus = ChargeStatus.Starting;
            _candlesManager = candlesManager;
            _game = game;

            soundManager.Play("TaurusStartingCharge");

            _chargeTimeToStartSeconds = TimeSpan.FromSeconds(taurus.TaurusTimeToStartSeconds);
            _stunTimeSeconds = TimeSpan.FromSeconds(taurus.TaurusTimeStunnedSeconds);
            taurus.SetTemporaryColor(taurus.TaurusChargeColorRGB);
            _nextAI = nextAI;
            _taurus = taurus;
            _taurus.DontPlayDefaultAnimations = true;

            if (directionAction == CharacterActions.Up)
                _taurus.MainAnimationsAnimationManager.PlayAnimation("TaurusChargeBack");
            else if (directionAction == CharacterActions.Right || directionAction == CharacterActions.Left)
                _taurus.MainAnimationsAnimationManager.PlayAnimation("TaurusChargeSide");
            else if (directionAction == CharacterActions.Down)
                _taurus.MainAnimationsAnimationManager.PlayAnimation("TaurusChargeFront");

            switch (directionAction)
            {
                case CharacterActions.Up:
                    _directionVector = new Vector2(0f, -1f);
                    break;

                case CharacterActions.Down:
                    _directionVector = new Vector2(0f, 1f);
                    break;

                case CharacterActions.Right:
                    _directionVector = new Vector2(1f, 0f);
                    break;

                case CharacterActions.Left:
                    _directionVector = new Vector2(-1f, 0f);
                    break;
            }
        }

        public Vector2 GetNextDirection(Enemy enemy, TimeSpan elapsed)
        {
            switch (_currentChargeStatus)
            {
                case ChargeStatus.Starting:

                    if (_elapsedTime < _chargeTimeToStartSeconds)
                    {
                        _elapsedTime += elapsed;
                        enemy.SetAction(_directionAction);
                        return Vector2.Zero;
                    }
                    else
                    {
                        _elapsedTime = TimeSpan.Zero;
                        _currentChargeStatus = ChargeStatus.Charge;

                        if (_directionAction == CharacterActions.Up)
                            _taurus.MainAnimationsAnimationManager.PlayAnimation("TaurusRunBack");
                        else if (_directionAction == CharacterActions.Right || _directionAction == CharacterActions.Left)
                            _taurus.MainAnimationsAnimationManager.PlayAnimation("TaurusRunSide");
                        else if (_directionAction == CharacterActions.Down)
                            _taurus.MainAnimationsAnimationManager.PlayAnimation("TaurusRunFront");
                    }

                    break;

                case ChargeStatus.Charge:

                    var currentCell = _gameGrid.GetCellAtPixel(enemy.Position);

                    // Se c'è una candela lungo la strada, la spengo:
                    var possibleCandle = _candlesManager.IsThereACandle(currentCell);
                    if (possibleCandle != null)
                    {
                        possibleCandle.DieFast();
                    }

                    // Se posso andare avanti vado avanti caricando
                    if (!_gameGrid.WillCollideWithSomethingNotPassable(
                        enemy.FutureCollisionRectangle,
                        _directionVector))
                    {
                        _game.StartCameraShake(5, TimeSpan.FromSeconds(1));
                        return _directionVector;
                    }
                    else
                    {
                        // Spacco il muro
                        var breakWallEffectLocation = enemy.WorldCenter;
                        switch (_directionAction)
                        {
                            case CharacterActions.Up:
                                breakWallEffectLocation.Y -= enemy.Height / 2;
                                break;
                            case CharacterActions.Down:
                                breakWallEffectLocation.Y += enemy.Height / 2;
                                break;
                            case CharacterActions.Right:
                                breakWallEffectLocation.X += enemy.Width / 2;
                                break;
                            case CharacterActions.Left:
                                breakWallEffectLocation.X -= enemy.Width / 2;
                                break;
                        }

                        _gameGrid.BreakWall(
                            currentCell,
                            _directionAction,
                            breakWallEffectLocation);

                        _currentChargeStatus = ChargeStatus.Stunned;
                        _taurus.DontPlayDefaultAnimations = false;
                    }

                    break;

                case ChargeStatus.Stunned:

                    if (_elapsedTime < _stunTimeSeconds)
                    {
                        _elapsedTime += elapsed;
                        return Vector2.Zero;
                    }
                    else
                    {
                        // Resetto la velocità
                        enemy.Speed = _normalSpeed;

                        // Ritorno a fare quello che facevo prima
                        enemy.ResetNormalColor();
                        if (_nextAI is PatrolAI)
                            ((PatrolAI)_nextAI).ClearActions();
                        enemy.SetNewAI(_nextAI);
                        return Vector2.Zero;
                    }
            }

            return _directionVector;
        }

        public bool IsBlocked(Character character)
            => false;
    }
}