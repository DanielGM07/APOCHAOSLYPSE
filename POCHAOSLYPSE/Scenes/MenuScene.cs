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
        private Rectangle btnExit;

        private Color colLevel1 = Color.White;
        private Color colExit = Color.White;

        public void LoadContent()
        {
            var loader = ContentLoader.Instance;
            font = loader.font;

            pixel = new Texture2D(loader.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            // ✅ DIMENSIONES REALES DE LA PANTALLA
            int w = loader.graphics.GraphicsDevice.Viewport.Width;
            int h = loader.graphics.GraphicsDevice.Viewport.Height;

            // Botones centrados correctamente
            btnLevel1 = new Rectangle(w / 2 - 150, h / 2 - 70, 300, 60);
            btnExit   = new Rectangle(w / 2 - 150, h / 2 + 20, 300, 60);
        }

        public void UnloadContent() {}

        public void Update(GameTime gameTime)
        {
            var mouse = Mouse.GetState();
            Point m = mouse.Position;

            colLevel1 = btnLevel1.Contains(m) ? Color.Yellow : Color.White;
            colExit   = btnExit.Contains(m)   ? Color.Yellow : Color.White;

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (btnLevel1.Contains(m))
                {
                    Game1.SceneManager.AddScene(new PlayScene(
                        "tiled/test1.csv",
                        "Content/Another Metroidvania Asset Pack Vol. 1 ver. 1.5/Tilesets/library/tileset_library.png"
                    ));
                    Game1.SceneManager.getScene().LoadContent();
                }
                else if (btnExit.Contains(m))
                {
                    Game1.ExitGameRequested = true;
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Fondo opcional (negro)
            spriteBatch.Draw(pixel, new Rectangle(0, 0,
                ContentLoader.Instance.graphics.GraphicsDevice.Viewport.Width,
                ContentLoader.Instance.graphics.GraphicsDevice.Viewport.Height),
                Color.Black * 0.6f);

            // Botón Level 1
            spriteBatch.Draw(pixel, btnLevel1, colLevel1 * 0.4f);
            spriteBatch.DrawString(font, "LEVEL 1",
                new Vector2(btnLevel1.X + 70, btnLevel1.Y + 15), colLevel1);

            // Botón Exit
            spriteBatch.Draw(pixel, btnExit, colExit * 0.4f);
            spriteBatch.DrawString(font, "EXIT",
                new Vector2(btnExit.X + 110, btnExit.Y + 15), colExit);

            // Título
            spriteBatch.DrawString(font, "APOCHAOSLYPSE",
                new Vector2(40, 40), Color.White);
        }
    }
}
