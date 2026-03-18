using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Sprites;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDark.Library.Particles.Snowing
{
    public class SnowingManager
    {
        private readonly SnowingLayer _snowingLayer0;
        private readonly SnowingLayer _snowingLayer1;
        private readonly SnowingLayer _snowingLayer2;

        public SnowingManager(Sprite snowFlake, Camera2D camera)
        {
            _snowingLayer0 = new SnowingLayer(
                snowFlake,
                camera,
                0);

            _snowingLayer1 = new SnowingLayer(
                snowFlake,
                camera,
                -250);

            _snowingLayer2 = new SnowingLayer(
                snowFlake,
                camera,
                -400);
        }

        public void Update(TimeSpan elapsed)
        {
            _snowingLayer0.Update(elapsed);
            _snowingLayer1.Update(elapsed);
            _snowingLayer2.Update(elapsed);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _snowingLayer0.Draw(spriteBatch);
            _snowingLayer1.Draw(spriteBatch);
            _snowingLayer2.Draw(spriteBatch);
        }
    }
}
