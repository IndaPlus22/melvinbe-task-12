using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

        private static bool movingNest;
        private static bool placingFood;

        private const float foodDelay = 0.03f;
        private static float foodTime;

        private static Vector2 mousePosition;

        public static void Update(float deltaTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.D1))
                placingFood = false;

            if (Keyboard.GetState().IsKeyDown(Keys.D2))
                placingFood = true;


            MouseState state = Mouse.GetState();

            mousePosition.X = Math.Clamp(state.X * World.worldWidth / Simulation.windowWidth, 0.0f, World.worldWidth);
            mousePosition.Y = Math.Clamp(state.Y * World.worldHeight / Simulation.windowHeight, 0.0f, World.worldHeight);

            if (state.ScrollWheelValue < lastScrollWheelValue)
            {
                brushRadius = Math.Clamp(brushRadius * scrollSpeed, minRadius, maxRadius);
            }
            else if (state.ScrollWheelValue > lastScrollWheelValue)
            {
                brushRadius = Math.Clamp(brushRadius / scrollSpeed, minRadius, maxRadius);
            }

            lastScrollWheelValue = state.ScrollWheelValue;

            if (state.LeftButton == ButtonState.Released)
            {
                clicked = false;

                movingNest = false;
            }

            if (movingNest)
            {
                MoveNest(mousePosition);

                return;
            }

            if (state.LeftButton == ButtonState.Pressed)
            {
                if (!clicked)
                {
                    clicked = true;

                    if (Vector2.Distance(mousePosition, World.nestPosition) < World.nestRadius)
                    {
                        movingNest = true;

                        return;
                    }

                    foodTime = foodDelay;
                }

                if (placingFood)
                {
                    MakeFood(deltaTime);
                }
                else
                {
                    MakeDirt();
                }
            }

            if (state.RightButton == ButtonState.Pressed)
            {
                if (placingFood)
                {
                    RemoveFood();
                }
                else
                {
                    RemoveDirt(mousePosition, brushRadius);
                }
            }
        }

        public static void MoveNest(Vector2 position)
        {
            World.nestPosition = position;

            RemoveDirt(World.nestPosition, World.nestRadius * 1.25f);
        }

        private static void MakeDirt()
        {
            World.ants = World.ants.Where(ant => Vector2.Distance(mousePosition, ant.position) > brushRadius).ToList();

            RemoveFood();

            bool updateNeeded = false;

            for (int x = 2; x < Terrain.gridWidth - 2; x++)
            {
                for (int y = 2; y < Terrain.gridHeight - 2; y++)
                {
                    Vector2 cellPosition = new Vector2(x, y) + Vector2.One * 0.5f;
                    Vector2 mouseCellPosition = mousePosition * Terrain.cellsPerUnit;

                    float distance = Vector2.Distance(cellPosition, mouseCellPosition) * 0.85f;

                    if (distance < brushRadius * Terrain.cellsPerUnit)
                    {
                        Terrain.values[x, y] = Math.Max(Terrain.values[x, y], 5.0f - (distance * 1.25f / brushRadius));

                        updateNeeded = true;
                    }
                }
            }

            RemoveDirt(World.nestPosition, World.nestRadius * 1.25f);

            if (updateNeeded)
            {
                Terrain.GenerateVertices();
            }
        }

        private static void RemoveDirt(Vector2 position, float radius)
        {
            bool updateNeeded = false;

            for (int x = 2; x < Terrain.gridWidth - 2; x++)
            {
                for (int y = 2; y < Terrain.gridHeight - 2; y++)
                {
                    Vector2 cellPosition = new Vector2(x, y) + Vector2.One * 0.5f;
                    Vector2 mouseCellPosition = position * Terrain.cellsPerUnit;

                    float distance = Vector2.Distance(cellPosition, mouseCellPosition) * 0.85f;

                    if (distance < radius * Terrain.cellsPerUnit)
                    {
                        Terrain.values[x, y] = Math.Min(Terrain.values[x, y], -5.0f + (distance * 1.25f / radius));

                        updateNeeded = true;
                    }
                }
            }

            if (updateNeeded)
            {
                Terrain.GenerateVertices();
            }
        }

        private static void MakeFood(float deltaTime)
        {
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

        private static void RemoveFood()
        {
            foreach (Food food in World.foods)
            {
                if (Vector2.Distance(mousePosition, food.position) <= brushRadius)
                {
                    food.willBeRemoved = true;
                }
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw
            (
                cursorTexture,
                mousePosition * Simulation.windowScale,
                cursorTexture.Bounds,
                (placingFood ? Color.GreenYellow : Color.Black) * 0.3f,
                0.0f,
                new Vector2(cursorTexture.Width / 2, cursorTexture.Height / 2),
                brushRadius * 2.0f * Simulation.windowScale / cursorTexture.Width,
                SpriteEffects.None,
                0.0f
            );
        }
    }
}
