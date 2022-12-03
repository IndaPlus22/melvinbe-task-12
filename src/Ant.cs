using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Antoids
{
    public class Ant
    {
        public static Texture2D texture;

        private static Random random = new Random();

        private const float maxSpeed = 1.25f;
        private const float steerStrength = 3.0f;
        private const float wanderStrength = 0.2f;

        private const float viewRange = 1.5f;
        //private const float viewAngle = (float)Math.PI / 2.0f;
        private const float pickUpRange = 0.05f;

        public Vector2 position;
        private Vector2 velocity = Vector2.One;
        private Vector2 desiredDirection;
        public float rotation;

        private Food targetFood;
        public bool hasFood;

        public Ant(Vector2 _position, Vector2 _velocity)
        {
            position = _position;
            velocity = _velocity;
        }

        public void Update(float deltaTime)
        {
            if (!hasFood) FindFood(deltaTime);

            desiredDirection = desiredDirection + MathHelper.RandomInsideUnitCircle() * wanderStrength;
            desiredDirection.Normalize();

            Vector2 desiredVelocity = desiredDirection * maxSpeed;
            Vector2 desiredSteeringForce = (desiredVelocity - velocity) * steerStrength;
            Vector2 acceleration = MathHelper.ClampNorm(desiredSteeringForce, steerStrength);

            velocity = MathHelper.ClampNorm(velocity + acceleration * deltaTime, maxSpeed);
            position += velocity * deltaTime;

            rotation = (float)Math.Atan2(velocity.Y, velocity.X);
        }

        private void FindFood(float deltaTime)
        {
            if (targetFood == null)
            {
                List<Food> foodInView = new List<Food>();
                foreach (Food food in World.foods)
                {
                    if (Vector2.Distance(position, food.position) < viewRange &&
                        !food.willBeRemoved)
                    {
                        // Should see only see food in front:

                        //Vector2 directionToFood = food - position;
                        //directionToFood.Normalize();
                        //...

                        foodInView.Add(food);
                    }
                }
                if (foodInView.Count > 0)
                {
                    targetFood = foodInView[random.Next(foodInView.Count)];
                }
            }
            else if (!targetFood.willBeRemoved)
            {
                desiredDirection = targetFood.position - position;
                desiredDirection.Normalize();

                if (Vector2.Distance(position, targetFood.position) < pickUpRange)
                {
                    targetFood.willBeRemoved = true;
                    hasFood = true;
                }
            }
            else
            {
                targetFood = null;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw
            (
                texture,
                position * Simulation.windowScale,
                texture.Bounds,
                Color.White,
                rotation,
                new Vector2(texture.Width / 2, texture.Height / 2),
                0.4f * Simulation.windowScale / texture.Width,
                SpriteEffects.None,
                0.0f
            );

            if (hasFood)
            {
                World.DrawCircle(spriteBatch, position, Color.YellowGreen, 0.15f);
            }
        }
    }
}
