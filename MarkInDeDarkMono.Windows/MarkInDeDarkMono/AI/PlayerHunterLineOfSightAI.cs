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

        private CharacterActions? _lastAction;

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
            Enemy enemy,
            TimeSpan elapsed)
        {
            var actionsToNearCandle = KillCandleAI.SearchForNearCandlePath(enemy, _candlesManager);
            if (actionsToNearCandle.Key != null)
            {
                enemy.SetNewAI(new KillCandleAI(actionsToNearCandle.Key, actionsToNearCandle.Value, this, _gameGrid));
                _currentActions = null; // Lo annullo così appena ritorna in questo stato ricalcola i waypoint per tornare dov'era prima
                return enemy.CurrentAI.Update(enemy, elapsed);
            }

            var destinationCell = _gameGrid.GetCellAtPixel(_player.Position);
            var enemyCell = _gameGrid.GetCellAtPixel(enemy.Position);

            // Se sono sulla stessa cella deve continuare a seguirlo
            if (destinationCell == enemyCell && _lastAction != null)
            {
                _currentActions = new Queue<CharacterActions>();
                _currentActions.Enqueue(_lastAction.Value);
            }
            // Se il player si è spostato rispetto a prima o se ho finito le azioni da fare...
            else if (_oldDestionation != destinationCell || !HasActionsToDo)
            {                
                // ...cerco di capire se riesco comunque a raggiungerlo a vista
                var pathToPlayerIfFacingAndSameDirection = PatrolHunterLineOfSightAI.PathToPositionIfFacingAndSameDirection(
                    enemy, _player.Position, _gameGrid);

                if (pathToPlayerIfFacingAndSameDirection != null)
                {
                    _oldDestionation = destinationCell;
                    _currentActions = new Queue<CharacterActions>(pathToPlayerIfFacingAndSameDirection);
                }
            }

            // Finchè ho azioni da fare, le eseguo
            if (HasActionsToDo)
            {
                _lastAction = _currentActions.Dequeue();
                return _lastAction.Value;
            }

            // Altrimenti ritorno allo stato originale
            enemy.ResetNormalColor();
            enemy.Speed = _originalSpeed;
            return enemy.SetNewAI(_nextAI);
        }
    }
}
