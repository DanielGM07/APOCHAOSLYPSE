using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public static SceneManager SceneManager;
        public static bool ExitGameRequested = false;
        private KeyboardState _prevKeyboard;

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
            {
                Exit();
                return;
            }

            var kb = Keyboard.GetState();
            bool escJustPressed = kb.IsKeyDown(Keys.Escape) && !_prevKeyboard.IsKeyDown(Keys.Escape);

            if (escJustPressed)
            {
                var current = SceneManager.getScene();

                if (current is PlayScene)
                {
                    SceneManager.AddScene(new PauseScene());
                    SceneManager.getScene().LoadContent();
                }
                else if (current is PauseScene)
                {
                    SceneManager.RemoveScene();
                }
            }

            SceneManager.getScene().Update(gameTime);

            _prevKeyboard = kb;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            var current = SceneManager.getScene();

            if (current is PlayScene)
                GraphicsDevice.Clear(new Color(200, 200, 200));
            else
                GraphicsDevice.Clear(Color.Black);

            // --------------------
            // DIBUJO DEL MUNDO
            // --------------------
            if (current is MenuScene || current is PauseScene)
            {
                _spriteBatch.Begin(
                    samplerState: SamplerState.PointClamp
                );
            }
            else
            {
                _spriteBatch.Begin(
                    samplerState: SamplerState.PointWrap,
                    transformMatrix: Camera.Instance.Matrix
                );
            }

            current.Draw(gameTime, _spriteBatch);

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
