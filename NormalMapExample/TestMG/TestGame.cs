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

        private RenderTarget2D _baseSceneRenderTarget;
        private RenderTarget2D _normalMapRenderTarget;

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

            _baseSceneRenderTarget = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight);

            _normalMapRenderTarget = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight);
        }

        protected override void Update(GameTime gameTime)
        {
            var mouse = Mouse.GetState();
            var direction = new Vector2(mouse.X, mouse.Y);
            _lightDirection = new Vector3(direction.X, direction.Y, 200);
            _lightDirection.Normalize();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Draws everything normal
            GraphicsDevice.SetRenderTarget(_baseSceneRenderTarget);
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();
            for (int col = 0; col < GraphicsDevice.Viewport.Width; col += _cell.Width)
                for (int row = 0; row < GraphicsDevice.Viewport.Height; row += _cell.Height)
                {
                    _spriteBatch.Draw(_cell, new Vector2(col, row), Color.White);
                }

            _spriteBatch.Draw(_thing, new Vector2(200, 200), Color.White);
            _spriteBatch.End();

            // Drawing the normal map
            GraphicsDevice.SetRenderTarget(_normalMapRenderTarget);
            GraphicsDevice.Clear(Color.Black);
           
            _spriteBatch.Begin();
            for (int col = 0; col < GraphicsDevice.Viewport.Width; col += _cell.Width)
                for (int row = 0; row < GraphicsDevice.Viewport.Height; row += _cell.Height)
                {
                    _spriteBatch.Draw(_cellNormalMap, new Vector2(col, row), Color.White);
                }
            
            // Without normal map, but no problem
            _spriteBatch.Draw(_thing, new Vector2(200, 200), Color.White);
            _spriteBatch.End();

            // Compose postprocessed image
            GraphicsDevice.SetRenderTarget(null);
            _normalMapShader.Parameters["LightDirection"].SetValue(_lightDirection);
            _normalMapShader.Parameters["NormalTexture"].SetValue((Texture2D)_normalMapRenderTarget);
            _normalMapShader.CurrentTechnique.Passes[0].Apply();

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, null, null, _normalMapShader);
            _spriteBatch.Draw((Texture2D)_baseSceneRenderTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
