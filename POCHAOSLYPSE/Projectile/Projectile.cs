using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class Projectile //TODO: make this a Sprite child
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Damage;
        public float Lifetime;
        public float Radius;
        public Color Color;
        public bool IsAlive => Lifetime > 0f;

        // 🔹 Nuevo: explosivo o no
        public bool IsExplosive;
        public float ExplosionRadius;

        public Projectile(Vector2 position, Vector2 velocity,
                          float damage, float lifetime,
                          Color color, float radius,
                          bool isExplosive = false, float explosionRadius = 0f)
        {
            Position        = position;
            Velocity        = velocity;
            Damage          = damage;
            Lifetime        = lifetime;
            Color           = color;
            Radius          = radius;
            IsExplosive     = isExplosive;
            ExplosionRadius = explosionRadius;
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
