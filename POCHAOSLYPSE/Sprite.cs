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

        public Sprite(Texture2D texture, Rectangle srcRec, Rectangle destRec, Color color)
        {
            this.texture = texture;
            this.sourceRectangle = srcRec;
            this.destinationRectangle = destRec;
            this.color = color;
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

        /// <summary>
        /// Centro del rectángulo destino en coordenadas de mundo.
        /// </summary>
        public Vector2 Center
        {
            get => destinationRectangle.Center.ToVector2();
            set
            {
                destinationRectangle.X = (int)(value.X - destinationRectangle.Width / 2f);
                destinationRectangle.Y = (int)(value.Y - destinationRectangle.Height / 2f);
            }
        }

        public virtual void Update(GameTime gameTime)
        { }

        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // Escala para adaptar source -> destino
            Vector2 scale = new Vector2(
                destinationRectangle.Width / (float)sourceRectangle.Width,
                destinationRectangle.Height / (float)sourceRectangle.Height
            );

            if (origin == Vector2.Zero)
            {
                origin = new Vector2(
                    sourceRectangle.Width / 2f,
                    sourceRectangle.Height / 2f
                );
            }

            spriteBatch.Draw(
                texture,
                Center,
                sourceRectangle,
                color,
                rotation,
                origin,
                scale,
                facingLeft,
                0f
            );
        }
    }
}
