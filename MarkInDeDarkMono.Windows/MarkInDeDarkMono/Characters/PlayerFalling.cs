
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDarkMono.Characters
{
    //public class PlayerFalling
    //{
    //    private TimeSpan default_interval_ = TimeSpan.FromMilliseconds(35);
    //    private TimeSpan animation_interval_ = TimeSpan.FromMilliseconds(35);
    //    private float fade_increment_ = -0.009f;
    //    private float scale_increment_ = -0.02f;
    //    private float rotation_increment_ = -0.04f;
    //    private float opacity_ = 1.0f;

    //    public PlayerFalling(
    //        Vector2 StartingPosition)
    //    {
    //        this.world_location_ = StartingPosition + new Vector2(frame_width_ / 2, frame_heigth_ / 2);
    //    }

    //    protected override void LoadEveryAnimation()
    //    {
    //        LoadAnimation("rotate", AssetsLoader.p_rotating_, true, TimeSpan.FromMilliseconds(200), AssetsLoader._atlas);
    //        PlayAnimation("rotate");

    //        this.frame_width_ = AssetsLoader.p_rotating_[0].Width;
    //        this.frame_heigth_ = AssetsLoader.p_rotating_[0].Height;
    //        this.frame_center_ = new Vector2(this.frame_width_ / 2, this.frame_heigth_ / 2);
    //        this.origin_ = new Vector2(frame_width_ / 2, frame_heigth_ / 2);
    //    }

    //    public override void Update(TimeSpan elapsed)
    //    {
    //        animation_interval_ -= elapsed;

    //        if (animation_interval_ <= TimeSpan.Zero)
    //        {
    //            animation_interval_ = default_interval_;
    //            opacity_ += fade_increment_;
    //            this.scale_ += scale_increment_;
    //            this._overlay_color *= opacity_;
    //            this.rotation_ += rotation_increment_;
    //        }

    //        base.Update(elapsed);
    //    }

    //    public override void Draw(SpriteBatch spriteBatch)
    //    {
    //        base.Draw(spriteBatch);
    //    }
    //}
}
