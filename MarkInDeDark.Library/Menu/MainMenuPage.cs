using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Drawing.Abstractions;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using FbonizziMonoGame.StringsLocalization.Abstractions;
using FbonizziMonoGame.TransformationObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;

namespace MarkInDeDark.Library.Menu
{
    public class MainMenuPage
    {
        private readonly IScreenTransformationMatrixProvider _screenTransformationMatrixProvider;
        private readonly Sprite _background;
        private readonly SpriteFont _font;

        private readonly Sprite _logo;
        private readonly DrawingInfos _logoDrawingInfos;

        private readonly string _betaText;
        private readonly DrawingInfos _betaDrawingInfos;

        private readonly TextButton _playButton;
        private readonly TextButton _exitButton;
        private readonly SoundManager _soundManager;

        public MainMenuPage(
            AssetsLoader assetsLoader,
            SoundManager soundManager,
            IScreenTransformationMatrixProvider screenTransformationMatrixProvider,
            ILocalizedStringsRepository localizedStringsRepository)
        {
            _soundManager = soundManager;
            _font = assetsLoader.Font;
            _background = assetsLoader.Sprites["mainpage"];
            _logo = assetsLoader.Sprites["logo"];

            _screenTransformationMatrixProvider = screenTransformationMatrixProvider;

            // Logo
            _logoDrawingInfos = new DrawingInfos()
            {
                Position = new Vector2(screenTransformationMatrixProvider.VirtualWidth / 2, 280f),
                Origin = _logo.SpriteCenter,
                Scale = 1.8f
            };

            _betaText = "Beta Ver.";
            var betaScale = 0.3f;
            var betaTextSize = _font.MeasureString(_betaText) * betaScale;
            _betaDrawingInfos = new DrawingInfos()
            {
                Position = new Vector2(
                    _logoDrawingInfos.Position.X + _logo.Width - betaTextSize.X - betaTextSize.X / 2,
                    _logoDrawingInfos.Position.Y + _logo.Height / 2 + 30),
                OverlayColor = new Color(186, 119, 75),
                Scale = betaScale
            };

            // Buttons
            var lastButtonY = _logoDrawingInfos.Position.Y + (screenTransformationMatrixProvider.VirtualHeight - _logoDrawingInfos.Position.Y) / 2f - 70f;
            _playButton = new TextButton(
                localizedStringsRepository.Get("PlayButtonString"),
                _font,
                new Vector2(
                    screenTransformationMatrixProvider.VirtualWidth / 2,
                    lastButtonY));
            _exitButton = new TextButton(
                localizedStringsRepository.Get("ExitButtonString"),
                _font,
                new Vector2(
                    screenTransformationMatrixProvider.VirtualWidth / 2,
                    lastButtonY + 80));

            // Background menu music play
            // TODO About, Settings - if any
        }

        public void StopMusic()
        {
            _soundManager.StopAll();
        }

        public void HandleInput(
            Vector2 pointerPosition,
            bool hasClicked,
            GameOrchestrator orchestrator)
        {
            _playButton.HandlePointerPosition(
                pointerPosition,
                hasClicked,
                () =>
                {
                    _soundManager.StopAll();
                    orchestrator.SetLevelState();
                });

            _exitButton.HandlePointerPosition(
                pointerPosition,
                hasClicked,
                () =>
                {
                    Environment.Exit(0);
                });
        }

        public void Update(TimeSpan elapsed)
        {
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(transformMatrix: _screenTransformationMatrixProvider.ScaleMatrix);

            spriteBatch.Draw(_background);
            spriteBatch.Draw(_logo, _logoDrawingInfos);
            _playButton.Draw(spriteBatch);
            _exitButton.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
