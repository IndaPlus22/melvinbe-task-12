using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Antoids
{
    // Autonomous agent that searches for food and brings it back to nest
    // by following pheromone trails left by other agents
    public class Ant
    {
        public static Texture2D antTexture;

        // Movement settings:
        private const float maxSpeed = 2.5f;
        private const float steerStrength = 2.5f;
        private const float wanderStrength = 0.25f;

        // Range settings:
        private const float foodViewRange = 1.5f;
        private const float nestViewRange = 4.0f;
        private const float pickUpRange = 0.15f;

        // Pheromone sensor settings:
        private const float sensorRadius = 0.2f;
        private const float sensorDistance = 1.8f;
        private const float sensorAngle = (float)Math.PI / 10.0f;

        // Movement variables:
        public Vector2 position;
        private Vector2 velocity;
        private Vector2 desiredDirection;
        public float rotation;

        // Food behaviour varables:
        private Food targetFood;
        private bool hasFood;
        // Save food color for carrying
        private Color foodColor;

        // How often to leave pheromones
        private const float pheromoneSpacing = 0.6f;
        private Vector2 lastPheromonePosition;

        // Save sensor positions for debugging purposes
        private Vector2[] sensPoses = new Vector2[3];

        public Ant(Vector2 position, Vector2 velocity)
        {
            this.position = position;
            this.velocity = velocity;
        }

        public void Update(float deltaTime)
        {
            // Decide what to look for
            if (!hasFood)
                FindFood();
            else
                FindNest();
            
            Move(deltaTime);

            MakePheromones();

            // If not running directly to food, follow pheromone trails
            if (targetFood == null) 
                SmellPheromones();
        }

        // Find, run towards and pick up food:
        private void FindFood()
        {
            targetFood = null;

            // Set closest food as target if close enough:
            float distance = float.PositiveInfinity;
            foreach (Food food in World.foods)
            {
                float dist = Vector2.Distance(position, food.position);
                if (dist < foodViewRange && dist < distance && 
                    !food.willBeRemoved) // Prevents unlikely food duplication
                {
                    distance = dist;
                    targetFood = food;
                    foodColor = targetFood.color;
                }
            }

            // If has target attempt to pick it up
            if (targetFood != null)
            {
                if (!targetFood.willBeRemoved)
                {
                    // Move towards target food
                    desiredDirection = targetFood.position - position;
                    desiredDirection.Normalize();

                    // Pick up if close enough:
                    if (Vector2.Distance(position, targetFood.position) < pickUpRange)
                    {
                        targetFood.willBeRemoved = true;
                        hasFood = true;
                        targetFood = null;

                        // Force turn around and slow down to better follow own trail
                        desiredDirection = -desiredDirection;
                        velocity = -velocity * 0.5f;
                    }
                }
                else
                {
                    // If target was grabbed first by another ant, find new target
                    targetFood = null;
                }
            }
        }

        // Run towards nest if close enough and deposit food:
        private void FindNest()
        {
            // Move towards if in view range:
            if (Vector2.Distance(position, World.nestPosition) < nestViewRange)
            {
                desiredDirection = World.nestPosition - position;
                desiredDirection.Normalize();

                // Deposit food if close enough:
                if (Vector2.Distance(position, World.nestPosition) < 1.0f)
                {
                    hasFood = false;

                    // Force turn around and slow down to better follow own trail
                    desiredDirection = -desiredDirection;
                    velocity = -velocity * 0.5f;
                }
            }
        }

        // Handle "physics" and move position
        private void Move(float deltaTime)
        {
            // Wander in random direction
            desiredDirection = desiredDirection + MathHelper.RandomInsideUnitCircle() * wanderStrength;
            desiredDirection.Normalize();

            AvoidObstacles(deltaTime);

            // Handle "physics":
            Vector2 desiredVelocity = desiredDirection * maxSpeed;
            Vector2 desiredSteeringForce = (desiredVelocity - velocity) * steerStrength;
            Vector2 acceleration = MathHelper.ClampNorm(desiredSteeringForce, steerStrength);

            // Limit velocity to max speed
            velocity = MathHelper.ClampNorm(velocity + acceleration * deltaTime, maxSpeed);
            position += velocity * deltaTime;

            // Set rotation to movement direction
            rotation = (float)Math.Atan2(velocity.Y, velocity.X);
        }

        // Make pheromone trail:
        private void MakePheromones()
        {
            // Create next pheromone after moving certain distance:
            if (Vector2.Distance(position, lastPheromonePosition) > pheromoneSpacing)
            {
                // Select partition based on position
                int x = (int)(position.X * World.partitionsX / World.worldWidth);
                int y = (int)(position.Y * World.partitionsY / World.worldHeight);

                // Select pheromone type
                if (hasFood)
                {
                    // Remove weakest pheromone if partition is full
                    if (World.foodPheromones[x, y].Count >= World.maxPheromonesPerPartition)
                        World.foodPheromones[x, y].RemoveAt(0);

                    World.foodPheromones[x, y].Add(new Pheromone(position));
                }
                else
                {
                    // Remove weakest pheromone if partition is full
                    if (World.homePheromones[x, y].Count >= World.maxPheromonesPerPartition)
                        World.homePheromones[x, y].RemoveAt(0);

                    World.homePheromones[x, y].Add(new Pheromone(position));
                }

                lastPheromonePosition = position;
            }
        }

        // Follow pheromone trails
        private void SmellPheromones()
        {
            // Choose pheromone type to follow depending on if has food
            List<Pheromone>[,] pheromones = hasFood ? World.homePheromones : World.foodPheromones;

            // Values of the 3 sensors
            float[] sensorValues = new float[3];

            for (int i = -1; i <= 1; i++)
            {
                // Calculate position of a sensor:
                float angle = rotation + i * sensorAngle;
                float xOffset = (float)Math.Cos(angle);
                float yOffset = (float)Math.Sin(angle);
                Vector2 offset = new Vector2(xOffset, yOffset) * sensorDistance;

                Vector2 sensorPosition = position + offset;

                // Save sensor position for debugging purposes
                sensPoses[i + 1] = sensorPosition;

                // Find partition ant is in
                int inPaX = (int)((position.X + 1.0f) / (World.worldWidth / World.partitionsX));
                int inPaY = (int)((position.Y + 1.0f) / (World.worldHeight / World.partitionsY));

                // Search 3 by 3 grid of partitions around ant:
                for (int x = inPaX - 1; x <= inPaX + 1; x++)
                {
                    // Don't look outside of world
                    if (x < 0 || x >= World.partitionsX) continue;

                    for (int y = inPaY - 1; y < inPaY + 1; y++)
                    {
                        // Don't look outside of world
                        if (y < 0 || y >= World.partitionsY) continue;

                        // Add strength of every pheromone in sensor to sensor value:
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

            // Adjust desired direction to move towards strongest sensor:
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

        // Handle "collision":
        private void AvoidObstacles(float deltaTime)
        {
            // Look at two spots in front of ant:
            for (int i = -1; i <= 1; i += 2)
            {
                // Find position of sensor spot:
                float angle = rotation + i * (float)Math.PI / 4.0f;
                float x = (float)Math.Cos(angle);
                float y = (float)Math.Sin(angle);
                Vector2 offset = new Vector2(x, y) * 0.3f;

                Vector2 sensorPosition = position + offset;

                // Turn away if spot has terrain:
                if (Terrain.GetValueAtWorldPoint(sensorPosition.X, sensorPosition.Y) > 0.0f)
                {
                    // Turn away
                    float steerAngle = rotation - i * (float)Math.PI / 2.0f;
                    desiredDirection.X = (float)Math.Cos(steerAngle);
                    desiredDirection.Y = (float)Math.Sin(steerAngle);

                    // Move in opposite direction
                    velocity += desiredDirection * 0.5f;
                    velocity *= 0.90f;
                }
            }

            // Avoid all other ants:
            foreach (Ant ant in World.ants)
            {
                // Move away if too close:
                if (Vector2.Distance(position, ant.position) <= 0.15f && position != ant.position)
                {
                    Vector2 direction = position - ant.position;
                    direction.Normalize();
                    velocity += direction * 5.0f * deltaTime;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw ant:
            spriteBatch.Draw
            (
                antTexture,
                position * Simulation.pixelsPerWorldUnit,
                antTexture.Bounds,
                Color.White,
                rotation,
                new Vector2(antTexture.Width / 2, antTexture.Height / 2),
                0.4f * Simulation.pixelsPerWorldUnit / antTexture.Width,
                SpriteEffects.None,
                0.0f
            );

            // Draw food in front of ant if carrying food:
            if (hasFood)
            {
                // Get front direction
                Vector2 forward;
                forward.X = (float)Math.Cos(rotation);
                forward.Y = (float)Math.Sin(rotation);

                Simulation.DrawCircle(spriteBatch, position + forward * 0.13f, foodColor, 0.15f);
            }

            // Draw sensors to debug:
            for (int i = 0; i < 3; i++)
            {
                //Simulation.DrawCircle(spriteBatch, sensPoses[i], Color.Black * 0.4f, sensorRadius * 2.0f);
            }
        }
    }
}
