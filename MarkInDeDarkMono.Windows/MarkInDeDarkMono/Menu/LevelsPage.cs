using FlowsoftGamesMonogame;
using FlowsoftGamesMonogame.Drawing;
using FlowsoftGamesMonogame.Drawing.ViewportAdapters;
using FlowsoftGamesMonogame.Extensions;
using FlowsoftGamesMonogame.UI;
using MarkInDeDarkMono.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDarkMono.Menu
{
    public class LevelsPage
    {
        private readonly Sprite _background;
        private readonly SpriteFont _font;

        private readonly ScalingObject _titleScalingObject;
        private readonly DrawingInfos _titleDrawingInfos;
        private readonly string _titleText;

        private readonly Matrix _scaleMatrix;
        private List<LevelUI> _levelsUi;
        private Vector2 _startingPosition;

        public LevelsPage(
            ViewportAdapter viewportAdapter,
            AssetsLoader assetsLoader)
        {
            _font = assetsLoader.Font;

            _titleText = GameStrings.LevelsPageTitleString.GetTranslation();
            _titleScalingObject = new ScalingObject(1f, 1.2f, 1.0f);
            _titleDrawingInfos = new DrawingInfos()
            {
                Position = new Vector2(viewportAdapter.VirtualWidth / 2f, 120f),
                Origin = new Vector2(_font.GetTextCenter(_titleText).X, 0),
                OverlayColor = new Color(138, 7, 7)
            };

            _background = assetsLoader.Sprites["levels"];
            _scaleMatrix = viewportAdapter.GetScaleMatrix();
            _levelsUi = assetsLoader.Levels.Select(l => new LevelUI(assetsLoader, l)).ToList();
            _startingPosition = new Vector2(240f, 240f);

            var startingPosition = _startingPosition;
            const float padding = 144f;

            for (int i = 0; i < _levelsUi.Count; ++i)
            {
                var levelUi = _levelsUi[i];

                if (i % 4 == 0)
                {
                    startingPosition.Y += levelUi.BoxRectangle.Height + padding;
                    startingPosition.X = _startingPosition.X;
                }

                levelUi.SetPosition(startingPosition);
                startingPosition.X += levelUi.BoxRectangle.Width + padding;
            }
        }

        public void Update(TimeSpan elapsed)
        {
            _titleScalingObject.Update(elapsed);
            _titleDrawingInfos.Scale = _titleScalingObject.Scale;
        }

        public void HandleInput(
            Vector2 touchPoint,
            GameOrchestrator orchestrator)
        {
            foreach (var levelUi in _levelsUi)
            {
                if (levelUi.HitBox.Contains(touchPoint))
                {
                    orchestrator.SetGameState(levelUi.Level);
                    break;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(transformMatrix: _scaleMatrix);

            spriteBatch.Draw(_background);
            spriteBatch.DrawString(_font, _titleText, _titleDrawingInfos);

            foreach (var levelUi in _levelsUi)
            {
                levelUi.Draw(spriteBatch);
            }

            spriteBatch.End();
        }

        public void StopMusic() { }
    }
}
