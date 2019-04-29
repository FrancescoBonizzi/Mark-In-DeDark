#region Using

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine
{
    public static class Camera
    {
        #region Dichiarazioni

        private static Vector2 position_ = Vector2.Zero;
        private static Vector2 viewPortSize_ = Vector2.Zero;
        private static Rectangle worldRectangle_ = new Rectangle(0, 0, 0, 0);

        #endregion

        #region Proprietà

        public static Vector2 Position
        {
            get { return position_; }
            set { position_ = value; }
        }
        
        public static Rectangle WorldRectangle
        {
            get { return worldRectangle_; }
            set { worldRectangle_ = value; }
        }

        public static int ViewPortWidth
        {
            get { return (int)viewPortSize_.X; }
            set { viewPortSize_.X = value; }
        }

        public static int ViewPortHeight
        {
            get { return (int)viewPortSize_.Y; }
            set { viewPortSize_.Y = value; }
        }
        
        public static Rectangle ViewPort
        {
            get
            {
                return new Rectangle(
                (int)Position.X, (int)Position.Y,
                ViewPortWidth, ViewPortHeight);
            }
        }

        #endregion

        #region Metodi
        
        public static void Move(Vector2 offset)
        {
            Position += offset;
        }

        public static bool ObjectIsVisible(Rectangle bounds)
        {
            return (ViewPort.Intersects(bounds));
        }

        public static Vector2 WorldToScreen(Vector2 worldLocation)
        {
            return worldLocation - position_;
        }

        public static Rectangle WorldToScreen(Rectangle worldRectangle)
        {
            return new Rectangle(
                worldRectangle.Left - (int)position_.X,
                worldRectangle.Top - (int)position_.Y,
                worldRectangle.Width,
                worldRectangle.Height);
        }

        public static Vector2 ScreenToWorld(Vector2 screenLocation)
        {
            return screenLocation + position_;
        }

        public static Rectangle ScreenToWorld(Rectangle screenRectangle)
        {
            return new Rectangle(
                screenRectangle.Left + (int)position_.X,
                screenRectangle.Top + (int)position_.Y,
                screenRectangle.Width,
                screenRectangle.Height);
        }

        #endregion
    }
}
