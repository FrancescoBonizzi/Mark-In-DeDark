using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Drawing.Abstractions;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using FbonizziMonoGame.StringsLocalization.Abstractions;
using FbonizziMonoGame.TransformationObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.Library.Menu
{
    public class LevelsPage
    {
        private readonly Sprite _background;
        private readonly SpriteFont _font;
        private readonly IScreenTransformationMatrixProvider _screenTransformationMatrixProvider;
        private readonly DrawingInfos _titleDrawingInfos;
        private readonly string _titleText;

        private readonly List<LevelButton> _levelButtons;
        private Vector2 _startingPosition;

        private readonly TextButton _backButton;

        public LevelsPage(
            IScreenTransformationMatrixProvider screenTransformationMatrixProvider,
            ILocalizedStringsRepository localizedStringsRepository,
            AssetsLoader assetsLoader)
        {
            _font = assetsLoader.Font;

            _screenTransformationMatrixProvider = screenTransformationMatrixProvider;

            _titleText = localizedStringsRepository.Get("LevelsPageTitleString");
            _titleDrawingInfos = new DrawingInfos()
            {
                Position = new Vector2(screenTransformationMatrixProvider.VirtualWidth / 2f, 240f),
                Origin = _font.GetTextCenter(_titleText),
                OverlayColor = Color.White
            };

            _background = assetsLoader.Sprites["levels"];
            _levelButtons = assetsLoader.Levels.Select(l => new LevelButton(assetsLoader, l)).ToList();
            _startingPosition = new Vector2(240f, 140f);

            var startingPosition = _startingPosition;
            const float padding = 100f;

            for (int i = 0; i < _levelButtons.Count; ++i)
            {
                var levelUi = _levelButtons[i];

                if (i % 4 == 0)
                {
                    startingPosition.Y += levelUi.BoxRectangle.Height + padding;
                    startingPosition.X = _startingPosition.X;
                }

                levelUi.SetStartingPosition(startingPosition);
                startingPosition.X += levelUi.BoxRectangle.Width + padding;
            }

            // Back button
            _backButton = new TextButton(
               localizedStringsRepository.Get("BackButtonString"),
               _font,
               new Vector2(
                   200,
                   screenTransformationMatrixProvider.VirtualHeight - 90),
               new Vector2(0, 0));
        }

        public void Update(TimeSpan elapsed)
        {
        }

        public void HandleInput(
            Vector2 pointerPosition,
            bool hasClicked,
            GameOrchestrator orchestrator)
        {
            foreach (var levelUi in _levelButtons)
            {
                levelUi.HandlePointerPosition(
                    pointerPosition,
                    hasClicked,
                    () =>
                    {
                        orchestrator.SetGameState(levelUi.Level);
                    });
            }

            _backButton.HandlePointerPosition(
                pointerPosition,
                hasClicked,
                () =>
                {
                    orchestrator.Back();
                });
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(transformMatrix: _screenTransformationMatrixProvider.ScaleMatrix);

            spriteBatch.Draw(_background);
            spriteBatch.DrawString(_font, _titleText, _titleDrawingInfos);

            foreach (var levelUi in _levelButtons)
            {
                levelUi.Draw(spriteBatch);
            }

            _backButton.Draw(spriteBatch);

            spriteBatch.End();
        }

        public void StopMusic() { }
    }
}
