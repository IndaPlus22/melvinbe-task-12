using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Antoids
{
    public class Terrain
    {
        private const int pointsPerUnit = 5;
        private const int width = (int)World.worldWidth * pointsPerUnit;
        private const int height = (int)World.worldHeight * pointsPerUnit;

        public float[,] values = new float[width, height];

        public Terrain()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (i <= 5 || i >= width - 5 || j <= 5 || j >= height - 5)
                    {
                        values[i, j] = 10.0f;
                    }
                }
            }
        }

        public float GetValueAtPoint(Vector2 position)
        {
            position *= pointsPerUnit;
            position += Vector2.One * 0.5f;

            return values[(int)position.X, (int)position.Y];
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (values[i, j] > 0.0f)
                    {
                        World.DrawCircle(spriteBatch, new Vector2(i, j) / pointsPerUnit, Color.SaddleBrown, 1.0f / pointsPerUnit);
                    }
                }
            }
        }
    }
}
