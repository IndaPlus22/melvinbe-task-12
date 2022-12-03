using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Diagnostics;

namespace Antoids
{
    public class Ant
    {
        public static Texture2D texture;
        private float scale = 0.08f;

        private const float maxSpeed = 1.0f;
        private const float steerStrength = 2.0f;
        private const float wanderStrength = 0.15f;

        private Vector2 position;
        private Vector2 velocity = Vector2.One;
        private Vector2 desiredDirection;
        private float rotation;

        private bool hasFood;

        public Ant(Vector2 _position, Vector2 _velocity)
        {
            position = _position;
            velocity = _velocity;
        }

        public void Update(float deltaTime)
        {
            FindFood(deltaTime);

            desiredDirection = desiredDirection + MathHelper.RandomInsideUnitCircle() * wanderStrength;
            desiredDirection.Normalize();

            Vector2 desiredVelocity = desiredDirection * maxSpeed;
            Vector2 desiredSteeringForce = (desiredVelocity - velocity) * steerStrength;
            Vector2 acceleration = MathHelper.ClampNorm(desiredSteeringForce, steerStrength) / 1.0f;

            velocity = MathHelper.ClampNorm(velocity + acceleration * deltaTime, maxSpeed);
            position += velocity * deltaTime * 30.0f;

            rotation = (float)Math.Atan2(velocity.Y, velocity.X);
        }

        private void FindFood(float deltaTime)
        {

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
                scale * Simulation.windowScale,
                SpriteEffects.None,
                0.0f
            );
        }
    }
}
