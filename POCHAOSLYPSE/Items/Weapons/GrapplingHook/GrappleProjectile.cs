using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class GrappleProjectile
    {
        public enum HookState
        {
            Flying,
            Latched,
            Dead
        }

        public Vector2 Position;
        public Vector2 Velocity;

        public float Radius;
        public float MaxDistance;

        public HookState State { get; private set; } = HookState.Flying;

        private float traveledDistance;

        public Vector2 HookPoint { get; private set; }

        public bool IsAlive => State != HookState.Dead;
        public bool IsLatched => State == HookState.Latched;

        public GrappleProjectile(Vector2 startPos, Vector2 velocity, float maxDistance, float radius = 4f)
        {
            Position      = startPos;
            Velocity      = velocity;
            MaxDistance   = maxDistance;
            Radius        = radius;
            traveledDistance = 0f;
        }

        public void Update(GameTime gameTime, TileMap tileMap)
        {
            if (!IsAlive)
                return;

            if (State == HookState.Flying)
            {
                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

                Vector2 oldPos = Position;
                Position += Velocity * dt;

                traveledDistance += Vector2.Distance(oldPos, Position);
                if (traveledDistance >= MaxDistance)
                {
                    State = HookState.Dead;
                    return;
                }

                // Chequear colisión con bloques sólidos
                Rectangle hookRect = new Rectangle(
                    (int)(Position.X - Radius),
                    (int)(Position.Y - Radius),
                    (int)(Radius * 2),
                    (int)(Radius * 2)
                );

                foreach (var block in tileMap.blocks)
                {
                    // Solo enganchamos en bloques sólidos
                    if (block is Collisionblock && block.collider.Intersects(hookRect))
                    {
                        State     = HookState.Latched;
                        HookPoint = Position;
                        Velocity  = Vector2.Zero;
                        break;
                    }
                }
            }
        }

        public void ForceKill()
        {
            State = HookState.Dead;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel, Vector2 playerCenter)
        {
            if (!IsAlive)
                return;

            // Dibujar cuerda: línea entre playerCenter y Position
            Vector2 delta  = Position - playerCenter;
            float length   = delta.Length();
            if (length > 0.1f)
            {
                float angle = (float)System.Math.Atan2(delta.Y, delta.X);

                // Usamos el pixel como línea estirada
                spriteBatch.Draw(
                    pixel,
                    playerCenter,
                    null,
                    Color.Gray,
                    angle,
                    new Vector2(0f, 0.5f),          // origen en el extremo "player"
                    new Vector2(length, 2f),        // largo y grosor de la cuerda
                    SpriteEffects.None,
                    0f
                );
            }

            // Dibujar la "punta" del gancho como un cuadradito
            var rect = new Rectangle(
                (int)(Position.X - Radius),
                (int)(Position.Y - Radius),
                (int)(Radius * 2),
                (int)(Radius * 2)
            );

            spriteBatch.Draw(pixel, rect, Color.Cyan);
        }
    }
}
