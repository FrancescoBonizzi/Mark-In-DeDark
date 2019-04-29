using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mark.Engine
{
    public static class Helper
    {
        public static Random random_;
        public static List<Color> colors_ = new List<Color>() { 
            Color.Chocolate,
            Color.Brown,
            Color.BurlyWood,
            Color.SaddleBrown
        };

        private static int colors_count_;

        static Helper()
        {
            random_ = new Random();
            colors_count_ = colors_.Count();
        }

        public static Color RandomColor()
        {
            return Color.Lerp(
                Color.White,
                colors_[random_.Next(0, colors_count_ - 1)],
                (float)random_.NextDouble());
        }

        public static float RandomBetween(float min, float max)
        {
            return min + (float)random_.NextDouble() * (max - min);
        }
    }
}
