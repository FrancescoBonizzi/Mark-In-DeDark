using MarkInDeDarkMono.Characters;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MarkInDeDarkMono.Objects
{
    public class ShowTextTriggerObject : TriggerObject
    {
        private readonly string _textToShow;
        private readonly GameHUD _gameHUD;

        public ShowTextTriggerObject(
            string textToShow,
            GameHUD gameHUD,
            Rectangle triggerCollisionRectangle,
            Player player) 
            : base(
                  triggerCollisionRectangle: triggerCollisionRectangle,
                  affectedCharacters: new List<Character>() { player },
                  onTriggerPressed: (c, t) => { },
                  onTriggerLeft: null,
                  shouldFireOnce: true)
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
