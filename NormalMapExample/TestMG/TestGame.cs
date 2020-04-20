using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace TestMG
{
    public class TestGame : Game
    {
        public GraphicsDeviceManager GraphicsDeviceManager { get; private set; }
        private SpriteBatch _spriteBatch;

        private Texture2D _cell;
        private Texture2D _cellNormalMap;
        private Texture2D _thing;
        private Effect _normalMapShader;
        private Vector3 _lightDirection;

        public TestGame()
        {
            GraphicsDeviceManager = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            Content.RootDirectory = "Content/bin";
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _cell = Content.Load<Texture2D>("cell");
            _cellNormalMap = Content.Load<Texture2D>("cell_normal");
            _normalMapShader = Content.Load<Effect>("normalmap");
            _thing = Content.Load<Texture2D>("thing");

            //_normalMapRenderTarget = new RenderTarget2D(
            //    GraphicsDevice,
            //    GraphicsDevice.PresentationParameters.BackBufferWidth,
            //    GraphicsDevice.PresentationParameters.BackBufferHeight);
        }

        protected override void Update(GameTime gameTime)
        {
            var mouse = Mouse.GetState();
            var dir = new Vector2(
                mouse.X,
                mouse.Y);
            dir.Normalize();
            _lightDirection = new Vector3(dir.X, dir.Y, 1f);
            _lightDirection.Normalize();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Drawing the ground with its normal map
            _normalMapShader.Parameters["LightDirection"].SetValue(_lightDirection);
            _normalMapShader.Parameters["NormalTexture"].SetValue(_cellNormalMap);
            _normalMapShader.CurrentTechnique.Passes[0].Apply();

            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, _normalMapShader);
            for (int col = 0; col < GraphicsDevice.Viewport.Width; col += _cell.Width)
                for (int row = 0; row < GraphicsDevice.Viewport.Height; row += _cell.Height)
                {
                    _spriteBatch.Draw(_cell, new Vector2(col, row), Color.White);
                }
            _spriteBatch.End();

            // Drawing the object 
            _spriteBatch.Begin();
            _spriteBatch.Draw(_thing, new Vector2(200, 200), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
