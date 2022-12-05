using System;
using System.Collections.Generic;

namespace Antoids
{
    public class Noise
    {
        private float frequency;
        private float amplitude;
        private int seed;

        public Noise(float _frequency, float _amplitude, int _seed) 
        {
            frequency = _frequency;
            amplitude = _amplitude;
            seed = _seed;
        }

        public float Sample(float x, float y)
        {
            x *= frequency;
            y *= frequency;

            float a1 = Smooth((int)x,     (int)y    );
            float a2 = Smooth((int)x + 1, (int)y    );
            float a3 = Smooth((int)x,     (int)y + 1);
            float a4 = Smooth((int)x + 1, (int)y + 1);

            float interpX1 = MathHelper.InterpolateCosine(a1, a2, x - (int)x);
            float interpX2 = MathHelper.InterpolateCosine(a3, a4, x - (int)x);

            float interp = MathHelper.InterpolateCosine(interpX1, interpX2, y - (int)y);

            return interp * amplitude;
        }

        private float NoiseValue(int x, int y)
        {
            if (x == 0 ||
                x == (int)(Terrain.gridWidth * frequency) ||
                y == 0 ||
                y == (int)(Terrain.gridHeight * frequency)) 
                return 1.0f;

            int n = (x * 1619 + y * 31337 * 1013 * seed) & 0x7fffffff;
            n = (n << 13) ^ n;
            return 1.0f - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f;
        }

        private float Smooth(int x, int y)
        {
            float corners = NoiseValue(x - 1, y - 1) + NoiseValue(x + 1, y - 1) + NoiseValue(x - 1, y + 1) + NoiseValue(x + 1, y + 1);

            float sides = NoiseValue(x - 1, y) + NoiseValue(x + 1, y) + NoiseValue(x, y - 1) + NoiseValue(x, y + 1);

            float center = NoiseValue(x, y) / 4;

            return corners / 16.0f + sides / 8.0f + center / 4.0f;
        }
    }
}
