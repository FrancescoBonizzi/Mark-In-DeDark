using Mark.Engine;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Characters;
using MarkInDeDarkMono.Common;
using MarkInDeDarkMono.Levels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDarkMono.AI
{
    public class PlayerHunterLineOfSightAI : IEnemyAI
    {
        private readonly CandlesManager _candlesManager;
        private readonly Player _player;

        private readonly IEnemyAI _nextAI;
        private Queue<CharacterActions> _currentActions;
        
        private readonly GameGrid _gameGrid;
        private GameCell _oldDestionation;

        private readonly float _originalSpeed;

        public PlayerHunterLineOfSightAI(
            Enemy skeletron,
            IEnemyAI nextAI,
            GameGrid gameGrid,
            CandlesManager candlesManager,
            Player player,
            SoundManager soundManager,
            GameConfig gameConfig,
            IEnumerable<CharacterActions> firstPathToPlayer)
        {
            _nextAI = nextAI;
            _player = player;
            _gameGrid = gameGrid;
            _candlesManager = candlesManager;
            _originalSpeed = skeletron.Speed;

            skeletron.SetTemporaryColor(gameConfig.SkeletronRageColorRGB);
            skeletron.Speed = gameConfig.SkeletronHuntingSpeed;
            soundManager.PlayPatrolSeenYou();

            _currentActions = new Queue<CharacterActions>(firstPathToPlayer);
        }

        private bool HasActionsToDo
            => _currentActions != null && _currentActions.Any();

        public CharacterActions Update(
            Enemy character,
            TimeSpan elapsed)
        {
            var actionsToNearCandle = KillCandleAI.SearchForNearCandlePath(character, _candlesManager);
            if (actionsToNearCandle.Key != null)
            {
                character.SetNewAI(new KillCandleAI(actionsToNearCandle.Key, actionsToNearCandle.Value, this, _gameGrid));
                _currentActions = null; // Lo annullo così appena ritorna in questo stato ricalcola i waypoint per tornare dov'era prima
                return character.CurrentAI.Update(character, elapsed);
            }

            var destination = _gameGrid.GetCellAtPixel(_player.Position);
            // Se il player si è spostato rispetto a prima o se ho finito le azioni da fare...
            if (_oldDestionation != destination || !HasActionsToDo)
            {
                // ...cerco di capire se riesco comunque a raggiungerlo a vista
                var pathToPlayerIfFacingAndSameDirection = PatrolHunterLineOfSightAI.PathToPositionIfFacingAndSameDirection(
                    character, _player.Position, _gameGrid);

                if (pathToPlayerIfFacingAndSameDirection != null)
                {
                    _oldDestionation = destination;
                    _currentActions = new Queue<CharacterActions>(pathToPlayerIfFacingAndSameDirection);
                }
            }

            // Finchè ho azioni da fare, le eseguo
            if (HasActionsToDo)
            {
                return _currentActions.Dequeue();
            }

            // Altrimenti ritorno allo stato originale
            character.ResetNormalColor();
            character.Speed = _originalSpeed;
            return character.SetNewAI(_nextAI);
        }
    }
}
