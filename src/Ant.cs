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

        public const float maxStength = 24.0f;
        public float strength = maxStength;

        public Pheromone(Vector2 _position)
        {
            position = _position;
        }
    }

    public class Ant
    {
        public static Texture2D texture;

        private const float maxSpeed = 2.5f;
        private const float steerStrength = 2.5f;
        private const float wanderStrength = 0.25f;

        private const float foodViewRange = 1.5f;
        private const float nestViewRange = 4.0f;
        private const float pickUpRange = 0.15f;

        private const float sensorRadius = 0.1f;
        private const float sensorDistance = 1.8f;
        private const float sensorAngle = (float)Math.PI / 9.0f;

        public Vector2 position;
        private Vector2 velocity;
        private Vector2 desiredDirection;
        public float rotation;

        private Food targetFood;
        private bool hasFood;

        private const float pheromoneSpacing = 0.6f;
        private Vector2 lastPheromonePosition;

        private Vector2[] sensPoses = new Vector2[3];

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

            //*
            foreach (Ant ant in World.ants)
            {
                if (Vector2.Distance(position, ant.position) <= 0.15f && position != ant.position)
                {
                    Vector2 direction = position - ant.position;
                    direction.Normalize();
                    velocity += direction * 5.0f * deltaTime;
                }
            }
            //*/
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
            float distance = float.PositiveInfinity;
            foreach (Food food in World.foods)
            {
                float dist = Vector2.Distance(position, food.position);
                if (dist < foodViewRange &&
                    dist < distance &&
                    !food.willBeRemoved)
                {
                    distance = dist;
                    targetFood = food;
                }
            }

            if (targetFood != null)
            {
                if (!targetFood.willBeRemoved)
                {
                    desiredDirection = targetFood.position - position;
                    desiredDirection.Normalize();

                    if (Vector2.Distance(position, targetFood.position) < pickUpRange)
                    {
                        targetFood.willBeRemoved = true;
                        hasFood = true;
                        targetFood = null;
                        desiredDirection = -desiredDirection;
                        velocity = -velocity * 0.5f;
                    }
                }
                else
                {
                    targetFood = null;
                }
            }
        }

        private void FindNest()
        {
            if (Vector2.Distance(position, World.nestPosition) < nestViewRange)
            {
                desiredDirection = World.nestPosition - position;
                desiredDirection.Normalize();

                if (Vector2.Distance(position, World.nestPosition) < 1.0f)
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
                int x = (int)(position.X / 2.0f);
                int y = (int)(position.Y / 2.0f);
                if (hasFood)
                {
                    if (World.foodPheromones[x, y].Count >= World.maxPheromonesPerPartition)
                        World.foodPheromones[x, y].RemoveAt(0);

                    World.foodPheromones[x, y].Add(new Pheromone(position));
                }
                else
                {
                    if (World.homePheromones[x, y].Count >= World.maxPheromonesPerPartition)
                        World.homePheromones[x, y].RemoveAt(0);

                    World.homePheromones[x, y].Add(new Pheromone(position));
                }

                lastPheromonePosition = position;
            }
        }

        private void SmellPheromones()
        {
            List<Pheromone>[,] pheromones = hasFood ? World.homePheromones : World.foodPheromones;

            float[] sensorValues = new float[3];

            for (int i = -1; i <= 1; i++)
            {
                float angle = rotation + i * sensorAngle;
                float xOffset = (float)Math.Cos(angle);
                float yOffset = (float)Math.Sin(angle);
                Vector2 offset = new Vector2(xOffset, yOffset) * sensorDistance;

                Vector2 sensorPosition = position + offset;

                sensPoses[i + 1] = sensorPosition;

                int inPaX = (int)((position.X + 1.0f) / 2.0f);
                int inPaY = (int)((position.Y + 1.0f) / 2.0f);

                for (int x = inPaX - 1; x <= inPaX + 1; x++)
                {
                    if (x < 0 || x >= World.partitionsX)
                        continue;

                    for (int y = inPaY - 1; y < inPaY + 1; y++)
                    {
                        if (y < 0 || y >= World.partitionsY) 
                            continue;

                        foreach (Pheromone pheromone in pheromones[x, y])
                        {
                            if (Vector2.Distance(sensorPosition, pheromone.position) < sensorRadius)
                            {
                                sensorValues[i + 1] += pheromone.strength;
                            }
                        }
                    }
                }
            }

            if (sensorValues[1] > Math.Max(sensorValues[0], sensorValues[2]))
            {
                desiredDirection.X += (float)Math.Cos(rotation);
                desiredDirection.Y += (float)Math.Sin(rotation);
            }
            else if (sensorValues[0] > sensorValues[2])
            {
                float steerAngle = rotation - (float)Math.PI / 2.0f;
                desiredDirection.X += (float)Math.Cos(steerAngle);
                desiredDirection.Y += (float)Math.Sin(steerAngle);
            }
            else if (sensorValues[2] > sensorValues[0])
            {
                float steerAngle = rotation + (float)Math.PI / 2.0f;
                desiredDirection.X += (float)Math.Cos(steerAngle);
                desiredDirection.Y += (float)Math.Sin(steerAngle);
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

                if (Terrain.GetValueAtPoint(sensorPosition.X, sensorPosition.Y) > 0.0f)
                {
                    float steerAngle = rotation - i * (float)Math.PI / 2.0f;
                    desiredDirection.X = (float)Math.Cos(steerAngle);
                    desiredDirection.Y = (float)Math.Sin(steerAngle);

                    velocity += desiredDirection * 0.5f;
                    velocity *= 0.90f;
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

            for (int i = 0; i < 3; i++)
            {
                //World.DrawCircle(spriteBatch, sensPoses[i], Color.Black * 0.4f, sensorRadius * 2.0f);
            }
        }
    }
}
