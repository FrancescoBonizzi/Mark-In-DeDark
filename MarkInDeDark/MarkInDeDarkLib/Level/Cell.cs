#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

#endregion

namespace Mark.Engine
{
    public class Cell
    {
        #region Dichiarazioni

        private Vector2 position_;

        private Texture2D atlas_;
        private Rectangle cell_rectangle_;
        private Rectangle occlusion_rectangle_;
        private Rectangle angle_rectangle_;

        private Rectangle blood_decal_ = Rectangle.Empty;

        private Rectangle dirty_texture_;
        private List<Vector2> dirty_position_;

        private int width_;
        private float occlusion_rectangle_h_over2_;
        private float angle_rectangle_h_over2_;

        // Per vedere dove si può passare
        private bool north_occlusion_ = false;
        private bool east_occlusion_ = false;
        private bool south_occlusion_ = false;
        private bool west_occlusion_ = false;

        private Cell[] links_;
        public List<Vector2> angle_positions_ { get; set; }
        public List<Wall> walls_ { get; set; }

        private Color wall_overlay_color_ = Color.White;
        private Color angle_overlay_color_ = Color.White;
        private Color cell_overlay_color_;

        private bool skip_in_graph_ = false;

        #endregion

        #region Costruttore

        public Cell(
            Vector2 StartingPosition,
            Rectangle CellTexture,
            Rectangle WallTexture,
            Rectangle AngleTexture,
            Rectangle DirtyTexture,
            double DirtyProbability)
        {
            this.position_ = StartingPosition;
            this.cell_rectangle_ = CellTexture;
            this.occlusion_rectangle_ = WallTexture;
            this.atlas_ = Loader.atlas_;
            this.width_ = CellTexture.Width;
            this.occlusion_rectangle_h_over2_ = WallTexture.Height / 2;
            this.angle_rectangle_h_over2_ = AngleTexture.Height / 2;

            this.cell_overlay_color_ = Color.White;

            this.angle_rectangle_ = AngleTexture;
            this.links_ = new Cell[4];
            this.angle_positions_ = new List<Vector2>();

            this.dirty_texture_ = DirtyTexture;
            this.dirty_position_ = new List<Vector2>();
            for (int i = 0; i < 5; ++i)
            {
                if (Helper.random_.NextDouble() <= DirtyProbability)
                {
                    this.dirty_position_.Add(new Vector2(
                        position_.X + Helper.random_.Next(0, width_ - DirtyTexture.Width / 2),
                        position_.Y + Helper.random_.Next(0, width_ - DirtyTexture.Height / 2)));
                }
            }
        }

        #endregion

        #region Get/Set

        public bool SkipForGraph
        {
            get { return skip_in_graph_; }
            set
            {
                skip_in_graph_ = value;
                if (skip_in_graph_)
                    dirty_texture_ = Rectangle.Empty;
            }
        }

        public Rectangle CellTexture
        {
            set
            {
                cell_rectangle_ = value;
                this.cell_overlay_color_ = Color.White;
            }
        }

        public Vector2 WorldLocation
        {
            get { return position_; }
        }

        public Cell[] Links
        {
            get { return links_; }
        }

        #endregion

        #region Metodi

        public bool IsNorthPassable
        {
            get { return !north_occlusion_; }
            set { north_occlusion_ = !value; }
        }

        public bool IsEastPassable
        {
            get { return !east_occlusion_; }
            set { east_occlusion_ = !value; }
        }

        public bool IsSouthPassable
        {
            get { return !south_occlusion_; }
            set { south_occlusion_ = !value; }
        }

        public bool IsWestPassable
        {
            get { return !west_occlusion_; }
            set { west_occlusion_ = !value; }
        }

