using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE
{
    public class Player : Entity
    {
        // Velocidades en unidades por segundo
        public float MoveSpeed      = 250f;   // velocidad horizontal
        public float Gravity        = 2000f;  // aceleración hacia abajo
        public float JumpSpeed      = -700f;  // velocidad inicial hacia arriba
        public float MaxFallSpeed   = 1000f;  // límite de caída

        // Coyote time
        private float coyoteTime        = 0.12f;
        private float coyoteTimeCounter = 0f;

        // Jump buffer
        private float jumpBufferTime    = 0.12f;
        private float jumpBufferCounter = 0f;

        public Weapon weapon;
        public TileMap TileMap { get; set; }

        private KeyboardState prevState;

        // Helpers para la cámara
        public Rectangle BoundingBox => destinationRectangle;
        public Vector2   Velocity    => velocity;
        public bool      FacingLeft  => isFacingLeft;

        // 🔹 Knockback horizontal acumulado (para recoil)
        private float knockbackX = 0f;

        public Player(Texture2D texture, Rectangle srcRec, Rectangle destRec, Color color)
            : base(texture, srcRec, destRec, color)
        {
        }

        public void Strafe() { }

        // 🔹 Knockback 2D: X acumulada, Y directo a la velocidad
        public override void ApplyKnockback(Vector2 impulse)
        {
            knockbackX += impulse.X;   // empuje horizontal sostenido
            velocity.Y += impulse.Y;   // impulso vertical inmediato
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState ks = Keyboard.GetState();

            // --- INPUT DE SALTO (W, UP, SPACE) ---
            bool jumpNow =
                ks.IsKeyDown(Keys.Space) ||
                ks.IsKeyDown(Keys.W) ||
                ks.IsKeyDown(Keys.Up);

            bool jumpPressedThisFrame =
                jumpNow &&
                !(prevState.IsKeyDown(Keys.Space) ||
                  prevState.IsKeyDown(Keys.W) ||
                  prevState.IsKeyDown(Keys.Up));

            // Jump buffer
            if (jumpPressedThisFrame)
                jumpBufferCounter = jumpBufferTime;
            else
                jumpBufferCounter -= dt;

            // Coyote time
            if (onGround)
                coyoteTimeCounter = coyoteTime;
            else
                coyoteTimeCounter -= dt;

            // --- MOVIMIENTO HORIZONTAL POR INPUT ---
            float inputVelX = 0f;
            if (ks.IsKeyDown(Keys.A) || ks.IsKeyDown(Keys.Left))
                inputVelX -= MoveSpeed;
            if (ks.IsKeyDown(Keys.D) || ks.IsKeyDown(Keys.Right))
                inputVelX += MoveSpeed;

            if (inputVelX < 0) isFacingLeft = true;
            else if (inputVelX > 0) isFacingLeft = false;

            // 🔹 velocity.X = input + knockback
            velocity.X = inputVelX + knockbackX;

            // --- GRAVEDAD ---
            velocity.Y += Gravity * dt;
            if (velocity.Y > MaxFallSpeed)
                velocity.Y = MaxFallSpeed;

            // --- SALTO ---
            if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
            {
                velocity.Y = JumpSpeed;
                onGround = false;
                jumpBufferCounter = 0f;
                coyoteTimeCounter = 0f;
            }

            // --- MOVIMIENTO + COLISIONES ---
            if (TileMap != null)
            {
                // X
                destinationRectangle.X += (int)(velocity.X * dt);
                TileMap.CheckCollisionHorizontal(this);

                // Y
                destinationRectangle.Y += (int)(velocity.Y * dt);
                onGround = false;
                TileMap.CheckCollisionVertical(this);
            }
            else
            {
                destinationRectangle.X += (int)(velocity.X * dt);
                destinationRectangle.Y += (int)(velocity.Y * dt);
            }

            // 🔹 Atenuar knockback horizontal con el tiempo
            knockbackX = MathHelper.Lerp(knockbackX, 0f, 5f * dt);

            prevState = ks;

            base.Update(gameTime);
        }
    }
}
