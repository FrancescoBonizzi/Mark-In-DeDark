#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Mark.Engine
{
    public class Animation
    {
        #region Dichiarazioni

        private Texture2D atlas_reference_;

        private List<Rectangle> frames_;

        private float frame_timer_ = 0f;
        private float frame_delay_ = 0.05f;
        private int current_frame_;
        private int last_frame_;

        private bool loop_animation_ = true;
        private bool finished_playing_ = false;
        private string name_;
        private string next_animation_;

        #endregion

        #region Costruttore / Distruttore

        public Animation(
            List<Rectangle> frames,
            string name,
            Texture2D TextureReference)
        {
            name_ = name;

            frames_ = frames;
            atlas_reference_ = TextureReference;

            last_frame_ = frames.Count - 1;
        }

        #endregion

        #region Proprietà

        public Texture2D Texture
        {
            get { return atlas_reference_; }
        }

        public string Name
        {
            get { return name_; }
            set { name_ = value; }
        }

        public string NextAnimation
        {
            get { return next_animation_; }
            set { next_animation_ = value; }
        }

        public bool LoopAnimation
        {
            get { return loop_animation_; }
            set { loop_animation_ = value; }
        }

        public bool FinishedPlaying
        {
            get { return finished_playing_; }
        }

        public int FrameWidth
        {
            get { return frames_[current_frame_].Width; }
        }

        public int FrameHeight
        {
            get { return frames_[current_frame_].Height; }
        }

        public int FrameCount
        {
            get { return frames_.Count; }
        }

        public float FrameLength
        {
            get { return frame_delay_; }
            set { frame_delay_ = value; }
        }

        public Rectangle FrameRectangle
        {
            get { return frames_[current_frame_]; }
        }

        #endregion

        #region Metodi ausiliari

        public void Play()
        {
            current_frame_ = 0;
            finished_playing_ = false;
        }

        public void Stop()
        {
            current_frame_ = 0;
            finished_playing_ = true;
        }

        #endregion

        #region Update

        public void Update(float elapsed)
        {
            frame_timer_ += elapsed;

            if (frame_timer_ >= frame_delay_)
            {
                ++current_frame_;

                //Se sono arrivato all'ultimo frame
                if (current_frame_ >= FrameCount)
                {
                    if (loop_animation_)
                    {
                        current_frame_ = 0;
                    }
                    else
                    {
                        current_frame_ = last_frame_;
                        finished_playing_ = true;
                    }
                }

                frame_timer_ = 0f;
            }
        }

        #endregion
    }
}
