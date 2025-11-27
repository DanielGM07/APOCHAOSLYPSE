using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE
{
    public class PauseScene : IScene
    {
        private SpriteFont font;
        private Texture2D pixel;

        private Rectangle btnResume;
        private Rectangle btnMenu;
        private Rectangle btnExit;

        private Color colResume = Color.White;
        private Color colMenu = Color.White;
        private Color colExit = Color.White;

        public void LoadContent()
        {
            var loader = ContentLoader.Instance;
            font = loader.font;

            pixel = new Texture2D(loader.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            int w = loader.graphics.PreferredBackBufferWidth;
            int h = loader.graphics.PreferredBackBufferHeight;

            btnResume = new Rectangle(w / 2 - 150, h / 2 - 100, 300, 60);
            btnMenu   = new Rectangle(w / 2 - 150, h / 2 - 20, 300, 60);
            btnExit   = new Rectangle(w / 2 - 150, h / 2 + 60, 300, 60);
        }

        public void UnloadContent() {}

        public void Update(GameTime gameTime)
        {
            var mouse = Mouse.GetState();
            Point m = mouse.Position;

            colResume = btnResume.Contains(m) ? Color.Yellow : Color.White;
            colMenu   = btnMenu.Contains(m)   ? Color.Yellow : Color.White;
            colExit   = btnExit.Contains(m)   ? Color.Yellow : Color.White;

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (btnResume.Contains(m))
                    Game1.SceneManager.RemoveScene();

                else if (btnMenu.Contains(m))
                {
                    Game1.SceneManager.RemoveScene(); // salir del pause
                    Game1.SceneManager.RemoveScene(); // salir del level
                    Game1.SceneManager.AddScene(new MenuScene());
                    Game1.SceneManager.getScene().LoadContent();
                }
                else if (btnExit.Contains(m))
                    Game1.ExitGameRequested = true;
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(pixel, new Rectangle(0, 0,
                Camera.Instance.Viewport.Width,
                Camera.Instance.Viewport.Height),
                Color.Black * 0.5f);

            spriteBatch.Draw(pixel, btnResume, colResume * 0.4f);
            spriteBatch.Draw(pixel, btnMenu, colMenu * 0.4f);
            spriteBatch.Draw(pixel, btnExit, colExit * 0.4f);

            spriteBatch.DrawString(font, "RESUME",
                new Vector2(btnResume.X + 80, btnResume.Y + 15), colResume);

            spriteBatch.DrawString(font, "MAIN MENU",
                new Vector2(btnMenu.X + 60, btnMenu.Y + 15), colMenu);

            spriteBatch.DrawString(font, "EXIT",
                new Vector2(btnExit.X + 110, btnExit.Y + 15), colExit);
        }
    }
}
