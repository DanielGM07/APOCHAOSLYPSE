using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class Explosion
    {
        public Vector2 Position;
        public float Radius;
        public float Lifetime;
        public Color Color;

        public bool IsAlive => Lifetime > 0f;

        public Explosion(Vector2 position, float radius, float lifetime, Color color)
        {
            Position = position;
            Radius   = radius;
            Lifetime = lifetime;
            Color    = color;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Lifetime -= dt;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            var rect = new Rectangle(
                (int)(Position.X - Radius),
                (int)(Position.Y - Radius),
                (int)(Radius * 2f),
                (int)(Radius * 2f)
            );

            spriteBatch.Draw(pixel, rect, Color);
        }
    }
}
