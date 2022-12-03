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

    public struct Pheromone
    {
        public Vector2 position;
        public float strength = 0.0f;
        public Pheromone(Vector2 _position)
        {
            position = _position;
        }
    }

    public static class World
    {
        public const float worldWidth = 800.0f / 30.0f;
        public const float worldHeight = 480.0f / 30.0f;

        public static Texture2D circleTexture;

        private static List<Ant> ants = new List<Ant>();

        public static List<Food> foods = new List<Food>();

        public static List<Pheromone> foodPheromones = new List<Pheromone>();
        public static List<Pheromone> homePheromones = new List<Pheromone>();

        private const int antCount = 20;
        public static Vector2 nestPosition = new Vector2(worldWidth / 2, worldHeight / 2);

        public static void Init()
        {
            for (int i = 0; i < antCount; i++)
            {
                ants.Add(new Ant(nestPosition, MathHelper.RandomInsideUnitCircle()));
            }

            Random random = new Random();
            for (int i = 0; i < 20; i++)
            {
                Vector2 foodPosition = new Vector2((float)random.NextDouble() * worldWidth, (float)random.NextDouble() * worldHeight);
                foods.Add(new Food(foodPosition));
            }
        }

        public static void Update(float deltaTime)
        {
            foreach (Ant ant in ants)
            {
                ant.Update(deltaTime);
            }

            foods = foods.Where(food => !food.willBeRemoved).ToList();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (Ant ant in ants)
            {
                ant.Draw(spriteBatch);                
            }

            foreach (Food food in foods)
            {
                DrawCircle(spriteBatch, food.position, Color.YellowGreen, 0.15f);
            }

            DrawCircle(spriteBatch, nestPosition, Color.Brown, 1.5f);
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
