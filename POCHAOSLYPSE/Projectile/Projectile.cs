using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class Projectile //TODO: make this a Sprite child
    {
        public Rectangle recPosition;
        public Vector2 Velocity;
        public float Damage;
        public float Lifetime;
        public Color Color;
        public bool IsAlive => Lifetime > 0f;

        // 🔹 Nuevo: explosivo o no
        public bool IsExplosive;
        public float ExplosionRadius;

        public Projectile(Rectangle position,
                          Vector2 velocity,
                          float damage,
                          float lifetime,
                          Color color,
                          bool isExplosive = false,
                          float explosionRadius = 0f)
        {
            recPosition = position;
            Velocity        = velocity;
            Damage          = damage;
            Lifetime        = lifetime;
            Color           = color;
            IsExplosive     = isExplosive;
            ExplosionRadius = explosionRadius;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            recPosition.X += (int)(Velocity.X * dt);
            recPosition.Y += (int)(Velocity.Y * dt);
            Lifetime -= dt;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
          spriteBatch.Draw(pixel, recPosition, Color);
        }
    }
}
