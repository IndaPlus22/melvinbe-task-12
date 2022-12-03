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

        public static Terrain terrain;

        private static List<Ant> ants = new List<Ant>();

        public static List<Food> foods = new List<Food>();

        public static List<Pheromone> foodPheromones = new List<Pheromone>();
        public static List<Pheromone> homePheromones = new List<Pheromone>();

        private const int antCount = 200;
        public static Vector2 nestPosition = new Vector2(worldWidth / 2, worldHeight / 2);

        public static void Init()
        {
            terrain = new Terrain();

            for (int i = 0; i < antCount; i++)
            {
                ants.Add(new Ant(nestPosition, MathHelper.RandomInsideUnitCircle()));
            }

            Random random = new Random();
            for (int i = 0; i < 300; i++)
            {
                Vector2 foodPosition = new Vector2((float)random.NextDouble() * 2.5f + 7.0f, (float)random.NextDouble() * 2.5f + 5.0f);
                foods.Add(new Food(foodPosition));
            }
        }

        public static void Update(float deltaTime)
        {
            foreach (Ant ant in ants)
            {
                ant.Update(deltaTime);
            }

            for (int i = 0; i < foodPheromones.Count; i++)
            {
                Pheromone pheromone = foodPheromones[i];
                pheromone.strength -= deltaTime;
                foodPheromones[i] = pheromone;
            }
            for (int i = 0; i < homePheromones.Count; i++)
            {
                Pheromone pheromone = homePheromones[i];
                pheromone.strength -= deltaTime;
                homePheromones[i] = pheromone;
            }

            foods = foods.Where(food => !food.willBeRemoved).ToList();

            foodPheromones = foodPheromones.Where(pheromone => pheromone.strength > 0.0f).ToList();
            homePheromones = homePheromones.Where(pheromone => pheromone.strength > 0.0f).ToList();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (Pheromone pheromone in foodPheromones)
            {
                float alpha = pheromone.strength / Pheromone.maxStength;
                Color color = Color.IndianRed * alpha;
                DrawCircle(spriteBatch, pheromone.position, color, 0.1f);
            }
            foreach (Pheromone pheromone in homePheromones)
            {
                float alpha = pheromone.strength / Pheromone.maxStength;
                Color color = Color.CornflowerBlue * alpha;
                DrawCircle(spriteBatch, pheromone.position, color, 0.1f);
            }

            terrain.Draw(spriteBatch);

            foreach (Ant ant in ants)
            {
                ant.Draw(spriteBatch);                
            }

            foreach (Food food in foods)
            {
                DrawCircle(spriteBatch, food.position, Color.YellowGreen, 0.15f);
            }

            DrawCircle(spriteBatch, nestPosition, Color.Brown, 1.25f);
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
