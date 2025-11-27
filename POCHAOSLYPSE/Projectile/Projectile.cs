using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class Projectile
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Damage;
        public float Lifetime;
        public float Radius;
        public Color Color;
        public bool IsAlive => Lifetime > 0f;

        public Projectile(Vector2 position, Vector2 velocity,
                          float damage, float lifetime,
                          Color color, float radius)
        {
            Position = position;
            Velocity = velocity;
            Damage = damage;
            Lifetime = lifetime;
            Color = color;
            Radius = radius;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += Velocity * dt;
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
