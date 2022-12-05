using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Antoids
{
    public static class Terrain
    {
        private static Random random = new Random();

        private static Noise perlinNoise;

        private const int cellsPerUnit = 5;
        public const int gridWidth = (int)World.worldWidth * cellsPerUnit;
        public const int gridHeight = (int)World.worldHeight * cellsPerUnit;

        private const float frequency = 0.13f;
        private const float amplitude = 100.0f;

        public const float surfaceLevel = 10;
        private static float[,] values = new float[gridWidth, gridHeight];

        private static GraphicsDevice graphicsDevice;

        private static BasicEffect basicEffect;

        private static VertexPositionTexture[] vertices;
        private static int[] indices;

        public static void Init(GraphicsDevice _graphicsDevice)
        {
            graphicsDevice = _graphicsDevice;

            GenerateGridValues();

            Texture2D whiteTexture = new Texture2D(_graphicsDevice, 1, 1);
            whiteTexture.SetData(new[] { Color.SaddleBrown });

            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = whiteTexture;

            GenerateVertices();
        }

        private static void GenerateGridValues()
        {
            int seed = random.Next(100000);

            perlinNoise = new Noise(frequency, amplitude, seed);

            for (int i = 0; i < gridWidth; i++)
            {
                for (int j = 0; j < gridHeight; j++)
                {
                    if (i <= 1 || i >= gridWidth - 2 || j <= 1 || j >= gridHeight - 2)
                        values[i, j] = amplitude;

                    float aspectRatio = World.worldHeight / World.worldWidth;

                    Vector2 worldCenter = new Vector2(World.worldWidth, World.worldHeight) / 2.0f;
                    Vector2 blockPosition = new Vector2(i - World.worldWidth, j - World.worldHeight) / 2.0f;
                    blockPosition.X *= aspectRatio;

                    float distanceToCenter = Vector2.Distance(blockPosition, worldCenter);

                    float limit = 20.0f - (float)Math.Pow(distanceToCenter, 0.75f) * 2.0f;

                    float noiseSample = perlinNoise.Sample(i, j);

                    values[i, j] = noiseSample - limit;
                }
            }
        }

        private static VertexPositionTexture[] squareVertices =
        {
            new VertexPositionTexture(new Vector3(-1.0f / gridWidth, -1.0f / gridHeight, 0.0f), new Vector2(0.0f, 0.0f)),
            new VertexPositionTexture(new Vector3( 1.0f / gridWidth, -1.0f / gridHeight, 0.0f), new Vector2(1.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(-1.0f / gridWidth,  1.0f / gridHeight, 0.0f), new Vector2(0.0f, 1.0f)),
            new VertexPositionTexture(new Vector3( 1.0f / gridWidth,  1.0f / gridHeight, 0.0f), new Vector2(1.0f, 1.0f)),
        };

        private static int[] squareIndices =
        {
            0, 2, 1, 1, 2, 3
        };

        private static void GenerateVertices()
        {
            List<VertexPositionTexture> verts = new List<VertexPositionTexture>();
            List<int> inds = new List<int>();

            int index = 0;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (values[x, y] > 0.0f)
                    {
                        foreach (VertexPositionTexture squareVertex in squareVertices)
                        {
                            VertexPositionTexture vert = squareVertex;
                            vert.Position += new Vector3
                            (
                                (x + 0.5f - (gridWidth / 2.0f)) * (1.0f / gridWidth),
                                -(y + 0.5f - (gridHeight / 2.0f)) * (1.0f / gridHeight),
                                0.0f
                            ) * 2.0f;

                            verts.Add(vert);
                        }
                        foreach (short squareIndex in squareIndices)
                        {
                            inds.Add(squareIndex + index);
                        }

                        index += 4;
                    }
                }
            }

            vertices = verts.ToArray();
            indices = inds.ToArray();
        }

        public static float GetValueAtPoint(float x, float y)
        {
            x = x * cellsPerUnit;
            y = y * cellsPerUnit;

            if (x < 0 || x >= gridWidth) return 0;
            if (y < 0 || y >= gridHeight) return 0;

            return values[(int)x, (int)y];
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (EffectPass effectPass in basicEffect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
            }
        }
    }
}
