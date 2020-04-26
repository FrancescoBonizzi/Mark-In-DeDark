using FbonizziMonoGame.Drawing;
using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Extensions;
using Microsoft.Xna.Framework;

namespace MarkInDeDark.Library
{
    public class Exit
    {
        private readonly Vector2 _cellSize;
        private readonly DrawingInfos _drawingInfos;
        public bool IsOpen { get; private set; } = false;

        public Exit(
            Vector2 position,
            Vector2 cellSize)
        {
            _cellSize = cellSize;

            _drawingInfos = new DrawingInfos()
            {
                Position = position,
                HitBoxTolerance = new Rectangle(60, 60, 120, 120),
                Origin = cellSize / 2f,
                LayerDepth = ZIndexCalculator.GroundObjectsLayerDepth
            };
        }

        public void SetOpen()
        {
            IsOpen = true;
        }

        public Vector2 Position
            => _drawingInfos.Position;

        public Rectangle CollisionRectangle
            => _drawingInfos.HitBox(
                (int)_cellSize.X,
                (int)_cellSize.Y);
    }
}
