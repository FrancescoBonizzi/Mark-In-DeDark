using FlowsoftGamesMonogame;
using FlowsoftGamesMonogame.Drawing;
using FlowsoftGamesMonogame.Drawing.ViewportAdapters;
using FlowsoftGamesMonogame.Extensions;
using FlowsoftGamesMonogame.UI;
using MarkInDeDarkMono.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDarkMono.Menu
{
    public class MainMenuPage
    {
        private readonly Sprite _background;
        private readonly SpriteFont _font;
        private readonly Matrix _scaleMatrix;

        private readonly ScalingObject _titleScalingObject;
        private readonly DrawingInfos _titleDrawingInfos;
        private readonly string _titleText;

        private readonly ScalingObject _playScalingObject;
        private readonly DrawingInfos _playDrawingInfos;
        private readonly string _playText;
        private readonly Vector2 _playTextSize;

        private readonly ScalingObject _shopScalingObject;
        private readonly DrawingInfos _shopDrawingInfos;
        private readonly string _shopText;
        private readonly Vector2 _shopTextSize;

        private readonly SoundManager _soundManager;

        public MainMenuPage(
            AssetsLoader assetsLoader,
            SoundManager soundManager,
            ViewportAdapter viewportAdapter)
        {
            _soundManager = soundManager;
            _font = assetsLoader.Font;
            _scaleMatrix = viewportAdapter.GetScaleMatrix();
            _background = assetsLoader.Sprites["menu"];

            _titleText = "Mark in DeDark";
            _titleScalingObject = new ScalingObject(1f, 1.2f, 1.0f);
            _titleDrawingInfos = new DrawingInfos()
            {
                Position = new Vector2(viewportAdapter.VirtualWidth / 3f, 360f),
                Origin = _font.GetTextCenter(_titleText),
                OverlayColor = new Color(138, 7, 7)
            };

            _playText = GameStrings.PlayButtonString.GetTranslation();
            _playScalingObject = new ScalingObject(0.7f, 1.0f, 0.8f);
            _playDrawingInfos = new DrawingInfos()
            {
                Position = new Vector2(336f, 600f),
                Origin = _font.GetTextCenter(_playText),
                OverlayColor = Color.Black
            };
            _playTextSize = _font.MeasureString(_playText);

            _shopText = GameStrings.ShopString.GetTranslation();
            _shopScalingObject = new ScalingObject(0.7f, 1.0f, 0.8f);
            _shopDrawingInfos = new DrawingInfos()
            {
                Position = new Vector2(336f, 768f),
                Origin = _font.GetTextCenter(_shopText),
                OverlayColor = Color.Black
            };
            _shopTextSize = _font.MeasureString(_shopText);

            // background menu music play
        }

        public void StopMusic()
        {
            _soundManager.StopSounds();
        }

        public void HandleInput(
            Vector2 touchPoint,
            GameOrchestrator orchestrator)
        {
            if (_playDrawingInfos.HitBox((int)_playTextSize.X, (int)_playTextSize.Y)
                .Contains(touchPoint))
            {
                _soundManager.StopSounds();
                orchestrator.SetLevelState();
            }
            else if (_shopDrawingInfos.HitBox((int)_shopTextSize.X, (int)_shopTextSize.Y)
                .Contains(touchPoint))
            {
                // TODO
            }
        }

        public void Update(TimeSpan elapsed)
        {
            _titleScalingObject.Update(elapsed);
            _titleDrawingInfos.Scale = _titleScalingObject.Scale;

            _playScalingObject.Update(elapsed);
            _playDrawingInfos.Scale = _playScalingObject.Scale;

            _shopScalingObject.Update(elapsed);
            _shopDrawingInfos.Scale = _shopScalingObject.Scale;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(transformMatrix: _scaleMatrix);

            spriteBatch.Draw(_background);

            spriteBatch.DrawString(_font, _titleText, _titleDrawingInfos);
            spriteBatch.DrawString(_font, _playText, _playDrawingInfos);
            spriteBatch.DrawString(_font, _shopText, _shopDrawingInfos);

            spriteBatch.End();
        }
    }
}
