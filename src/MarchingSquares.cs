using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Antoids
{
    public static class MarchingSquares
    {
        // Cell width/height on screen. Names are same length as "1.0f" to make squareVertices list readable
        private const float cwos = 1.0f / Terrain.gridWidth;
        private const float chos = 1.0f / Terrain.gridHeight;

        // Can be set to false to demonstrate what marching squares looks like without interpolation
        private const bool interpolateVertices = true;

        // Every configuration of triangle vertices with UV coordinates
        public static List<VertexPositionTexture>[] squareVertices =
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

        // Corresponding indices describing which vertices make triangles
        public static List<int>[] squareIndices =
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

        // Uses values to fill arrays of vertices and indices
        // First time using "ref", did not want to return a tuple or clone values. Seems to work though
        public static void GetData(ref float[,] values, ref VertexPositionTexture[] vertices, ref int[] indices)
        {
            // Temporary lists to fill one element at a time
            List<VertexPositionTexture> verts = new List<VertexPositionTexture>();
            List<int> inds = new List<int>();

            int index = 0;

            for (int x = 0; x < values.GetLength(0) - 1; x++)
            {
                for (int y = 0; y < values.GetLength(1) - 1; y++)
                {
                    // Calculate which triangle configuration to add as a 4 bit number
                    // based on which cells are active
                    int configuration = 0;
                    if (values[x,     y    ] > 0.0f) configuration += 8;
                    if (values[x + 1, y    ] > 0.0f) configuration += 4;
                    if (values[x + 1, y + 1] > 0.0f) configuration += 2;
                    if (values[x,     y + 1] > 0.0f) configuration += 1;

                    // Configuration 0 represents no triangles, so don't do anything
                    if (configuration == 0) continue;

                    // Add elements of configuration to vertex list:
                    foreach (VertexPositionTexture squareVertex in squareVertices[configuration])
                    {
                        // Copy vertex
                        VertexPositionTexture vert = squareVertex;

                        // Interpolate to bring vertices closer to cells with greater values
                        if (interpolateVertices)
                        {
                            // 0 is middle, so it should be brought to a more fitting position
                            if (vert.Position.X == 0.0f)
                            {
                                float t = 1.0f + (vert.Position.Y > 0.0f ? // Choose which two cells to compare
                                    values[x    , y    ] / (values[x + 1, y    ] - values[x    , y    ]) :
                                    values[x    , y + 1] / (values[x + 1, y + 1] - values[x    , y + 1]));

                                // Convert 0.0f to 1.0f value to -1.0f to 1.0f value
                                // and then to screen position
                                vert.Position.X = (1.0f - (t * 2.0f)) * cwos;
                            }
                            else if (vert.Position.Y == 0.0f)
                            {
                                float t = -(vert.Position.X < 0.0f ? // Choose which two cells to compare
                                    values[x    , y    ] / (values[x    , y + 1] - values[x    , y    ]) :
                                    values[x + 1, y    ] / (values[x + 1, y + 1] - values[x + 1, y    ]));

                                // Convert 0.0f to 1.0f value to -1.0f to 1.0f value
                                // and then to screen position
                                vert.Position.Y = (1.0f - (t * 2.0f)) * chos;
                            }
                        }

                        // Move vertex to cell position
                        vert.Position += new Vector3
                        (
                            (x + 1.0f - (values.GetLength(0) / 2.0f)) * cwos * 2.0f,
                           -(y + 1.0f - (values.GetLength(1) / 2.0f)) * chos * 2.0f,
                            0.0f
                        );

                        verts.Add(vert);
                    }
                    foreach (int squareIndex in squareIndices[configuration])
                    {
                        // New indices begin counting at last vertex
                        inds.Add(squareIndex + index);
                    }

                    index += squareVertices[configuration].Count;
                }
            }

            vertices = verts.ToArray();
            indices = inds.ToArray();
        }
    }
}
