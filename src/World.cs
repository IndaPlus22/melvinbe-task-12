using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;

namespace Antoids
{
    public static class World
    {
        public const int worldWidth = 800;
        public const int worldHeight = 480;

        public static Texture2D circleTexture;

        private static List<Ant> ants = new List<Ant>();
        public static List<Vector2> food = new List<Vector2>();
        public static List<Vector2> foodPheromones = new List<Vector2>();
        public static List<Vector2> homePheromones = new List<Vector2>();

        private const int antCount = 20;
        public static Vector2 nestPosition = new Vector2(worldWidth / 2, worldHeight / 2);

        public static void Init()
        {
            for (int i = 0; i < antCount; i++)
            {
                ants.Add(new Ant(nestPosition, MathHelper.RandomInsideUnitCircle()));
            }

            Random random = new Random();
            for (int i = 0; i < 100; i++)
            {
                food.Add(new Vector2((float)random.NextDouble() * worldWidth, (float)random.NextDouble() * worldHeight));
            }
        }

        public static void Update(float deltaTime)
        {
            foreach (Ant ant in ants)
            {
                ant.Update(deltaTime);
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (Ant ant in ants)
            {
                ant.Draw(spriteBatch);
            }

            foreach (Vector2 foodPosition in food)
            {
                spriteBatch.Draw
                (
                    circleTexture,
                    foodPosition * Simulation.windowScale,
                    circleTexture.Bounds,
                    Color.GreenYellow,
                    0.0f,
                    new Vector2(circleTexture.Width / 2, circleTexture.Height / 2),
                    0.1f * Simulation.windowScale,
                    SpriteEffects.None,
                    0.0f
                );
            }
        }
    }
}
