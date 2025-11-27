using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE
{
    public class MenuScene : IScene
    {
        private SpriteFont font;
        private Texture2D pixel;

        private Rectangle btnLevel1;
        private Rectangle btnLevel2;
        private Rectangle btnExit;

        private Color colLevel1 = Color.White;
        private Color colLevel2 = Color.White;
        private Color colExit   = Color.White;

        // ✅ para detectar click solo una vez
        private MouseState prevMouse;

        public void LoadContent()
        {
          Globals.color = Color.Black;
            var loader = ContentLoader.Instance;
            font = loader.font;

            pixel = new Texture2D(loader.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            // DIMENSIONES REALES DE LA PANTALLA
            int w = loader.graphics.GraphicsDevice.Viewport.Width;
            int h = loader.graphics.GraphicsDevice.Viewport.Height;

            // Tres botones centrados verticalmente
            btnLevel1 = new Rectangle(w / 2 - 150, h / 2 - 110, 300, 60);
            btnLevel2 = new Rectangle(w / 2 - 150, h / 2 - 20,  300, 60);
            btnExit   = new Rectangle(w / 2 - 150, h / 2 + 70,  300, 60);

            // Inicializar prevMouse para que el primer frame no cuente como click
            prevMouse = Mouse.GetState();
        }

        public void UnloadContent() {}

        public void Update(GameTime gameTime)
        {
            var mouse = Mouse.GetState();
            Point m = mouse.Position;

            bool leftDown    = mouse.LeftButton == ButtonState.Pressed;
            bool leftWasDown = prevMouse.LeftButton == ButtonState.Pressed;
            bool leftJustPressed = leftDown && !leftWasDown;

            // Hover
            colLevel1 = btnLevel1.Contains(m) ? Color.Yellow : Color.White;
            colLevel2 = btnLevel2.Contains(m) ? Color.Yellow : Color.White;
            colExit   = btnExit.Contains(m)   ? Color.Yellow : Color.White;

            // ✅ Click sólo en el frame donde se PRESIONA, no mientras se mantiene
            if (leftJustPressed)
            {
                if (btnLevel1.Contains(m))
                {
                    Game1.SceneManager.AddScene(new PlayScene(
                        "tiled/test1.csv",
                        "Content/Another Metroidvania Asset Pack Vol. 1 ver. 1.5/Tilesets/library/tileset_library.png"
                    ));
                    Game1.SceneManager.getScene().LoadContent();
                }
                else if (btnLevel2.Contains(m))
                {
                    Game1.SceneManager.AddScene(new PlayScene(
                        "tiled/level2.csv",
                        "Content/Another Metroidvania Asset Pack Vol. 1 ver. 1.5/Tilesets/library/tileset_library.png"
                    ));
                    Game1.SceneManager.getScene().LoadContent();
                }
                else if (btnExit.Contains(m))
                {
                    Game1.ExitGameRequested = true;
                }
            }

            // Guardar estado para el próximo frame
            prevMouse = mouse;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        { }
        public void DrawUI(GameTime gameTime, SpriteBatch spriteBatch)
        {
            var loader = ContentLoader.Instance;

            // Fondo
            spriteBatch.Draw(pixel, new Rectangle(0, 0,
                loader.graphics.GraphicsDevice.Viewport.Width,
                loader.graphics.GraphicsDevice.Viewport.Height),
                Color.Black * 0.6f);

            // Botón Level 1
            spriteBatch.Draw(pixel, btnLevel1, colLevel1 * 0.4f);
            spriteBatch.DrawString(font, "LEVEL 1",
                new Vector2(btnLevel1.X + 70, btnLevel1.Y + 15), colLevel1);

            // Botón Level 2
            spriteBatch.Draw(pixel, btnLevel2, colLevel2 * 0.4f);
            spriteBatch.DrawString(font, "LEVEL 2",
                new Vector2(btnLevel2.X + 70, btnLevel2.Y + 15), colLevel2);

            // Botón Exit
            spriteBatch.Draw(pixel, btnExit, colExit * 0.4f);
            spriteBatch.DrawString(font, "EXIT",
                new Vector2(btnExit.X + 110, btnExit.Y + 15), colExit);

            spriteBatch.DrawString(font, "APOCHAOSLYPSE",
                new Vector2(40, 40), Color.White);
        }
    }
}
