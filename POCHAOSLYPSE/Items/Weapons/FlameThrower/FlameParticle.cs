using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class FlameParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;

        public float Lifetime;     // tiempo restante
        public float MaxLifetime;  // tiempo total
        public float Radius;
        public float DamagePerSecond;

        public bool IsAlive => Lifetime > 0f;

        public FlameParticle(Vector2 position, Vector2 velocity,
                             float lifetime, float radius, float dps)
        {
            Position       = position;
            Velocity       = velocity;
            Lifetime       = lifetime;
            MaxLifetime    = lifetime;
            Radius         = radius;
            DamagePerSecond = dps;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Movimiento
            Position += Velocity * dt;

            // Drag: la velocidad va cayendo progresivamente
            const float drag = 3.0f;
            Velocity -= Velocity * drag * dt;

            // Pequeño "swirl" aleatorio opcional podrías meter acá si quisieras

            Lifetime -= dt;
        }

        public Rectangle BoundingBox
        {
            get
            {
                int size = (int)(Radius * 2f);
                return new Rectangle(
                    (int)(Position.X - Radius),
                    (int)(Position.Y - Radius),
                    size,
                    size
                );
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (!IsAlive) return;

            // Progreso de vida 0..1
            float t = 1f - (Lifetime / MaxLifetime);
            t = MathHelper.Clamp(t, 0f, 1f);

            // Color transición: amarillo -> naranja -> rojo
            Color start = Color.Yellow;
            Color mid   = Color.Orange;
            Color end   = Color.DarkRed;

            Color c;
            if (t < 0.5f)
            {
                float tt = t / 0.5f;
                c = Color.Lerp(start, mid, tt);
            }
            else
            {
                float tt = (t - 0.5f) / 0.5f;
                c = Color.Lerp(mid, end, tt);
            }

            // Fade-out suave
            float alpha = Lifetime / MaxLifetime;
            alpha = MathHelper.Clamp(alpha, 0f, 1f);
            c *= alpha;

            var rect = BoundingBox;
            spriteBatch.Draw(pixel, rect, c);
        }

        // 🧯 FUTURO: cuando tengas enemies en la escena, podés hacer algo así:
        //
        // public void ApplyDamage(IEnumerable<Enemy> enemies, float dt)
        // {
        //     foreach (var e in enemies)
        //     {
        //         if (e.destinationRectangle.Intersects(this.BoundingBox))
        //         {
        //             e.Health -= DamagePerSecond * dt;
        //         }
        //     }
        // }
    }
}
