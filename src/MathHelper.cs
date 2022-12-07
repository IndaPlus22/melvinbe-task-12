using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Antoids
{
    // A helper class with various functions that I could not fit anywhere else
    public static class MathHelper
    {
        private static Random random = new Random();

        // Returns any random vector with magnitude less than 1
        public static Vector2 RandomInsideUnitCircle()
        {
            // Get any random angle
            double angle = random.NextDouble() * 2.0 * Math.PI;
            // Get a random distance from 0 to 1
            double radius = Math.Sqrt(random.NextDouble());

            double x = radius * Math.Cos(angle);
            double y = radius * Math.Sin(angle);

            return new Vector2((float)x, (float)y);
        }

        // Limit magnitude of vector
        public static Vector2 ClampNorm(Vector2 v, float max)
        {
            double f = Math.Min(v.Length(), max);
            f /= v.Length();
            return v * (float)f;
        }

        // Linear Interpolation
        public static float Lerp(float a, float b, float x)
        {
            return a * (1 - x) + b * x;
        }

        // Cosine Interpolation
        public static float Cerp(float a, float b, float x)
        {
            double f = (1.0 - Math.Cos(x * Math.PI)) / 2.0;
            return a * (1.0f - (float)f) + b * (float)f;
        }
    }
}
