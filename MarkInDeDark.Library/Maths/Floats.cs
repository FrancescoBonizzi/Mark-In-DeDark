using System;

namespace MarkInDeDark.Library.Maths
{
    public static class Floats
    {
        public static bool IsEqualToInRange(this float a, float b, float range = 1f / 100f)
            => Math.Abs(a - b) <= range;

        public static bool GreaterThanInRange(this float a, float b, float range = 1f / 100f)
        {
            if (a > b)
                return !a.IsEqualToInRange(b, range);
            return false;
        }

        public static bool LessThanInRange(this float a, float b, float range = 1f / 100f)
        {
            if (a < b)
                return !a.IsEqualToInRange(b, range);
            return false;
        }

        public static bool LessThanOrClose(this float a, float b, float range = 1f / 100f)
        {
            if (a >= b)
                return a.IsEqualToInRange(b, range);
            return true;
        }

        public static bool GreaterThanOrClose(this float a, float b, float range = 1f / 100f)
        {
            if (a <= b)
                return a.IsEqualToInRange(b, range);
            return true;
        }
    }
}
