using System;

namespace MarkInDeDarkMono.Maths
{
    public static class Floats
    {
        public static bool IsEqualToInRange(this float a, float b, float range = 1f / 100f)
            => Math.Abs(a - b) <= range;
    }
}
