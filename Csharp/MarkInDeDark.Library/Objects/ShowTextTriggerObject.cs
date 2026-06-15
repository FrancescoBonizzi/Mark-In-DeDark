using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MarkInDeDark.Library.Objects
{
    public class ShowTextTriggerObject : TriggerObject
    {
        private readonly string _textToShow;
        private readonly GameHUD _gameHUD;

        public ShowTextTriggerObject(
            string textToShow,
            GameHUD gameHUD,
            Rectangle triggerCollisionRectangle,
            Player player,
            GameGrid gameGrid) 
            : base(
                  triggerCollisionRectangle: triggerCollisionRectangle,
                  affectedCharacters: new List<Character>() { player },
                  onTriggerPressed: (c, t) => { },
                  onTriggerLeft: null,
                  shouldFireOnce: true,
                  gameGrid: gameGrid)
        {
            _textToShow = textToShow;
            _gameHUD = gameHUD;

            SetOnTriggerPressed((c, t) =>
            {
                _gameHUD.ShowMessage(_textToShow, Color.Yellow);
            });
        }
    }
}
