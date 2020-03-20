using Mark.Engine;
using MarkInDeDarkMono.Characters;
using MarkInDeDarkMono.Common;
using MarkInDeDarkMono.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDarkMono.AI
{
    public class PatrolHunterLineOfSightAI : PatrolAI
    {
        private readonly GameConfig _gameConfig;

        public PatrolHunterLineOfSightAI(
            List<GameCell> patrolWaypoints,
            GameGrid gameGrid,
            CandlesManager candlesManager,
            Player player,
            SoundManager soundManager,
            GameConfig gameConfig)
            : base(patrolWaypoints, gameGrid, candlesManager, player, soundManager)
        {
            _gameConfig = gameConfig;
        }

#warning Forse è un metodo più da enemy che da AI... boh
        public static IEnumerable<CharacterActions> PathToPositionIfFacingAndSameDirection(
            Enemy character, 
            Vector2 position, 
            GameGrid gameGrid)
        {
            // Prima euristica: devono essere sulla stessa retta,
            // Verticale o orizzontale che sia
            if (character.Position.Y != position.Y
                && character.Position.X != position.X)
            {
                return null;
            }

            // Seconda euristica: il nemico sta guardando in faccia il player?
            // (Se è di schiena non lo può vedere)
            if ((
                (character.CurrentCharacterState == Character.CharacterState.GoingLeft
                && character.Position.X < position.X)

                || (character.CurrentCharacterState == Character.CharacterState.GoingRight
                && character.Position.X > position.X)

                || (character.CurrentCharacterState == Character.CharacterState.GoingUp
                && character.Position.Y < position.Y)

                || (character.CurrentCharacterState == Character.CharacterState.GoingDown
                && character.Position.Y > position.Y)
                ))
            {
                return null;
            }

            // Se sono sulla stessa retta, calcolo le azioni per raggiungersi,
            // così posso vedere se ci sono muri di mezzo (ovvero se deve fare più azioni per raggiungerlo)
            var actionsToPlayer = gameGrid.FindActionsToPath(
                character.Position,
                position);

            if (actionsToPlayer == null || !actionsToPlayer.Any()) // Può succedere se il player si sposta al volo
            {
                return null;
            }

            var firstAction = actionsToPlayer.First();

            foreach (var action in actionsToPlayer.Skip(1))
            {
                // Se il path per il player non è sempre la stessa direzione, non lo seguo
                if (action != firstAction)
                {
                    return null;
                }
            }

            return actionsToPlayer;
        }

        public override CharacterActions Update(
            Enemy character,
            TimeSpan elapsed)
        {
            var pathToPlayerIfFacingAndSameDirection = PathToPositionIfFacingAndSameDirection(
                character,
                _player.Position,
                _gameGrid);

            if (pathToPlayerIfFacingAndSameDirection == null)
                return base.Update(character, elapsed);

            return character.SetNewAI(new PlayerHunterLineOfSightAI(
                character, this, _gameGrid, _candlesManager, _player, _soundManager, _gameConfig, pathToPlayerIfFacingAndSameDirection));
        }

    }
}
