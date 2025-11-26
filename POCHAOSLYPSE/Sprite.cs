using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class Sprite
    {
        public Texture2D texture;
        public Rectangle sourceRectangle = new();
        public Rectangle destinationRectangle = new();
        public float rotation = 0;
        public Vector2 origin = new();
        public Color color;
        public Sprite(Texture2D texture, Rectangle srcRec, Rectangle destRec)
        {
            this.texture = texture;
            this.sourceRectangle = srcRec;
            this.destinationRectangle = destRec;
            color = Color.White;
        }
        public Sprite(Texture2D texture, Rectangle sourceRectangle, Rectangle destRect, float rotation, Vector2 origin, Color color)
        {
            this.texture = texture;
            this.sourceRectangle = sourceRectangle;
            this.destinationRectangle = destRect;
            this.rotation = rotation;
            this.origin = origin;
            this.color = color;
        }
        public virtual void Update(GameTime gameTime)
        { }
        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, color, rotation, origin, SpriteEffects.None, 0);
        }
    }
}
