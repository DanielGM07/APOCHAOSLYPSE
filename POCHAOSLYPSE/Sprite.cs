using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE
{
    public class Sprite
    {
        Texture2D texture;
        Rectangle sourceRectangle = new();
        Rectangle destinationRectangle = new();
        public Sprite(Texture2D texture, Rectangle srcRec, Rectangle destRec)
        {
            this.texture = texture;
            this.sourceRectangle = srcRec;
            this.destinationRectangle = destRec;
        }
        public void Update(GameTime gameTime)
        { }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, Color.White);
        }

    }
}
