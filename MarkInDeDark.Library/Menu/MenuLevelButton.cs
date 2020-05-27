using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;

using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MarkInDeDark.Library.Menu
{
    public class MenuLevelButton
    {
        private readonly string _upText;
        private readonly Sprite _backgroundSprite;

        private DrawingInfos _upTextDrawingInfos;
        private DrawingInfos _iconDrawingInfos;
        private DrawingInfos _bottomTextDrawingInfos;

        private readonly Color _textColor;
        public Rectangle BoxRectangle { get; }

        public Level Level { get; }

        public Rectangle HitBox => new Rectangle(
            (int)(_iconDrawingInfos.Position.X - _iconDrawingInfos.Origin.X),
            (int)(_iconDrawingInfos.Position.Y - _iconDrawingInfos.Origin.Y),
            BoxRectangle.Width,
            BoxRectangle.Height);

        private readonly SpriteFont _font;
        private readonly string _bottomText;

        private readonly Vector2 _upTextOrigin;
        private readonly Vector2 _bottomTextOrigin;

        private const float _fontSize = 0.3f;
        private readonly float _iconScale = 1.0f;

        public MenuLevelButton(
            AssetsLoader assetsLoader,
            Level level)
        {
            Level = level;
            _font = assetsLoader.Font;

            // Per me qui ci va un'icona accattivamente e ciccia, senza testo
            if (level.IsCompleted)
            {
                _backgroundSprite = assetsLoader.Sprites["Objects/key"];
                _textColor = new Color(138, 7, 7);
#warning Tradurre o mettere un'icona più chiara
                _bottomText = "Completed!";
                _iconScale = 0.7f;
            }
            else
            {
                _backgroundSprite = assetsLoader.Sprites["Grounds/pacco4/Door/door_0"];
                _bottomText = string.Empty;
                _textColor = Color.White;
            }

            _upText = level.LevelTitleToShow;

            _upTextOrigin = _font.GetTextCenter(_upText);
            _bottomTextOrigin = _font.GetTextCenter(_bottomText);

            BoxRectangle = _backgroundSprite.SourceRectangle;

            _upTextDrawingInfos = new DrawingInfos();
            _iconDrawingInfos = new DrawingInfos();
            _bottomTextDrawingInfos = new DrawingInfos();
        }

        public void SetStartingPosition(Vector2 position)
        {
            // TODO Mi fa cagare passato da fuori, lo voglio nel costruttore
            _upTextDrawingInfos = new DrawingInfos()
            {
                Origin = _upTextOrigin,
                Position = position,
                OverlayColor = _textColor,
                Scale = _fontSize
            };

            _iconDrawingInfos = new DrawingInfos()
            {
                Origin = _backgroundSprite.SpriteCenter * _iconScale,
                Position = position + new Vector2(0f, 50f),
                Scale = _iconScale
            };

            _bottomTextDrawingInfos = new DrawingInfos()
            {
                Origin = _bottomTextOrigin,
                Position = position + new Vector2(0f, 140f),
                OverlayColor = _textColor,
                Scale = _fontSize
            };
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, _upText, _upTextDrawingInfos);
            spriteBatch.Draw(_backgroundSprite, _iconDrawingInfos);
            spriteBatch.DrawString(_font, _bottomText, _bottomTextDrawingInfos);
        }

    }
}
