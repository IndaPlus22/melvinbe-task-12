using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Antoids
{
    public class Simulation : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private static Texture2D circleTexture;

        // Information about the game window:
        public const float pixelsPerWorldUnit = 60.0f;
        public const int windowWidth = (int)(World.worldWidth * pixelsPerWorldUnit);
        public const int windowHeight = (int)(World.worldHeight * pixelsPerWorldUnit);

        // Some color values that can be changed globally from here:
        public static Color groundColor        = new Color(98, 77, 66, 255);
        public static Color terrainColor          = new Color(64, 52, 54, 255);
        public static Color foodColor1         =     Color.YellowGreen;
        public static Color foodColor2         =     Color.GreenYellow;
        public static Color foodPheromoneColor =     Color.YellowGreen;
        public static Color homePheromoneColor =     Color.IndianRed;
        public static Color nestColor1         = new Color(102, 57, 49, 255);
        public static Color nestColor2         = new Color(143, 86, 59, 255);
        public static Color brushColorDirt     = new Color(30, 25, 25, 255);
        public static Color brushColorFood     =     Color.YellowGreen;

        // Keep track of current and last state to detect any changes
        public static KeyboardState keyboardState;
        public static KeyboardState lastKeyboardState;

        // Is the simulation running?
        public static bool simulating = false;

        // Run at double speed
        private static bool doubleSpeed;

        // Enable rendering of pheromones
        public static bool showPheromones = true;

        public Simulation()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "res";
            IsMouseVisible = true;

            // Set window size
            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            Terrain.Init(GraphicsDevice);

            World.Clean();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load textures:
            circleTexture = Content.Load<Texture2D>("circle");
            Ant.antTexture = Content.Load<Texture2D>("ant");
            Brush.cursorTexture = Content.Load<Texture2D>("cursor");
        }

        protected override void Update(GameTime gameTime)
        {
            // Save last state and update current
            lastKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();

            // Close application when pressing escape
            if (keyboardState.IsKeyDown(Keys.Escape) && lastKeyboardState.IsKeyUp(Keys.Escape))
                Exit();

            // Toggle simulation properties:
            if (keyboardState.IsKeyDown(Keys.Space) && lastKeyboardState.IsKeyUp(Keys.Space))
                simulating = !simulating;

            if (keyboardState.IsKeyDown(Keys.Right) && lastKeyboardState.IsKeyUp(Keys.Right))
                doubleSpeed = !doubleSpeed;

            if (keyboardState.IsKeyDown(Keys.P) && lastKeyboardState.IsKeyUp(Keys.P))
                showPheromones = !showPheromones;

            // Regenerate terrain, clean world and pause simulation:
            if (keyboardState.IsKeyDown(Keys.R) && lastKeyboardState.IsKeyUp(Keys.R))
            {
                Terrain.GenerateGridValues();
                Terrain.GenerateVertices();
                World.Clean();
                simulating = false;

                // Remove dirt around nest that may have generated
                Brush.CleanNest(); 
            }

            // Clean world and pause simulation
            if (keyboardState.IsKeyDown(Keys.C) && lastKeyboardState.IsKeyUp(Keys.C))
            {
                simulating = false;
                World.Clean();
            }

            // Get time that passed since last frame
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Multiply by two if running att double speed
            if (doubleSpeed) deltaTime *= 2.0f; 

            // Update brush even when paused to allow editing
            Brush.Update(deltaTime); 

            // Don't update world if simulation is paused
            if (simulating) World.Update(deltaTime); 

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(groundColor);

            // Draw world in spritebatch:
            spriteBatch.Begin();

            World.Draw(spriteBatch);

            spriteBatch.End();

            // Draw terrain mesh seperatly because it is a "user indexed primitive"
            Terrain.Draw();

            // Draw brush last (on top) 
            spriteBatch.Begin();

            Brush.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        // A general function for drawing circles
        public static void DrawCircle(SpriteBatch spriteBatch, Vector2 position, Color color, float scale)
        {
            spriteBatch.Draw
            (
                circleTexture,
                position * pixelsPerWorldUnit,
                circleTexture.Bounds,
                color,
                0.0f,
                new Vector2(circleTexture.Width / 2, circleTexture.Height / 2),
                scale * pixelsPerWorldUnit / circleTexture.Width,
                SpriteEffects.None,
                0.0f
            );
        }
    }
}