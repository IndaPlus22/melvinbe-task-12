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
        public Color color;

        // Flip when food is destroyed
        public bool willBeRemoved;
        public Food(Vector2 position, Color color)
        {
            this.position = position;
            this.color = color;
        }
    }

    public struct Pheromone
    {
        public Vector2 position;

        // How long a pheromone lasts for in seconds
        public const float maxStength = 20.0f;
        public float strength = maxStength;

        public Pheromone(Vector2 position)
        {
            this.position = position;
        }
    }

    public static class World
    {
        // World dimension in "world units"
        public const float worldWidth = 40.0f;
        public const float worldHeight = 24.0f;

        public static List<Ant> ants = new List<Ant>();

        public static List<Food> foods = new List<Food>();

        // The pheromone list is split into 20 * 12 smaller lists that
        // hold up to 70 pheromones each and are positioned in a grid
        public const int partitionsX = 20;
        public const int partitionsY = 12;
        public const int maxPheromonesPerPartition = 70;
        public static List<Pheromone>[,] foodPheromones = new List<Pheromone>[partitionsX, partitionsY];
        public static List<Pheromone>[,] homePheromones = new List<Pheromone>[partitionsX, partitionsY];

        // No more than 250 ants will spawn from the nest
        private const int antCount = 250;

        // Maybe this could be a class...
        public const float nestRadius = 1.2f;
        public static Vector2 nestPosition = new Vector2(worldWidth / 2.0f, worldHeight / 2.0f);

        // Reset simulation world:
        public static void Clean()
        {
            // Create or clear every list:
            ants = new List<Ant>();

            foods = new List<Food>();

            for (int x = 0; x < partitionsX; x++)
            {
                for (int y = 0; y < partitionsY; y++)
                {
                    foodPheromones[x, y] = new List<Pheromone>();
                    homePheromones[x, y] = new List<Pheromone>();
                }
            }

            // Remove dirt around nest
            Brush.CleanNest();
        }

        public static void Update(float deltaTime)
        {
            // Create more ants as long as the limit has not been reached
            while (ants.Count < antCount)
            {
                // Create new ant at nest with random position offset and velocity:
                Vector2 antPosition = nestPosition + MathHelper.RandomInsideUnitCircle();
                Vector2 antVelocity = MathHelper.RandomInsideUnitCircle();
                ants.Add(new Ant(antPosition, antVelocity));
            }

            foreach (Ant ant in ants)
            {
                ant.Update(deltaTime);
            }

            // Reduce strength of every pheromone and remove pheromones with 0 strength:
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

                        // Filter pheromones with strength less than 0
                        foodPheromones[x, y] = foodPartion.Where(pheromone => pheromone.strength > 0.0f).ToList();
                    }

                    List<Pheromone> homePartion = homePheromones[x, y];
                    for (int i = 0; i < homePartion.Count; i++)
                    {
                        Pheromone pheromone = homePartion[i];
                        pheromone.strength -= deltaTime;
                        homePartion[i] = pheromone;

                        // Filter pheromones with strength less than 0
                        homePheromones[x, y] = homePartion.Where(pheromone => pheromone.strength > 0.0f).ToList();
                    }
                }
            }

            // Filter foods that have been set to be removed
            foods = foods.Where(food => !food.willBeRemoved).ToList();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            // Draw pheromones if set to show:
            if (Simulation.showPheromones)
            {
                for (int x = 0; x < partitionsX; x++)
                {
                    for (int y = 0; y < partitionsY; y++)
                    {
                        foreach (Pheromone pheromone in foodPheromones[x, y])
                        {
                            // Reduce opacity of weaker pheromones:
                            float alpha = pheromone.strength / Pheromone.maxStength;
                            Color color = Simulation.foodPheromoneColor * alpha;
                            Simulation.DrawCircle(spriteBatch, pheromone.position, color, 0.1f);
                        }
                        foreach (Pheromone pheromone in homePheromones[x, y])
                        {
                            // Reduce opacity of weaker pheromones:
                            float alpha = pheromone.strength / Pheromone.maxStength;
                            Color color = Simulation.homePheromoneColor * alpha;
                            Simulation.DrawCircle(spriteBatch, pheromone.position, color, 0.1f);
                        }
                    }
                }
            }

            // Draw ants
            foreach (Ant ant in ants)
            {
                ant.Draw(spriteBatch);                
            }

            // Draw food
            foreach (Food food in foods)
            {
                Simulation.DrawCircle(spriteBatch, food.position, food.color, 0.15f);
            }

            // Draw nest
            Simulation.DrawCircle(spriteBatch, nestPosition, Simulation.nestColor1, nestRadius * 2.0f);
            Simulation.DrawCircle(spriteBatch, nestPosition, Simulation.nestColor2, nestRadius * 0.9f * 2.0f);
        }
    }
}
