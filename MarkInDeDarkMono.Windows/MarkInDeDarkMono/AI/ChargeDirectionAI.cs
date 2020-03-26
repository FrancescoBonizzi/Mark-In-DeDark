using Mark.Engine;
using MarkInDeDarkMono.Characters;
using MarkInDeDarkMono.Levels;
using System;

namespace MarkInDeDarkMono.AI
{
    public class ChargeDirectionAI : IEnemyAI
    {
        private readonly GameGrid _gameGrid;
        private readonly CharacterActions _direction;
        private readonly float _normalSpeed;
        private readonly IEnemyAI _nextAI;
        private readonly CandlesManager _candlesManager;

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
            IEnemyAI nextAI)
        {
            _gameGrid = gameGrid;
            _direction = direction;
            _normalSpeed = taurus.Speed;
            taurus.Speed = taurus.TaurusChargeSpeed;
            _currentChargeStatus = ChargeStatus.Starting;
            _candlesManager = candlesManager;

            soundManager.PlayTaurusStargingCharge();

            _chargeTimeToStartSeconds = TimeSpan.FromSeconds(taurus.TaurusTimeToStartSeconds);
            _stunTimeSeconds = TimeSpan.FromSeconds(taurus.TaurusTimeStunnedSeconds);
            taurus.SetTemporaryColor(taurus.TaurusChargeColorRGB);
            _nextAI = nextAI;
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
                    }

                    break;

                case ChargeStatus.Charge:

                    var currentCell = _gameGrid.GetCellAtPixel(character.Position);

                    // Se c'è una candela lungo la strada, la spengo:
                    var possibleCandle = _candlesManager.IsThereACandle(currentCell);
                    if (possibleCandle != null)
                    {
                        possibleCandle.DieFast();
                    }

                    // Se posso andare avanti vado avanti caricando
                    if (currentCell.IsPassableWithAction(_direction))
                    {
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
                        return character.SetNewAI(_nextAI);
                    }
            }

            return _direction;
        }
    }
}
