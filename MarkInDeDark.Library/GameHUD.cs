using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Drawing.Abstractions;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.TransformationObjects;
using MarkInDeDark.Library;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkInDeDark.Library
{
    public class GameHUD
    {
        private class PopupText
        {
            public string Text { get; set; }
            public DrawingInfos DrawingInfos { get; set; }
            public PopupObject PopupObject { get; set; }
        }

        private readonly SpriteFont _font;

        private readonly DrawingInfos _gameElapsedTimeDrawingInfos;
        private readonly StringBuilder _gameElapsedString;
        public TimeSpan GameElapsed { get; private set; }

        private readonly DrawingInfos _remainingCandlesDrawingInfos;
        private readonly StringBuilder _remainingCandlesString;

        private readonly Vector2 _messagesStartPosition;
        private readonly List<PopupText> _messages;

        private readonly CandlesManager _candlesManager;

        public GameHUD(
            AssetsLoader assetsLoader,
            IScreenTransformationMatrixProvider scaleMatrixProvider,
            CandlesManager candlesManager)
        {
            _font = assetsLoader.Font;

            _gameElapsedString = new StringBuilder();
            _gameElapsedTimeDrawingInfos = new DrawingInfos()
            {
                Position = new Vector2(24.0f, 24.0f),
                OverlayColor = Color.White,
                Scale = 0.6f
            };

            _remainingCandlesString = new StringBuilder();
            _remainingCandlesDrawingInfos = new DrawingInfos()
            {
                Position = new Vector2(
                    scaleMatrixProvider.RealScreenWidth - 120f,
                    scaleMatrixProvider.RealScreenHeight - 96f),
                Scale = 0.4f,
                OverlayColor = Color.Brown
            };

            _messagesStartPosition = new Vector2(
               scaleMatrixProvider.RealScreenWidth / 2,
               78f);

            _messages = new List<PopupText>();
            GameElapsed = TimeSpan.Zero;
            _candlesManager = candlesManager;
        }

        public void ShowMessage(
            string text,
            Color color,
            bool isImportant = false)
        {
            var messagePosition = _messagesStartPosition;

            if (_messages.Count > 0)
            {
                // Non scrivo due messaggi identici in fila
                if (_messages.Last().Text == text)
                {
                    return;
                }

                messagePosition =
                    new Vector2(
                        _messagesStartPosition.X,
                        _messages.Max(m => m.DrawingInfos.Position.Y) + 50);
            }

            var newPopup = new PopupText()
            {
                Text = text,
                PopupObject = new PopupObject(
                    TimeSpan.FromSeconds(3),
                    messagePosition,
                    color,
                    60f),
                DrawingInfos = new DrawingInfos()
                {
                    Scale = isImportant ? 0.7f : 0.6f,
                    Position = messagePosition,
                    Origin = new Vector2(_font.GetTextCenter(text).X, 0)
                }
            };

            newPopup.PopupObject.Popup();
            _messages.Add(newPopup);
        }

        public void Update(TimeSpan elapsed)
        {
            _remainingCandlesString.Clear();
            _remainingCandlesString.AppendNumber(_candlesManager.LeftCandles);

            GameElapsed += elapsed;
            _gameElapsedString.Clear();
            _gameElapsedString.Append(GameElapsed.ToMinuteSecondsFormat());

            for (int i = _messages.Count - 1; i >= 0; --i)
            {
                var message = _messages[i];
                message.PopupObject.Update(elapsed);
                message.DrawingInfos.Position = message.PopupObject.Position;
                message.DrawingInfos.OverlayColor = message.PopupObject.OverlayColor;

                if (message.PopupObject.IsCompleted)
                {
                    _messages.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, _gameElapsedString.ToString(), _gameElapsedTimeDrawingInfos);
            spriteBatch.DrawString(_font, _remainingCandlesString.ToString(), _remainingCandlesDrawingInfos);

            foreach (var message in _messages)
            {
                spriteBatch.DrawString(_font, message.Text, message.DrawingInfos);
            }
        }

    }
}
