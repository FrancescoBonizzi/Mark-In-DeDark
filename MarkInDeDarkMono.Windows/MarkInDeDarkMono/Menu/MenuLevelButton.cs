using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MarkInDeDarkMono.Menu
{
    public class MenuLevelButton
    {
        private string _upText;
        private readonly Sprite _backgroundSprite;
        private readonly DrawingInfos _drawingInfos;
        private readonly Color _textColor;
        public Rectangle BoxRectangle { get; }

        public Level Level { get; }

        public Rectangle HitBox => new Rectangle(
            (int)(_drawingInfos.Position.X - _drawingInfos.Origin.X), 
            (int)(_drawingInfos.Position.Y - _drawingInfos.Origin.Y),
            BoxRectangle.Width, 
            BoxRectangle.Height);

        private readonly SpriteFont _font;
        private readonly string _bottomText;

        private readonly Vector2 _upTextOrigin;
        private readonly Vector2 _bottomTextOrigin;

        private const float _fontSize = 0.3f;

        public MenuLevelButton(
            AssetsLoader assetsLoader,
            Level level)
        {
            Level = level;
            _font = assetsLoader.Font;

            _drawingInfos = new DrawingInfos();

            if (level.IsCompleted)
            {
                _backgroundSprite = assetsLoader.Sprites["win"];
                var completionTimeRecord = level.RecordCompletionTime == null ? string.Empty : level.RecordCompletionTime.Value.ToMinuteSecondsFormat();
                var candleUsedRecord = level.RecordUsedCandles == null ? string.Empty : level.RecordUsedCandles.Value.ToString();
                _bottomText = $"{candleUsedRecord} - {completionTimeRecord}";
                _textColor = new Color(138, 7, 7);
            }
            else
            {
                _backgroundSprite = assetsLoader.Sprites["win_closed"];
                _bottomText = string.Empty;
                _textColor = Color.White;
            }

            _upText = level.LevelTitleToShow;

            var uptextCenter = _font.GetTextCenter(_upText);
            _upTextOrigin = new Vector2(uptextCenter.X, uptextCenter.Y * 2);

            _bottomTextOrigin = new Vector2(_font.GetTextCenter(_bottomText).X, 0f);

            BoxRectangle = _backgroundSprite.SourceRectangle;
            _drawingInfos.Origin = new Vector2(BoxRectangle.Width / 2.0f, BoxRectangle.Height / 2.0f);
        }

        public void SetPosition(Vector2 newPosition)
            => _drawingInfos.Position = newPosition;

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_backgroundSprite, _drawingInfos);

            spriteBatch.DrawString(
                _font, 
                _upText, 
                new Vector2(
                    _drawingInfos.Position.X,
                    _drawingInfos.Position.Y - _drawingInfos.Origin.Y),
                _textColor,
                0f,
                _upTextOrigin,
                _fontSize,
                SpriteEffects.None,
                0f);

            spriteBatch.DrawString(
                _font,
                _bottomText,
                new Vector2(
                    _drawingInfos.Position.X,
                    _drawingInfos.Position.Y + _drawingInfos.Origin.Y),
                _textColor,
                0f,
                _bottomTextOrigin,
                _fontSize,
                SpriteEffects.None,
                0f);
        }

    }
}
