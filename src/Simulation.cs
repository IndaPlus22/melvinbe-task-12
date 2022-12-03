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

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            World.circleTexture = Content.Load<Texture2D>("circle");
            Ant.texture = Content.Load<Texture2D>("ant");
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            World.Update(deltaTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(146, 106, 85, 255));

            spriteBatch.Begin();

            World.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}