using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.GameGridCommon;
using MarkInDeDark.Library.Extensions;
using MarkInDeDark.Library.Maths;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;

namespace MarkInDeDark.Library.Levels
{
    public class Pillar
    {
        public DrawingInfos DrawingInfos { get; }
        public Hull Hull { get; }
        public PillarPositions PillarPosition { get; }

        private readonly Sprite _sprite;
        private readonly Sprite _normalSprite;

        public Pillar(
            Vector2 position,
            Sprite sprite,
            Sprite normalSprite,
            PillarPositions pillarPosition)
        {
            PillarPosition = pillarPosition;

            DrawingInfos = new DrawingInfos()
            {
                Position = position,
                LayerDepth = ZIndexCalculator.ForPillars(position + new Vector2(0, 96)) // Così li equiparo all'altezza dei muri
            };

            _sprite = sprite;
            _normalSprite = normalSprite;

            Hull = Utilities.HullFromRectangle(new Rectangle(
                (int)position.X + 15,
                (int)position.Y + _sprite.Height - 65,
                50,
                35));
            Hull.Points.Add(new Vector2(0, 0));
            Hull.Points.Add(new Vector2(0, 1));
            Hull.Points.Add(new Vector2(1, 1));
            Hull.Points.Add(new Vector2(1, 0));
        }

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_sprite, DrawingInfos);

        public void DrawNormalMap(SpriteBatch spriteBatch)
        {
            if (_normalSprite != null)
                spriteBatch.Draw(_normalSprite, DrawingInfos);
        }
    }
}
