using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Diagnostics;

namespace Antoids
{
    public static class MathHelper
    {
        private static Random random = new Random();

        public static Vector2 RandomInsideUnitCircle()
        {
            double angle = random.NextDouble() * 2.0 * Math.PI;
            double radius = Math.Sqrt(random.NextDouble());

            double x = radius * Math.Cos(angle);
            double y = radius * Math.Sin(angle);

            return new Vector2((float)x, (float)y);
        }

        public static Vector2 ClampNorm(Vector2 v, float max)
        {
            double f = Math.Min(v.Length(), max);
            f /= v.Length();
            return v * (float)f;
        }
    }
}
