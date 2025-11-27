using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        public static Color color = Color.Gray;
        private SpriteBatch _spriteBatch;
        public static SceneManager SceneManager; // jraphics should also be using this trick
        public static bool ExitGameRequested = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            _graphics.PreferredBackBufferWidth = displayMode.Width;
            _graphics.PreferredBackBufferHeight = displayMode.Height;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();

            Camera.Initialize(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        }

        protected override void Initialize()
        {
            ContentLoader.Initialize(_graphics, Content, "Content/Another Metroidvania Asset Pack Vol. 1 ver. 1.5");

            Camera.Instance.CenterOrigin();

            SceneManager = new SceneManager();
            SceneManager.AddScene(new MenuScene());

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            SceneManager.getScene().LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (ExitGameRequested)
            { Exit(); }

            SceneManager.getScene().Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            var current = SceneManager.getScene();

            GraphicsDevice.Clear(color);

            _spriteBatch.Begin(
                samplerState: SamplerState.PointWrap,
                transformMatrix: Camera.Instance.Matrix
            );
            current.Draw(gameTime, _spriteBatch);
            _spriteBatch.End();

            _spriteBatch.Begin(samplerState: SamplerState.PointWrap);
            SceneManager.getScene().DrawUI(gameTime, _spriteBatch);
            _spriteBatch.End();

            // --------------------
            //     HUD (PlayScene)
            // --------------------
            if (current is PlayScene ps)
            {
                _spriteBatch.Begin(
                    samplerState: SamplerState.PointClamp
                );

                ps.DrawHUD(_spriteBatch);

                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