        public void GenerateWalls()
        {
            this.walls_ = new List<Wall>();
            if (north_occlusion_)
            {
                walls_.Add(new Wall(
                    new Vector2(position_.X, position_.Y - occlusion_rectangle_h_over2_),
                    0.0f));
            }

            if (east_occlusion_)
            {
                walls_.Add(new Wall(
                    new Vector2(position_.X + width_ + occlusion_rectangle_h_over2_, position_.Y),
                    MathHelper.PiOver2));
            }

            // Ogni south è la north di qualcun altro!
            if (south_occlusion_)
            {
                walls_.Add(new Wall(
                    new Vector2(position_.X, position_.Y + width_ - occlusion_rectangle_h_over2_),
                    0.0f));
            }

            if (west_occlusion_)
            {
                walls_.Add(new Wall(
                  new Vector2(position_.X + occlusion_rectangle_h_over2_, position_.Y),
                   MathHelper.PiOver2));
            }
        }

        public void AddLink(Cell c, int Pos)
        {
            links_[Pos] = c;
        }

        public void ApplyBloodDecal()
        {
            this.blood_decal_ = Loader.blood_decal_;
        }

        public void AddNWAngle()
        {
            angle_positions_.Add(
                new Vector2(
                    position_.X - angle_rectangle_h_over2_,
                    position_.Y - angle_rectangle_h_over2_));
        }

        public void AddNEAngle()
        {
            angle_positions_.Add(
                  new Vector2(
                      position_.X + width_ - angle_rectangle_h_over2_,
                      position_.Y - angle_rectangle_h_over2_));
        }

        public void AddSWAngle()
        {
            angle_positions_.Add(
                new Vector2(
                    position_.X - angle_rectangle_h_over2_,
                    position_.Y + width_ - angle_rectangle_h_over2_));
        }

        public void AddSEAngle()
        {
            angle_positions_.Add(
                new Vector2(
                    position_.X + width_ - angle_rectangle_h_over2_,
                    position_.Y + width_ - angle_rectangle_h_over2_));
        }

        public void SetSimpleAngles()
        {
            if (north_occlusion_ && east_occlusion_)
                AddNEAngle();
            if (north_occlusion_ && west_occlusion_)
                AddNWAngle();
            if (south_occlusion_ && east_occlusion_)
                AddSEAngle();
            if (south_occlusion_ && west_occlusion_)
                AddSWAngle();
        }

        #endregion

        #region Update / Draw

        public void Update(float dt)
        {
        }

        public void DrawCell(SpriteBatch spritebatch_)
        {
            // Tutti questi if si possono tenere, tanto alla fine di Draw per le celle ne eseguo poche
            // (perchè disegno solo le celle nella viewport della telecamera)
            Vector2 screen_location_ = Camera.WorldToScreen(position_);

            spritebatch_.Draw(
                atlas_,
                screen_location_,
                cell_rectangle_,
                cell_overlay_color_);

            foreach (Vector2 v in dirty_position_)
            {                
                spritebatch_.Draw(
                    atlas_,
                    Camera.WorldToScreen(v),
                    dirty_texture_,
                    Color.White);
            }

            if (blood_decal_ != Rectangle.Empty)
            {
                spritebatch_.Draw(
                    atlas_,
                    screen_location_,
                    blood_decal_,
                    Color.White);
            }
        }

        public void DrawWalls(SpriteBatch spritebatch_)
        {
            foreach (Wall w in walls_)
            {
                spritebatch_.Draw(
                    atlas_,
                    Camera.WorldToScreen(w.location),
                    occlusion_rectangle_,
                    Color.White,
                    w.rotation,
                    Vector2.Zero,
                    1.0f,
                    SpriteEffects.None,
                    0.0f);
            }
        }

        // E' una soluzione dispendiosa, perchè bisogna scorrere di nuovo tutte le celle,
        // ma se funziona bene per me è più comoda che mettere tutti i layerDepth
        public void DrawAngles(SpriteBatch spritebatch_)
        {
            #region Angoli

            foreach (Vector2 v in angle_positions_)
            {
                spritebatch_.Draw(atlas_,
                    Camera.WorldToScreen(v),
                    angle_rectangle_,
                    angle_overlay_color_);
            }

            #endregion
        }

        public class Wall
        {
            public Wall(Vector2 Location, float Rotation)
            {
                this.location = Location;
                this.rotation = Rotation;
            }

            public Vector2 location { get; set; }
            public float rotation { get; set; }
        }


        #endregion
    }
}
