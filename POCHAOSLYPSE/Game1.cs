using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SceneManager sceneManager;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Ventana a tamaño de monitor, en modo ventana
            var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            _graphics.PreferredBackBufferWidth  = displayMode.Width;
            _graphics.PreferredBackBufferHeight = displayMode.Height;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();

            // Inicializar cámara con esas dimensiones
            Camera.Initialize(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        }

        protected override void Initialize()
        {
            // ContentLoader carga font y deja GraphicsDevice accesible
            ContentLoader.Initialize(_graphics, Content, "Content/Another Metroidvania Asset Pack Vol. 1 ver. 1.5");

            Camera.Instance.CenterOrigin();

            sceneManager = new SceneManager();

            // Por ahora arrancamos directo en gameplay
            sceneManager.AddScene(new PlayScene());

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Cargar contenido de la escena actual
            sceneManager.getScene().LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            // Salir con Escape o botón Back
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            sceneManager.getScene().Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(
                samplerState: SamplerState.PointWrap,
                transformMatrix: Camera.Instance.Matrix
            );

            sceneManager.getScene().Draw(gameTime, _spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
