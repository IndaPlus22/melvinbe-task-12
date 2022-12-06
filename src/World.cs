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
        public Color color;
        public Food(Vector2 _position, Color color)
        {
            position = _position;
            this.color = color;
        }
    }

    public static class World
    {
        public const float worldWidth = 40.0f;
        public const float worldHeight = 24.0f;

        public static List<Ant> ants = new List<Ant>();

        public static List<Food> foods = new List<Food>();

        public const int partitionsX = 20;
        public const int partitionsY = 12;
        public const int maxPheromonesPerPartition = 70;
        public static List<Pheromone>[,] foodPheromones = new List<Pheromone>[partitionsX, partitionsY];
        public static List<Pheromone>[,] homePheromones = new List<Pheromone>[partitionsX, partitionsY];

        private const int antCount = 250;

        public static Vector2 nestPosition;
        public const float nestRadius = 1.0f;

        public static void Init()
        {
            Brush.MoveNest(new Vector2(worldWidth / 2.0f, worldHeight / 2.0f));

            Clean();
        }

        public static void Clean()
        {
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
        }

        public static void Update(float deltaTime)
        {
            while (ants.Count < antCount)
            {
                ants.Add(new Ant(nestPosition + MathHelper.RandomInsideUnitCircle() * 0.5f, MathHelper.RandomInsideUnitCircle()));
            }

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
            if (Simulation.showPheromones)
            {
                for (int x = 0; x < partitionsX; x++)
                {
                    for (int y = 0; y < partitionsY; y++)
                    {
                        foreach (Pheromone pheromone in foodPheromones[x, y])
                        {
                            float alpha = pheromone.strength / Pheromone.maxStength;
                            Color color = Simulation.foodPheromoneColor * alpha;
                            Simulation.DrawCircle(spriteBatch, pheromone.position, color, 0.1f);
                        }
                        foreach (Pheromone pheromone in homePheromones[x, y])
                        {
                            float alpha = pheromone.strength / Pheromone.maxStength;
                            Color color = Simulation.homePheromoneColor * alpha;
                            Simulation.DrawCircle(spriteBatch, pheromone.position, color, 0.1f);
                        }
                    }
                }
            }

            foreach (Ant ant in ants)
            {
                ant.Draw(spriteBatch);                
            }

            foreach (Food food in foods)
            {
                Simulation.DrawCircle(spriteBatch, food.position, food.color, 0.15f);
            }

            Simulation.DrawCircle(spriteBatch, nestPosition, Simulation.nestColor, nestRadius * 2.0f);
        }
    }
}
