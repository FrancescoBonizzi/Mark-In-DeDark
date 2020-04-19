
using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDark.Library.AI
{
    public class SkeletonWithPlayerAI : HuntPlayerAI
    {
        private readonly SkeletonWithoutPlayerAI _previousAI;

        private GameCell _oldDestionation;

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

        public SkeletonWithPlayerAI(
            Skeletron skeletron,
            SkeletonWithoutPlayerAI nextAI,
            GameGrid gameGrid,
            CandlesManager candlesManager,
            Player player,
            SoundManager soundManager,
            GameConfig gameConfig)
            : base(gameGrid, player, candlesManager)
        {
            _previousAI = nextAI;
            _originalSpeed = skeletron.Speed;

            skeletron.SetTemporaryColor(gameConfig.SkeletronRageColorRGB);
            skeletron.Speed = gameConfig.SkeletronHuntingSpeed;
            soundManager.Play("PatrolSeenYou");

            _currentStatus = HuntingStatus.Following;
        }

        public override Vector2 GetNextDirection(Enemy character, TimeSpan elapsed)
        {
            switch (_currentStatus)
            {
                case HuntingStatus.Following:

                    // Ricerca candele...
                    var pathToTheNearestCandle = KillCandleAI.SearchForNearCandlePath(character, _candlesManager);
                    // ...se c'è una candela nei paraggi...
                    if (pathToTheNearestCandle != null)
                    {
                        // ...passo all'AI di ricerca candele
                        character.SetNewAI(new KillCandleAI(pathToTheNearestCandle.Candle, pathToTheNearestCandle.PathToTheCandle, this));
                        return character.CurrentAI.GetNextDirection(character, elapsed);
                    }


                    // Se il player che sto seguendo si è spostato rispetto a prima...
                    var destinationCell = _gameGrid.GetCellAtPixel(_player.Position);
                    if (_oldDestionation != destinationCell)
                    {
                        // ...cerco di capire se riesco comunque a raggiungerlo a vista
                        var pathToPlayerIfFacingAndSameDirection = SkeletonWithoutPlayerAI.PathToPositionIfFacingAndSameDirection(
                            character,
                            _player.Position,
                            _gameGrid);

                        if (pathToPlayerIfFacingAndSameDirection != null)
                        {
                            _oldDestionation = destinationCell;
                            // ...se ci riesco, continuo a seguirlo
                            break;
                        }
                        else
                        {
                            // ...altrimenti passo nella fase di sgomento
                            _currentStatus = HuntingStatus.Incredulity;
                            return Vector2.Zero;
                        }
                    }

                    // ...se non si è spostato, continuo a seguirlo
                    break;
                  
                case HuntingStatus.Incredulity:

                    if (_elapsedTime <= _incredulityPhaseDuration)
                    {
                        _elapsedTime += elapsed;
                    }
                    else
                    {
                        _elapsedTime = TimeSpan.Zero;
                        _currentIncredulityPhase++;
                    }

                    if (_currentIncredulityPhase == 0)
                    {
                        character.SetAction(CharacterActions.Left);
                        return Vector2.Zero;
                    }
                    else if (_currentIncredulityPhase == 1)
                    {
                        character.SetAction(CharacterActions.Right);
                        return Vector2.Zero;
                    }
                    if (_currentIncredulityPhase == 2)
                    {
                        character.SetAction(CharacterActions.Left);
                        return Vector2.Zero;
                    }
                    else if (_currentIncredulityPhase == 3)
                    {
                        character.SetAction(CharacterActions.Right);
                        return Vector2.Zero;
                    }
                    else if (_currentIncredulityPhase == 4)
                    {
                        character.SetAction(CharacterActions.Left);
                        return Vector2.Zero;
                    }
                    else if (_currentIncredulityPhase == 5)
                    {
                        character.SetAction(CharacterActions.Right);
                        return Vector2.Zero;
                    }

                    _elapsedTime = TimeSpan.Zero;
                    _currentStatus = HuntingStatus.Lost;
                    return Vector2.Zero;

                case HuntingStatus.Lost:

                    character.ResetNormalColor();
                    character.Speed = _originalSpeed;
                    _previousAI.ClearActions();
                    character.SetNewAI(_previousAI);
                    return Vector2.Zero;
            }

            return base.GetNextDirection(character, elapsed);
        }
    }
}
