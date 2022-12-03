using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Antoids
{
    public struct Pheromone
    {
        public Vector2 position;

        public const float maxStength = 15.0f;
        public float strength = maxStength;

        public Pheromone(Vector2 _position)
        {
            position = _position;
        }
    }

    public class Ant
    {
        public static Texture2D texture;

        private static Random random = new Random();

        private const float maxSpeed = 1.8f;
        private const float steerStrength = 2.3f;
        private const float wanderStrength = 0.12f;

        private const float foodViewRange = 1.3f;
        private const float nestViewRange = 2.0f;
        private const float pickUpRange = 0.07f;

        public Vector2 position;
        private Vector2 velocity = Vector2.One;
        private Vector2 desiredDirection;
        public float rotation;

        private Food targetFood;
        private bool hasFood;

        private Vector2 lastPheromonePosition;
        private const float pheromoneSpacing = 0.25f;

        private const float sensorRadius = 0.4f;
        private const float sensorDistance = 0.5f;
        private const float sensorAngle = (float)Math.PI / 4.0f;

        public Ant(Vector2 _position, Vector2 _velocity)
        {
            position = _position;
            velocity = _velocity;
        }

        public void Update(float deltaTime)
        {
            if (!hasFood)
                FindFood();
            else
                FindNest();

            Move(deltaTime);

            MakePheromones();

            if (targetFood == null) 
                SmellPheromones();
        }

        private void Move(float deltaTime)
        {
            desiredDirection = desiredDirection + MathHelper.RandomInsideUnitCircle() * wanderStrength;
            desiredDirection.Normalize();

            AvoidTerrain();

            Vector2 desiredVelocity = desiredDirection * maxSpeed;
            Vector2 desiredSteeringForce = (desiredVelocity - velocity) * steerStrength;
            Vector2 acceleration = MathHelper.ClampNorm(desiredSteeringForce, steerStrength);

            velocity = MathHelper.ClampNorm(velocity + acceleration * deltaTime, maxSpeed);
            position += velocity * deltaTime;

            rotation = (float)Math.Atan2(velocity.Y, velocity.X);
        }

        private void FindFood()
        {
            if (targetFood == null)
            {
                List<Food> foodInView = new List<Food>();
                foreach (Food food in World.foods)
                {
                    if (Vector2.Distance(position, food.position) < foodViewRange &&
                        !food.willBeRemoved)
                    {
                        targetFood = food;
                        break;
                    }
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
                    targetFood = null;
                    desiredDirection = -desiredDirection;
                }
            }
            else
            {
                targetFood = null;
            }
        }

        private void FindNest()
        {
            if (Vector2.Distance(position, World.nestPosition) < nestViewRange)
            {
                desiredDirection = World.nestPosition - position;
                desiredDirection.Normalize();

                if (Vector2.Distance(position, World.nestPosition) < 0.5f)
                {
                    hasFood = false;
                    desiredDirection = -desiredDirection;
                    velocity = Vector2.Zero;
                }
            }
        }

        private void MakePheromones()
        {
            if (Vector2.Distance(position, lastPheromonePosition) > pheromoneSpacing)
            {
                if (hasFood)
                {
                    World.foodPheromones.Add(new Pheromone(position));
                }
                else
                {
                    World.homePheromones.Add(new Pheromone(position));
                }

                lastPheromonePosition = position;
            }
        }

        private void SmellPheromones()
        {
            List<Pheromone> pheromones = hasFood ? World.homePheromones : World.foodPheromones;

            float[] sensorValues = new float[3];

            for (int i = -1; i <= 1; i++)
            {
                float angle = rotation + i * sensorAngle;
                float x = (float)Math.Cos(angle);
                float y = (float)Math.Sin(angle);
                Vector2 offset = new Vector2(x, y) * sensorDistance;

                Vector2 sensorPosition = position + offset;

                foreach (Pheromone pheromone in pheromones)
                {
                    if (Vector2.Distance(sensorPosition, pheromone.position) < sensorRadius)
                    {
                        sensorValues[i + 1] += pheromone.strength / Pheromone.maxStength;
                    }
                }
            }

            if (sensorValues[1] > Math.Max(sensorValues[0], sensorValues[2]))
            {
                desiredDirection.X += (float)Math.Cos(rotation) * sensorValues[1];
                desiredDirection.Y += (float)Math.Sin(rotation) * sensorValues[1];
            }
            else if (sensorValues[0] > sensorValues[2])
            {
                float steerAngle = rotation - (float)Math.PI / 2.0f;
                desiredDirection.X += (float)Math.Cos(steerAngle) * sensorValues[0];
                desiredDirection.Y += (float)Math.Sin(steerAngle) * sensorValues[0];
            }
            else if (sensorValues[2] > sensorValues[0])
            {
                float steerAngle = rotation + (float)Math.PI / 2.0f;
                desiredDirection.X += (float)Math.Cos(steerAngle) * sensorValues[2];
                desiredDirection.Y += (float)Math.Sin(steerAngle) * sensorValues[2];
            }
        }

        private void AvoidTerrain()
        {
            for (int i = -1; i <= 1; i += 2)
            {
                float angle = rotation + i * sensorAngle * 0.5f;
                float x = (float)Math.Cos(angle);
                float y = (float)Math.Sin(angle);
                Vector2 offset = new Vector2(x, y) * 0.3f;

                Vector2 sensorPosition = position + offset;

                if (World.terrain.GetValueAtPoint(sensorPosition) > 0.0f)
                {
                    float steerAngle = rotation - i * (float)Math.PI / 4.0f;
                    desiredDirection.X = (float)Math.Cos(steerAngle);
                    desiredDirection.Y = (float)Math.Sin(steerAngle);

                    velocity *= 0.9f;
                }
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
