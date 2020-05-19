using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Levels;
using System;

namespace MarkInDeDark.Library.AI
{
    public class ChargeDirectionAI : IEnemyAI
    {
        private readonly GameGrid _gameGrid;
        private readonly CharacterActions _direction;
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

        public ChargeDirectionAI(
            CharacterActions direction,
            GameGrid gameGrid,
            Taurus taurus,
            SoundManager soundManager,
            CandlesManager candlesManager,
            IEnemyAI nextAI,
            MarkInDeDarkGame game)
        {
            _gameGrid = gameGrid;
            _direction = direction;
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

            if (direction == CharacterActions.Up)
                _taurus.MainAnimationsAnimationManager.PlayAnimation("TaurusChargeBack");
            else if (direction == CharacterActions.Right || direction == CharacterActions.Left)
                _taurus.MainAnimationsAnimationManager.PlayAnimation("TaurusChargeSide");
            else if (direction == CharacterActions.Down)
                _taurus.MainAnimationsAnimationManager.PlayAnimation("TaurusChargeFront");
        }

        public CharacterActions Update(
            Enemy character,
            TimeSpan elapsed)
        {
            switch (_currentChargeStatus)
            {
                case ChargeStatus.Starting:

                    if (_elapsedTime < _chargeTimeToStartSeconds)
                    {
                        _elapsedTime += elapsed;
                        character.SetDirectionAnimation(_direction);
                        return CharacterActions.Idle;
                    }
                    else
                    {
                        _elapsedTime = TimeSpan.Zero;
                        _currentChargeStatus = ChargeStatus.Charge;

                        if (_direction == CharacterActions.Up)
                            _taurus.MainAnimationsAnimationManager.PlayAnimation("TaurusRunBack");
                        else if (_direction == CharacterActions.Right || _direction == CharacterActions.Left)
                            _taurus.MainAnimationsAnimationManager.PlayAnimation("TaurusRunSide");
                        else if (_direction == CharacterActions.Down)
                            _taurus.MainAnimationsAnimationManager.PlayAnimation("TaurusRunFront");
                    }

                    break;

                case ChargeStatus.Charge:

                    var currentCell = _gameGrid.GetCellAtPixel(character.Position);

                    // Se c'è una candela lungo la strada, la spengo:
                    var possibleCandle = _candlesManager.IsThereACandle(currentCell);
                    if (possibleCandle != null)
                    {
                        possibleCandle.Die(TimeSpan.FromMilliseconds(500));
                    }

                    // Se posso andare avanti vado avanti caricando
                    if (currentCell.IsPassableWithAction(_direction))
                    {
                        _game.StartCameraShake(5, TimeSpan.FromSeconds(1));
                        return _direction;
                    }
                    else
                    {
                        // Spacco il muro
                        var breakWallEffectLocation = character.WorldCenter;
                        switch (_direction)
                        {
                            case CharacterActions.Up:
                                breakWallEffectLocation.Y -= character.Height / 2;
                                break;
                            case CharacterActions.Down:
                                breakWallEffectLocation.Y += character.Height / 2;
                                break;
                            case CharacterActions.Right:
                                breakWallEffectLocation.X += character.Width / 2;
                                break;
                            case CharacterActions.Left:
                                breakWallEffectLocation.X -= character.Width / 2;
                                break;
                        }

                        _gameGrid.BreakWall(
                            currentCell,
                            _direction,
                            breakWallEffectLocation);

                        _currentChargeStatus = ChargeStatus.Stunned;
                        _taurus.DontPlayDefaultAnimations = false;
                    }

                    break;

                case ChargeStatus.Stunned:

                    if (_elapsedTime < _stunTimeSeconds)
                    {
                        _elapsedTime += elapsed;
                        return CharacterActions.Idle;
                    }
                    else
                    {
                        // Resetto la velocità
                        character.Speed = _normalSpeed;

                        // Ritorno a fare quello che facevo prima
                        character.ResetNormalColor();
                        if (_nextAI is PatrolAI)
                            ((PatrolAI)_nextAI).ClearActions();
                        return character.SetNewAI(_nextAI);
                    }
            }

            return _direction;
        }
    }
}
