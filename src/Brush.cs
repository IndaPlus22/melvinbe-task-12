using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Diagnostics;
using System.Linq;

namespace Antoids
{
    public static class Brush
    {
        public static Texture2D cursorTexture;

        private static Random random = new Random();

        // Brush size:
        private static float brushRadius = 1.25f;
        private static float minRadius = 0.2f;
        private static float maxRadius = 7.5f;

        // Scroll sensitivity:
        private const float scrollSpeed = 1.1f;
        private static int lastScrollWheelValue;

        // What "mode" the cursor is in:
        private static bool movingNest;
        private static bool placingFood;

        // Food placing:
        private const float foodDelay = 0.025f;
        private static float foodTime;

        private static Vector2 mousePosition;
        // Storing the one useful bool istead of entire last mouse state
        private static bool clicked;

        public static void Update(float deltaTime)
        {
            // Toggle between placing food or dirt
            if (Simulation.keyboardState.IsKeyDown(Keys.Tab) && Simulation.lastKeyboardState.IsKeyUp(Keys.Tab))
                placingFood = !placingFood;

            MouseState state = Mouse.GetState();

            // Convert from screen space to world space
            // and limit to inside of screen
            mousePosition.X = Math.Clamp(state.X * World.worldWidth / Simulation.windowWidth, 0.0f, World.worldWidth);
            mousePosition.Y = Math.Clamp(state.Y * World.worldHeight / Simulation.windowHeight, 0.0f, World.worldHeight);

            // Increase or decrease brush size when scrolling:
            if (state.ScrollWheelValue < lastScrollWheelValue)
            {
                brushRadius = Math.Min(brushRadius * scrollSpeed, maxRadius);
            }
            else if (state.ScrollWheelValue > lastScrollWheelValue)
            {
                brushRadius = Math.Max(brushRadius / scrollSpeed, minRadius);
            }
            // Save last value to detect changes in wheel position
            lastScrollWheelValue = state.ScrollWheelValue;

            // Reset click and let go of nest when mouse button is released:
            if (state.LeftButton == ButtonState.Released)
            {
                clicked = false;

                movingNest = false;
            }

            // Move nest to mouse position and remove dirt around it:
            if (movingNest)
            {
                World.nestPosition = mousePosition;

                CleanNest();

                return;
            }

            if (state.LeftButton == ButtonState.Pressed)
            {
                if (!clicked) // The frame the button is pressed:
                {
                    clicked = true;

                    // Grab nest if click was overlapping
                    if (Vector2.Distance(mousePosition, World.nestPosition) < World.nestRadius)
                    {
                        movingNest = true;

                        return;
                    }

                    // Guarantees that at least one food will be placed per click
                    // For when cursor is very small
                    foodTime = foodDelay;
                }

                // Left click to place
                if (placingFood)
                {
                    MakeFood(deltaTime);
                }
                else
                {
                    MakeDirt();
                }
            }

            // Right click to remove
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

        // Removes any dirt around nest
        public static void CleanNest()
        {
            RemoveDirt(World.nestPosition, World.nestRadius * 1.25f);
        }

        // Creates dirt inside cursor
        private static void MakeDirt()
        {
            // Remove any ants that happen to be too close to cursor :(
            World.ants = World.ants.Where(ant => Vector2.Distance(mousePosition, ant.position) > brushRadius).ToList();

            // Remove food inside cursor
            RemoveFood();

            // Assume that no regeneration is needed before values are updated
            bool updateNeeded = false;

            // +-2 because edges can't be removed
            for (int x = 2; x < Terrain.gridWidth - 2; x++)
            {
                for (int y = 2; y < Terrain.gridHeight - 2; y++)
                {
                    // Offset to center of cells
                    Vector2 cellPosition = new Vector2(x, y) + Vector2.One * 0.5f;
                    // Convert mouse world position to cell position
                    Vector2 mouseCellPosition = mousePosition * Terrain.cellsPerWorldUnit;

                    float distance = Vector2.Distance(cellPosition, mouseCellPosition) * 0.85f;

                    if (distance < brushRadius * Terrain.cellsPerWorldUnit)
                    {
                        // Don't add too much around edges for smoothness
                        // Also don't add more if existing value is greater than added value
                        Terrain.values[x, y] = Math.Max(Terrain.values[x, y], 5.0f - (distance * 1.25f / brushRadius));

                        // A cell changed, update is needed after all
                        updateNeeded = true;
                    }
                }
            }

            // Remove dirt that may have been placed near nest
            RemoveDirt(World.nestPosition, World.nestRadius * 1.25f);

            // Regenerate only if needed
            if (updateNeeded) Terrain.GenerateVertices();
        }

        // Removes dirt inside cursor
        private static void RemoveDirt(Vector2 position, float radius)
        {
            // Assume that no regeneration is needed before values are updated
            bool updateNeeded = false;

            // +-2 because edges can't be removed
            for (int x = 2; x < Terrain.gridWidth - 2; x++)
            {
                for (int y = 2; y < Terrain.gridHeight - 2; y++)
                {
                    // Offset to center of cells
                    Vector2 cellPosition = new Vector2(x, y) + Vector2.One * 0.5f;
                    // Convert mouse world position to cell position
                    Vector2 mouseCellPosition = position * Terrain.cellsPerWorldUnit;

                    float distance = Vector2.Distance(cellPosition, mouseCellPosition) * 0.85f;

                    if (distance < radius * Terrain.cellsPerWorldUnit)
                    {
                        // Don't remove too much around edges for smoothness
                        // Also don't remove more if existing value is greater than remove value
                        Terrain.values[x, y] = Math.Min(Terrain.values[x, y], -5.0f + (distance * 1.25f / radius));

                        // A cell changed, update is needed after all
                        updateNeeded = true;
                    }
                }
            }

            // Regenerate only if needed
            if (updateNeeded) Terrain.GenerateVertices();
        }

        // Creates food inside cursor
        private static void MakeFood(float deltaTime)
        {
            while (foodTime <= foodDelay)
            {
                // Random position inside cursor
                Vector2 foodPosition = mousePosition + MathHelper.RandomInsideUnitCircle() * brushRadius;

                // Don't create food if dirt is in the way
                if (Terrain.GetValueAtWorldPoint(foodPosition.X, foodPosition.Y) <= 0.0f)
                {
                    // Random color between 1 and 2
                    Color foodColor = Color.Lerp(Simulation.foodColor1, Simulation.foodColor2, (float)random.NextDouble());

                    World.foods.Add(new Food(foodPosition, foodColor));
                }

                foodTime += foodDelay;
            }
            // Create food more often if brush is bigger
            foodTime -= deltaTime * brushRadius * brushRadius * (float)Math.PI;
        }

        // Remove food inside cursor
        private static void RemoveFood()
        {
            foreach (Food food in World.foods)
            {
                // Mark food for removal if too close
                if (Vector2.Distance(mousePosition, food.position) <= brushRadius)
                {
                    food.willBeRemoved = true;
                }
            }

            // Force food cleanup in case game is paused
            World.foods = World.foods.Where(food => !food.willBeRemoved).ToList();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw
            (
                cursorTexture,
                mousePosition * Simulation.pixelsPerWorldUnit,
                cursorTexture.Bounds,
                (placingFood ? Simulation.brushColorFood : Simulation.brushColorDirt) * 0.8f, // Color based on mode
                0.0f,
                new Vector2(cursorTexture.Width / 2, cursorTexture.Height / 2),
                brushRadius * 2.0f * Simulation.pixelsPerWorldUnit / cursorTexture.Width,
                SpriteEffects.None,
                0.0f
            );
        }
    }
}
