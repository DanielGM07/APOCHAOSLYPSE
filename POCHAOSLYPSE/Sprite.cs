using System;
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
        public SpriteEffects facingLeft;
        public Sprite(Texture2D texture, Rectangle srcRec, Rectangle destRec)
        {
            this.texture = texture;
            this.sourceRectangle = srcRec;
            this.destinationRectangle = destRec;
            color = Color.White;
            facingLeft = SpriteEffects.None;
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
    // Centro del rect destino en mundo
    Vector2 position = destinationRectangle.Center.ToVector2();

    // Escala para adaptar source -> destino
    Vector2 scale = new Vector2(
        destinationRectangle.Width  / (float)sourceRectangle.Width,
        destinationRectangle.Height / (float)sourceRectangle.Height
    );

    // Si no te dieron origin, usá el centro del source
    if (origin == Vector2.Zero)
    {
        origin = new Vector2(
            sourceRectangle.Width  / 2f,
            sourceRectangle.Height / 2f
        );
    }

    spriteBatch.Draw(
        texture,
        position,            // centro en mundo
        sourceRectangle,
        color,
        rotation,            // en radianes
        origin,              // centro del source
        scale,
        facingLeft,
        0f
    );
}

    }
}
