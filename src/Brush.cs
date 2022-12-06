using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Antoids
{
    public static class Brush
    {
        public static Texture2D cursorTexture;

        private static Random random = new Random();

        private static bool clicked;

        private static float brushRadius = 1.25f;
        private static float minRadius = 0.2f;
        private static float maxRadius = 7.5f;

        private const float scrollSpeed = 1.1f;
        private static int lastScrollWheelValue;

        private const float foodDelay = 0.03f;
        private static float foodTime;

        private static Vector2 mousePosition;

        public static void Update(float deltaTime)
        {
            MouseState state = Mouse.GetState();

            mousePosition.X = state.X * World.worldWidth / Simulation.windowWidth;
            mousePosition.Y = state.Y * World.worldHeight / Simulation.windowHeight;

            if (state.LeftButton == ButtonState.Pressed)
            {
                if (!clicked)
                {
                    foodTime = foodDelay;

                    clicked = true;
                }

                while (foodTime <= foodDelay)
                {
                    Vector2 foodPosition = mousePosition + MathHelper.RandomInsideUnitCircle() * brushRadius;
                    Color foodColor = Color.Lerp(Simulation.foodColor1, Simulation.foodColor2, (float)random.NextDouble());

                    if (Terrain.GetValueAtPoint(foodPosition.X, foodPosition.Y) <= 0.0f)
                    {
                        World.foods.Add(new Food(foodPosition, foodColor));
                    }

                    foodTime += foodDelay;
                }
                foodTime -= deltaTime * brushRadius * brushRadius * (float)Math.PI;
            }
            if (state.LeftButton == ButtonState.Released)
            {
                clicked = false;
            }

            if (state.RightButton == ButtonState.Pressed)
            {
                foreach (Food food in World.foods)
                {
                    if (Vector2.Distance(mousePosition, food.position) <= brushRadius)
                    {
                        food.willBeRemoved = true;
                    }
                }
            }

            if (state.ScrollWheelValue > lastScrollWheelValue)
            {
                brushRadius = Math.Clamp(brushRadius * scrollSpeed, minRadius, maxRadius);
            }
            else if (state.ScrollWheelValue < lastScrollWheelValue)
            {
                brushRadius = Math.Clamp(brushRadius / scrollSpeed, minRadius, maxRadius);
            }

            lastScrollWheelValue = state.ScrollWheelValue;
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw
            (
                cursorTexture,
                mousePosition * Simulation.windowScale,
                cursorTexture.Bounds,
                Color.Yellow * 0.2f,
                0.0f,
                new Vector2(cursorTexture.Width / 2, cursorTexture.Height / 2),
                brushRadius * 2.0f * Simulation.windowScale / cursorTexture.Width,
                SpriteEffects.None,
                0.0f
            );
        }
    }
}
