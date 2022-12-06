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

        public const int cellsPerUnit = 5;
        public const int gridWidth = (int)World.worldWidth * cellsPerUnit;
        public const int gridHeight = (int)World.worldHeight * cellsPerUnit;
        private const float cwos = 1.0f / gridWidth;
        private const float chos = 1.0f / gridHeight;

        private const float frequency = 0.12f;
        private const float amplitude = 100.0f;

        public const float surfaceLevel = 10;
        public static float[,] values = new float[gridWidth, gridHeight];

        private static GraphicsDevice graphicsDevice;

        private static BasicEffect basicEffect;

        private static VertexPositionTexture[] vertices;
        private static int[] indices;

        public static void Init(GraphicsDevice _graphicsDevice)
        {
            graphicsDevice = _graphicsDevice;

            GenerateGridValues();

            Texture2D whiteTexture = new Texture2D(_graphicsDevice, 1, 1);
            whiteTexture.SetData(new[] { Simulation.wallColor });

            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = whiteTexture;

            GenerateVertices();
        }

        public static void GenerateGridValues()
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

        private static List<VertexPositionTexture>[] squareVertices =
        {
            new List<VertexPositionTexture>(), // 0

            new List<VertexPositionTexture> { // 1
                new VertexPositionTexture(new Vector3(-cwos,  0.0f, 0.0f), new Vector2(0.0f, 0.0f)), // Left
                new VertexPositionTexture(new Vector3( 0.0f, -chos, 0.0f), new Vector2(1.0f, 0.0f)), // Bottom 
                new VertexPositionTexture(new Vector3(-cwos, -chos, 0.0f), new Vector2(0.0f, 1.0f)), // Left Bottom
            },
            new List<VertexPositionTexture> { // 2
                new VertexPositionTexture(new Vector3( cwos,  0.0f, 0.0f), new Vector2(0.0f, 0.0f)), // Right
                new VertexPositionTexture(new Vector3( 0.0f, -chos, 0.0f), new Vector2(1.0f, 0.0f)), // Bottom 
                new VertexPositionTexture(new Vector3( cwos, -chos, 0.0f), new Vector2(0.0f, 1.0f)), // Right Bottom
            },
            new List<VertexPositionTexture> { // 3
                new VertexPositionTexture(new Vector3(-cwos, -chos, 0.0f), new Vector2(0.0f, 0.0f)), // Left Bottom
                new VertexPositionTexture(new Vector3( cwos, -chos, 0.0f), new Vector2(1.0f, 0.0f)), // Right Bottom 
                new VertexPositionTexture(new Vector3(-cwos,  0.0f, 0.0f), new Vector2(0.0f, 1.0f)), // Left 
                new VertexPositionTexture(new Vector3( cwos,  0.0f, 0.0f), new Vector2(1.0f, 1.0f)), // Right
            },
            new List<VertexPositionTexture> { // 4
                new VertexPositionTexture(new Vector3( cwos,  0.0f, 0.0f), new Vector2(0.0f, 0.0f)), // Right
                new VertexPositionTexture(new Vector3( 0.0f,  chos, 0.0f), new Vector2(1.0f, 0.0f)), // Top 
                new VertexPositionTexture(new Vector3( cwos,  chos, 0.0f), new Vector2(0.0f, 1.0f)), // Right Top
            },
            new List<VertexPositionTexture> { // 5
                new VertexPositionTexture(new Vector3(-cwos,  0.0f, 0.0f), new Vector2(0.0f, 0.0f)), // Left
                new VertexPositionTexture(new Vector3( 0.0f, -chos, 0.0f), new Vector2(1.0f, 0.0f)), // Bottom 
                new VertexPositionTexture(new Vector3(-cwos, -chos, 0.0f), new Vector2(0.0f, 1.0f)), // Left Bottom
                new VertexPositionTexture(new Vector3( cwos,  0.0f, 0.0f), new Vector2(0.0f, 0.0f)), // Right
                new VertexPositionTexture(new Vector3( 0.0f,  chos, 0.0f), new Vector2(1.0f, 0.0f)), // Top 
                new VertexPositionTexture(new Vector3( cwos,  chos, 0.0f), new Vector2(0.0f, 1.0f)), // Right Top
            },
            new List<VertexPositionTexture> { // 6
                new VertexPositionTexture(new Vector3( 0.0f, -chos, 0.0f), new Vector2(0.0f, 0.0f)), // Bottom
                new VertexPositionTexture(new Vector3( cwos, -chos, 0.0f), new Vector2(1.0f, 0.0f)), // Right Bottom
                new VertexPositionTexture(new Vector3( 0.0f,  chos, 0.0f), new Vector2(0.0f, 1.0f)), // Top
                new VertexPositionTexture(new Vector3( cwos,  chos, 0.0f), new Vector2(1.0f, 1.0f)), // Right Top
            },
            new List<VertexPositionTexture> { // 7
                new VertexPositionTexture(new Vector3(-cwos,  0.0f, 0.0f), new Vector2(0.0f, 0.0f)), // Left
                new VertexPositionTexture(new Vector3( cwos, -chos, 0.0f), new Vector2(0.0f, 1.0f)), // Right Bottom
                new VertexPositionTexture(new Vector3(-cwos, -chos, 0.0f), new Vector2(0.0f, 1.0f)), // Left Bottom
                new VertexPositionTexture(new Vector3( 0.0f,  chos, 0.0f), new Vector2(0.0f, 1.0f)), // Top
                new VertexPositionTexture(new Vector3( cwos,  chos, 0.0f), new Vector2(1.0f, 1.0f)), // Right Top
            },
            new List<VertexPositionTexture> { // 8
                new VertexPositionTexture(new Vector3(-cwos,  0.0f, 0.0f), new Vector2(0.0f, 0.0f)), // Left
                new VertexPositionTexture(new Vector3( 0.0f,  chos, 0.0f), new Vector2(1.0f, 0.0f)), // Top 
                new VertexPositionTexture(new Vector3(-cwos,  chos, 0.0f), new Vector2(0.0f, 1.0f)), // Left Top
            },
            new List<VertexPositionTexture> { // 9
                new VertexPositionTexture(new Vector3( 0.0f, -chos, 0.0f), new Vector2(0.0f, 0.0f)), // Bottom
                new VertexPositionTexture(new Vector3(-cwos, -chos, 0.0f), new Vector2(1.0f, 0.0f)), // Left Bottom
                new VertexPositionTexture(new Vector3( 0.0f,  chos, 0.0f), new Vector2(0.0f, 1.0f)), // Top
                new VertexPositionTexture(new Vector3(-cwos,  chos, 0.0f), new Vector2(1.0f, 1.0f)), // Left Top
            },
            new List<VertexPositionTexture> { // 10
                new VertexPositionTexture(new Vector3( cwos,  0.0f, 0.0f), new Vector2(0.0f, 0.0f)), // Right
                new VertexPositionTexture(new Vector3( 0.0f, -chos, 0.0f), new Vector2(1.0f, 0.0f)), // Bottom 
                new VertexPositionTexture(new Vector3( cwos, -chos, 0.0f), new Vector2(0.0f, 1.0f)), // Right Bottom
                new VertexPositionTexture(new Vector3(-cwos,  0.0f, 0.0f), new Vector2(0.0f, 0.0f)), // Left
                new VertexPositionTexture(new Vector3( 0.0f,  chos, 0.0f), new Vector2(1.0f, 0.0f)), // Top 
                new VertexPositionTexture(new Vector3(-cwos,  chos, 0.0f), new Vector2(0.0f, 1.0f)), // Left Top
            },
            new List<VertexPositionTexture> { // 11
                new VertexPositionTexture(new Vector3( cwos,  0.0f, 0.0f), new Vector2(0.0f, 0.0f)), // Right
                new VertexPositionTexture(new Vector3(-cwos, -chos, 0.0f), new Vector2(0.0f, 1.0f)), // Left Bottom
                new VertexPositionTexture(new Vector3( cwos, -chos, 0.0f), new Vector2(0.0f, 1.0f)), // Right Bottom
                new VertexPositionTexture(new Vector3( 0.0f,  chos, 0.0f), new Vector2(0.0f, 1.0f)), // Top
                new VertexPositionTexture(new Vector3(-cwos,  chos, 0.0f), new Vector2(1.0f, 1.0f)), // Left Top
            },
            new List<VertexPositionTexture> { // 12
                new VertexPositionTexture(new Vector3(-cwos,  chos, 0.0f), new Vector2(0.0f, 0.0f)), // Left Top
                new VertexPositionTexture(new Vector3( cwos,  chos, 0.0f), new Vector2(1.0f, 0.0f)), // Right Top 
                new VertexPositionTexture(new Vector3(-cwos,  0.0f, 0.0f), new Vector2(0.0f, 1.0f)), // Left 
                new VertexPositionTexture(new Vector3( cwos,  0.0f, 0.0f), new Vector2(1.0f, 1.0f)), // Right
            },
            new List<VertexPositionTexture> { // 13
                new VertexPositionTexture(new Vector3( cwos,  0.0f, 0.0f), new Vector2(0.0f, 0.0f)), // Right
                new VertexPositionTexture(new Vector3(-cwos,  chos, 0.0f), new Vector2(0.0f, 1.0f)), // Left Top
                new VertexPositionTexture(new Vector3( cwos,  chos, 0.0f), new Vector2(0.0f, 1.0f)), // Right Top
                new VertexPositionTexture(new Vector3( 0.0f, -chos, 0.0f), new Vector2(0.0f, 1.0f)), // Bottom
                new VertexPositionTexture(new Vector3(-cwos, -chos, 0.0f), new Vector2(1.0f, 1.0f)), // Left Bottom
            },
            new List<VertexPositionTexture> { // 14
                new VertexPositionTexture(new Vector3(-cwos,  0.0f, 0.0f), new Vector2(0.0f, 0.0f)), // Left
                new VertexPositionTexture(new Vector3( cwos,  chos, 0.0f), new Vector2(0.0f, 1.0f)), // Right Top
                new VertexPositionTexture(new Vector3(-cwos,  chos, 0.0f), new Vector2(0.0f, 1.0f)), // Left Top
                new VertexPositionTexture(new Vector3( 0.0f, -chos, 0.0f), new Vector2(0.0f, 1.0f)), // Bottom
                new VertexPositionTexture(new Vector3( cwos, -chos, 0.0f), new Vector2(1.0f, 1.0f)), // Right Bottom
            },
            new List<VertexPositionTexture> { // 15
                new VertexPositionTexture(new Vector3(-cwos, -chos, 0.0f), new Vector2(0.0f, 0.0f)), // Top Left
                new VertexPositionTexture(new Vector3( cwos, -chos, 0.0f), new Vector2(1.0f, 0.0f)), // Left Bottom
                new VertexPositionTexture(new Vector3(-cwos,  chos, 0.0f), new Vector2(0.0f, 1.0f)), // Top Right
                new VertexPositionTexture(new Vector3( cwos,  chos, 0.0f), new Vector2(1.0f, 1.0f)), // Right Bottom
            }
        };

        private static List<int>[] squareIndices =
        {
            new List<int>(),                             // 0
            new List<int> { 0, 1, 2                   }, // 1
            new List<int> { 0, 2, 1                   }, // 2
            new List<int> { 0, 2, 1, 1, 2, 3          }, // 3
            new List<int> { 0, 1, 2                   }, // 4
            new List<int> { 0, 1, 2, 3, 4, 5          }, // 5
            new List<int> { 0, 2, 1, 1, 2, 3          }, // 6
            new List<int> { 0, 1, 2, 3, 1, 0, 3, 4, 1 }, // 7
            new List<int> { 0, 2, 1                   }, // 8
            new List<int> { 0, 1, 2, 1, 3, 2          }, // 9
            new List<int> { 0, 2, 1, 3, 5, 4          }, // 10
            new List<int> { 0, 2, 1, 3, 0, 1, 3, 1, 4 }, // 11
            new List<int> { 0, 1, 2, 1, 3, 2          }, // 12
            new List<int> { 0, 1, 2, 3, 1, 0, 3, 4, 1 }, // 13 
            new List<int> { 0, 2, 1, 3, 0, 1, 3, 1, 4 }, // 14
            new List<int> { 0, 2, 1, 1, 2, 3          }, // 15
        };

        public static void GenerateVertices()
        {
            List<VertexPositionTexture> verts = new List<VertexPositionTexture>();
            List<int> inds = new List<int>();

            int index = 0;

            for (int x = 0; x < gridWidth - 1; x++)
            {
                for (int y = 0; y < gridHeight - 1; y++)
                {
                    int state = 0;
                    if (values[x,     y    ] > 0.0f) state += 8;
                    if (values[x + 1, y    ] > 0.0f) state += 4;
                    if (values[x + 1, y + 1] > 0.0f) state += 2;
                    if (values[x,     y + 1] > 0.0f) state += 1;

                    if (state == 0) continue;

                    foreach (VertexPositionTexture squareVertex in squareVertices[state])
                    {
                        VertexPositionTexture vert = squareVertex;

                        if (vert.Position.X == 0.0f)
                        {
                            if (vert.Position.Y > 0.0f)
                            {
                                float t = 1.0f - (0.0f - values[x, y]) / (values[x + 1, y] - values[x, y]);
                                //t = 0.5f;
                                vert.Position.X = (1.0f - (t * 2.0f)) * cwos;
                            }
                            else
                            {
                                float t = 1.0f - (0.0f - values[x, y + 1]) / (values[x + 1, y + 1] - values[x, y + 1]);
                                //t = 0.5f;
                                vert.Position.X = (1.0f - (t * 2.0f)) * cwos;
                            }
                        }
                        else if (vert.Position.Y == 0.0f)
                        {
                            if (vert.Position.X < 0.0f)
                            {
                                float t = (0.0f - values[x, y]) / (values[x, y + 1] - values[x, y]);
                                //t = 0.5f;
                                vert.Position.Y = (1.0f - (t * 2.0f)) * chos;
                            }
                            else
                            {
                                float t = (0.0f - values[x + 1, y]) / (values[x + 1, y + 1] - values[x + 1, y]);
                                //t = 0.5f;
                                vert.Position.Y = (1.0f - (t * 2.0f)) * chos;
                            }
                        }

                        vert.Position += new Vector3
                        (
                            (x + 1.0f - (gridWidth  / 2.0f)) * cwos * 2.0f,
                           -(y + 1.0f - (gridHeight / 2.0f)) * chos * 2.0f,
                            0.0f
                        );

                        verts.Add(vert);
                    }
                    foreach (short squareIndex in squareIndices[state])
                    {
                        inds.Add(squareIndex + index);
                    }

                    index += squareVertices[state].Count;
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
