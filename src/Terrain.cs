using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Antoids
{
    // Cave terrain represented by a grid of values
    public static class Terrain
    {
        private static Random random = new Random();

        // Noise values
        private const float frequency = 0.12f;
        private const float amplitude = 100.0f;
        private static Noise perlinNoise;

        // The amount of blocks or "cells" per world unit
        public const int cellsPerWorldUnit = 5;
        // Dimensions of world in cells
        public const int gridWidth = (int)World.worldWidth * cellsPerWorldUnit;
        public const int gridHeight = (int)World.worldHeight * cellsPerWorldUnit;

        // Cell value greater than 0 <=> terrain exists in that cell
        public static float[,] values = new float[gridWidth, gridHeight];

        // Same GraphicsDevice as in simulation class
        private static GraphicsDevice graphicsDevice;

        // Basic shader for drawing primitives
        private static BasicEffect basicEffect;

        // Vertices of terrain mesh
        private static VertexPositionTexture[] vertices;
        // Describes which vertices form triangels
        private static int[] indices;

        public static void Init(GraphicsDevice _graphicsDevice)
        {
            graphicsDevice = _graphicsDevice;

            GenerateGridValues();

            // Create new 1 by 1 terrain colored texture
            Texture2D whiteTexture = new Texture2D(_graphicsDevice, 1, 1);
            whiteTexture.SetData(new[] { Simulation.terrainColor });

            // Create and set texture of basic shader
            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = whiteTexture;

            GenerateVertices();
        }

        // Generate new values
        public static void GenerateGridValues()
        {
            int seed = random.Next(100000);

            perlinNoise = new Noise(frequency, amplitude, seed);

            for (int i = 0; i < gridWidth; i++)
            {
                for (int j = 0; j < gridHeight; j++)
                {
                    // Guarantee that edges are solid 
                    if (i <= 1 || i > gridWidth - 1 || j <= 1 || j > gridHeight - 1)
                        values[i, j] = amplitude;

                    float aspectRatio = World.worldHeight / World.worldWidth;

                    // Translate positions to make them more easier to compare:
                    Vector2 worldCenter = new Vector2(World.worldWidth, World.worldHeight) / 2.0f;
                    Vector2 blockPosition = new Vector2(i - World.worldWidth, j - World.worldHeight) / 2.0f;
                    blockPosition.X *= aspectRatio;

                    // Create more empty space closer to middle of world:
                    float distanceToCenter = Vector2.Distance(blockPosition, worldCenter);

                    float offset = 20.0f - (float)Math.Pow(distanceToCenter, 0.75f) * 2.0f;

                    float noiseSample = perlinNoise.Sample(i, j);

                    values[i, j] = noiseSample - offset;
                }
            }
        }

        // Update mesh by generating new triangles
        public static void GenerateVertices()
        {
            MarchingSquares.GetData(ref values, ref vertices, ref indices);
        }

        // Return value of terrain at a point in world space
        public static float GetValueAtWorldPoint(float x, float y)
        {
            // Convert cell space to world space
            x = x * cellsPerWorldUnit;
            y = y * cellsPerWorldUnit;

            // return -1 if sampling outside world
            if (x < 0.0f || x >= gridWidth) return -1.0f;
            if (y < 0.0f || y >= gridHeight) return -1.0f;

            return values[(int)x, (int)y];
        }

        // Draw several triangles
        public static void Draw()
        {
            // Apply shader
            foreach (EffectPass effectPass in basicEffect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
            }
        }
    }
}
