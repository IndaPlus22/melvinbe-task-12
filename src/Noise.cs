using System;
using System.Collections.Generic;

namespace Antoids
{
    public class Noise
    {
        private float frequency; // How zoomed in or out sampling is
        private float amplitude; // Scale of noise values
        private int seed; // Gives a unique noise map for unique seed

        public Noise(float frequency, float amplitude, int seed) 
        {
            this.frequency = frequency;
            this.amplitude = amplitude;
            this.seed = seed;
        }

        // Returns value of noise map at position:
        public float Sample(float x, float y)
        {
            // Zoom position by frequency
            x *= frequency;
            y *= frequency;

            // Get smooth neighboring values:
            float nb1 = Smooth((int)x,     (int)y    );
            float nb2 = Smooth((int)x + 1, (int)y    );
            float nb3 = Smooth((int)x,     (int)y + 1);
            float nb4 = Smooth((int)x + 1, (int)y + 1);

            // Interpolate between all neighbors using sample decimals:
            float interpX1 = MathHelper.Cerp(nb1, nb2, x - (int)x);
            float interpX2 = MathHelper.Cerp(nb3, nb4, x - (int)x);

            float interp = MathHelper.Cerp(interpX1, interpX2, y - (int)y);

            // Return interpolated value scaled by amplitude
            return interp * amplitude;
        }

        // Returns a chaotic value based on input,
        // unless input is on edge of world:
        private float NoiseValue(int x, int y)
        {
            // Always return 1.25 at world edges
            // Yes I know, this does limit the use of the noise class to this simulation,
            // but doing this here before smoothing creates nicer edges
            if (x == 0 ||
                x == (int)(Terrain.gridWidth * frequency) ||
                y == 0 ||
                y == (int)(Terrain.gridHeight * frequency)) 
                return 1.25f;

            // Do some unspeakable things to values using large primes
            int n = (x * 1619 + y * 31337 * 1013 * seed) & 0x7fffffff;
            n = (n << 13) ^ n;
            return 1.0f - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f;
        }

        private float Smooth(int x, int y)
        {
            // Sample neighboring corners
            float corners = NoiseValue(x - 1, y - 1) + NoiseValue(x + 1, y - 1) + NoiseValue(x - 1, y + 1) + NoiseValue(x + 1, y + 1);

            // Sample neighboring edges
            float sides = NoiseValue(x - 1, y) + NoiseValue(x + 1, y) + NoiseValue(x, y - 1) + NoiseValue(x, y + 1);

            // Sample center
            float center = NoiseValue(x, y);

            // Return sum with center valued most and corners least, for smoothness
            return center / 4.0f + sides / 8.0f + corners / 16.0f;
        }
    }
}
