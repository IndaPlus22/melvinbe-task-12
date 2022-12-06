using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Antoids
{
    public class Simulation : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public const float windowScale = 60.0f;
        public const int windowWidth = (int)(World.worldWidth * windowScale);
        public const int windowHeight = (int)(World.worldHeight * windowScale);

        public static Texture2D circleTexture;

        public static Color groundColor = new Color(98, 77, 66, 255);
        public static Color wallColor =   new Color(64, 52, 54, 255);
        public static Color foodColor1 = Color.YellowGreen;
        public static Color foodColor2 = Color.GreenYellow;
        //public static Color foodPheromoneColor = Color.IndianRed;
        //public static Color homePheromoneColor = Color.CornflowerBlue;
        public static Color foodPheromoneColor = Color.YellowGreen;
        public static Color homePheromoneColor = Color.IndianRed;
        public static Color nestColor = Color.Brown;

        public static bool showPheromones = true;
        private static bool pressedP;

        public Simulation()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "res";
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            World.Init();

            Terrain.Init(GraphicsDevice);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            circleTexture = Content.Load<Texture2D>("circle");
            Ant.antTexture = Content.Load<Texture2D>("ant");
            Brush.cursorTexture = Content.Load<Texture2D>("cursor");
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.P) && !pressedP)
            {
                showPheromones = !showPheromones;
                pressedP = true;
            }
            if (Keyboard.GetState().IsKeyUp(Keys.P))
                pressedP = false;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Brush.Update(deltaTime);

            World.Update(deltaTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(groundColor);

            spriteBatch.Begin();

            World.Draw(spriteBatch);

            spriteBatch.End();

            Terrain.Draw(spriteBatch);

            spriteBatch.Begin();

            Brush.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public static void DrawCircle(SpriteBatch spriteBatch, Vector2 position, Color color, float scale)
        {
            spriteBatch.Draw
            (
                circleTexture,
                position * windowScale,
                circleTexture.Bounds,
                color,
                0.0f,
                new Vector2(circleTexture.Width / 2, circleTexture.Height / 2),
                scale * windowScale / circleTexture.Width,
                SpriteEffects.None,
                0.0f
            );
        }
    }
}