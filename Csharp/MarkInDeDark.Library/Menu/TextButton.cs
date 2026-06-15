using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using MarkInDeDark.Library.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDark.Library.Menu
{
    public class TextButton
    {

        private readonly SpriteFont _font;

        private readonly string _text;
        private readonly Vector2 _textSize;
        private readonly Color _unhoveredColor = Color.White.WithAlpha(0.3f);
        private readonly Color _hoveredColor = Color.White;
        private readonly DrawingInfos _drawingInfos;

        private ButtonStates _currentState;

        public TextButton(
            string text,
            SpriteFont font,
            Vector2 position,
            Vector2? origin = null)
        {
            _font = font;

            _text = text;
            _drawingInfos = new DrawingInfos()
            {
                Position = position,
                Origin = origin ?? _font.GetTextCenter(_text),
                OverlayColor = _unhoveredColor,
                Scale = 0.5f
            };
            _textSize = _font.MeasureString(_text) * 0.5f;

            _currentState = ButtonStates.Unhovered;
        }

        public void HandlePointerPosition(
            Vector2 pointerPosition,
            bool hasClicked,
            Action onClickAction)
        {
            // BUG nella libreria: non è calcolato nell'origine
            if (_drawingInfos.HitBoxFixed((int)_textSize.X, (int)_textSize.Y).Contains(pointerPosition))
            {
                if (hasClicked)
                {
                    onClickAction();
                }

                _currentState = ButtonStates.Hovered;
                _drawingInfos.OverlayColor = _hoveredColor;
            }
            else
            {
                _currentState = ButtonStates.Unhovered;
                _drawingInfos.OverlayColor = _unhoveredColor;
                // TODO cambiare anche il puntatore al mouse?
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, _text, _drawingInfos);
        }
    }
}
