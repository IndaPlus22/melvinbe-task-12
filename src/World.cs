using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Antoids
{
    public class Food
    {
        public Vector2 position;
        public bool willBeRemoved;
        public Food(Vector2 _position)
        {
            position = _position;
        }
    }

    public static class World
    {
        public static Texture2D circleTexture;

        public const float worldWidth = 40.0f;
        public const float worldHeight = 24.0f;

        public static List<Ant> ants = new List<Ant>();

        public static List<Food> foods = new List<Food>();

        public const int partitionsX = 20;
        public const int partitionsY = 12;
        public const int maxPheromonesPerPartition = 60;
        public static List<Pheromone>[,] foodPheromones = new List<Pheromone>[partitionsX, partitionsY];
        public static List<Pheromone>[,] homePheromones = new List<Pheromone>[partitionsX, partitionsY];

        //public static List<Pheromone> foodPheromones = new List<Pheromone>();
        //public static List<Pheromone> homePheromones = new List<Pheromone>();

        private const int antCount = 200;
        public static Vector2 nestPosition = new Vector2(worldWidth / 2, worldHeight / 2);

        public static void Init()
        {
            for (int i = 0; i < antCount; i++)
            {
                ants.Add(new Ant(nestPosition + MathHelper.RandomInsideUnitCircle() * 0.5f, MathHelper.RandomInsideUnitCircle()));
            }

            Random random = new Random();
            for (int i = 0; i < 500; i++)
            {
                Vector2 foodPosition = new Vector2((float)random.NextDouble() * 2.5f + 7.0f, (float)random.NextDouble() * 2.5f + 5.0f);
                foods.Add(new Food(foodPosition));

                foodPosition = new Vector2(worldWidth - (float)random.NextDouble() * 2.5f - 7.0f, worldHeight - (float)random.NextDouble() * 2.5f - 5.0f);
                foods.Add(new Food(foodPosition));
            }

            for (int x = 0; x < partitionsX; x++)
            {
                for (int y = 0; y < partitionsY; y++)
                {
                    foodPheromones[x, y] = new List<Pheromone>();
                    homePheromones[x, y] = new List<Pheromone>();
                }
            }
        }

        public static void Update(float deltaTime)
        {
            foreach (Ant ant in ants)
            {
                ant.Update(deltaTime);
            }

            for (int x = 0; x < partitionsX; x++)
            {
                for (int y = 0; y < partitionsY; y++)
                {
                    List<Pheromone> foodPartion = foodPheromones[x, y];
                    for (int i = 0; i < foodPartion.Count; i++)
                    {
                        Pheromone pheromone = foodPartion[i];
                        pheromone.strength -= deltaTime;
                        foodPartion[i] = pheromone;

                        foodPheromones[x, y] = foodPartion.Where(pheromone => pheromone.strength > 0.0f).ToList();
                    }

                    List<Pheromone> homePartion = homePheromones[x, y];
                    for (int i = 0; i < homePartion.Count; i++)
                    {
                        Pheromone pheromone = homePartion[i];
                        pheromone.strength -= deltaTime;
                        homePartion[i] = pheromone;

                        homePheromones[x, y] = homePartion.Where(pheromone => pheromone.strength > 0.0f).ToList();
                    }
                }
            }

            foods = foods.Where(food => !food.willBeRemoved).ToList();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < partitionsX; x++)
            {
                for (int y = 0; y < partitionsY; y++)
                {
                    foreach (Pheromone pheromone in foodPheromones[x, y])
                    {
                        float alpha = pheromone.strength / Pheromone.maxStength;
                        Color color = Color.IndianRed * alpha;
                        DrawCircle(spriteBatch, pheromone.position, color, 0.1f);
                    }
                    foreach (Pheromone pheromone in homePheromones[x, y])
                    {
                        float alpha = pheromone.strength / Pheromone.maxStength;
                        Color color = Color.CornflowerBlue * alpha;
                        DrawCircle(spriteBatch, pheromone.position, color, 0.1f);
                    }
                }
            }

            foreach (Ant ant in ants)
            {
                ant.Draw(spriteBatch);                
            }

            foreach (Food food in foods)
            {
                DrawCircle(spriteBatch, food.position, Color.YellowGreen, 0.15f);
            }

            DrawCircle(spriteBatch, nestPosition, Color.Brown, 2.0f);
        }

        public static void DrawCircle(SpriteBatch spriteBatch, Vector2 position, Color color, float scale)
        {
            spriteBatch.Draw
            (
                circleTexture,
                position * Simulation.windowScale,
                circleTexture.Bounds,
                color,
                0.0f,
                new Vector2(circleTexture.Width / 2, circleTexture.Height / 2),
                scale * Simulation.windowScale / World.circleTexture.Width,
                SpriteEffects.None,
                0.0f
            );
        }
    }
}
