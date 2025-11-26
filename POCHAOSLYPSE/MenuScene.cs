using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE;

public class MenuScene : IScene
{
    Texture2D background;
    string menustr = "APOCHAOSLYPSE";
    Vector2 positioner = new();
    SpriteFont fontUsed;
    Viewport vport;
    public void LoadContent()
    {
        fontUsed = ContentLoader.Instance.font;
        vport = ContentLoader.Instance.graphics.GraphicsDevice.Viewport;
        Vector2 measure = fontUsed.MeasureString(menustr);
        Console.WriteLine($"measue: {measure}");
        positioner = new(
            vport.Width/2 - measure.X/2,
            vport.Height/2 - measure.Y/2
        );
    }
    public void UnloadContent()
    { }
    public void Update(GameTime gameTime)
    {
    }
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(fontUsed, menustr, positioner, Color.White);
    }
}
