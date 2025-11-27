using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE
{
    public class Player : Entity
    {
        // -------------------------
        // MOVIMIENTO BASE (TUYO)
        // -------------------------
        public float MoveSpeed    = 250f;
        public float Gravity      = 2000f;
        public float JumpSpeed    = -700f;
        public float MaxFallSpeed = 1000f;

        // Coyote time
        private float coyoteTime = 0.12f;
        private float coyoteTimeCounter = 0f;

        // Jump buffer
        private float jumpBufferTime = 0.12f;
        private float jumpBufferCounter = 0f;

        // Helper para cámara
        public Rectangle BoundingBox => destinationRectangle;
        public Vector2   Velocity    => velocity;
        public bool      FacingLeft  => isFacingLeft;

        // Knockback horizontal acumulado
        private float knockbackX = 0f;

        private KeyboardState prevState;

        // ---------------------------------------------------------
        private Vector2 preDashVelocity;  // velocidad justo antes del dash
        private float dashForce = 1000f;  // fuerza del dash (no velocidad directa)

        public TileMap TileMap { get; set; }
        public Weapon weapon;

        // --------------------------------
        // 🟦 DOBLE SALTO
        // --------------------------------
        private int maxJumps = 2;
        private int jumpsUsed = 0;

        // --------------------------------
        // 🟪 DASH OMNIDIRECCIONAL
        // --------------------------------
        private bool  isDashing = false;
        private float dashSpeed = 900f;
        private float dashDuration = 0.55f;
        private float dashTimer = 0f;

        private float dashCooldown = 0.35f;
        private float dashCooldownTimer = 0f;

        private Vector2 dashDirection = Vector2.Zero;

        // --------------------------------
        // 🟥 DIVE SLAM
        // --------------------------------
        private bool isDiveSlam = false;
        private float diveSpeed = 1800f;
        private float diveAccumulatedTime = 0f;
        private float diveMaxMultiplier = 3f;

        // daño que hará el slam
        public float SlamDamage => 20f * MathHelper.Clamp(diveAccumulatedTime * 6f, 1f, diveMaxMultiplier);


        public Player(Texture2D texture, Rectangle srcRec, Rectangle destRec, Color color)
            : base(texture, srcRec, destRec, color)
        {
        }


        // -----------------------------
        //   KNOCKBACK REAL
        // -----------------------------
        public override void ApplyKnockback(Vector2 impulse)
        {
            knockbackX += impulse.X;  // horizontal se acumula
            velocity.Y += impulse.Y;  // vertical instantáneo
        }


        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var ks = Keyboard.GetState();

            // ---------------------------
            // INPUTS DE SALTO
            // ---------------------------
            bool jumpHeld =
                ks.IsKeyDown(Keys.Space) ||
                ks.IsKeyDown(Keys.W) ||
                ks.IsKeyDown(Keys.Up);

            bool jumpPressedThisFrame =
                jumpHeld &&
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
            {
                coyoteTimeCounter = coyoteTime;
                jumpsUsed = 0; // reset doble salto
            }
            else
                coyoteTimeCounter -= dt;
// ------------------------------------
// 🟪 DASH — con INERCIA real
// ------------------------------------
// -------------------------------
// 🟪 DASH — solo se recarga en el PISO
// -------------------------------
            if (!isDashing)
            {
                // 💡 El dash se recarga SOLO estando en el piso
                if (onGround)
                {
                    if (dashCooldownTimer > 0f)
                        dashCooldownTimer -= dt;
                }

                // Intentar iniciar el dash SOLO si está listo
                if ((ks.IsKeyDown(Keys.LeftControl)) && dashCooldownTimer <= 0f)
                {
                    isDashing = true;
                    dashTimer = dashDuration;

                    // Reiniciar cooldown SOLO cuando empieza el dash
                    dashCooldownTimer = dashCooldown;

                    // Guardamos velocidad previa
                    preDashVelocity = velocity;

                    // Dirección del dash
                    dashDirection = Vector2.Zero;

                    if (ks.IsKeyDown(Keys.W) || ks.IsKeyDown(Keys.Up))         dashDirection.Y = -1;
                    if (ks.IsKeyDown(Keys.S) || ks.IsKeyDown(Keys.Down))       dashDirection.Y = 1;
                    if (ks.IsKeyDown(Keys.A) || ks.IsKeyDown(Keys.Left))       dashDirection.X = -1;
                    if (ks.IsKeyDown(Keys.D) || ks.IsKeyDown(Keys.Right))      dashDirection.X = 1;

                    if (dashDirection == Vector2.Zero)
                        dashDirection = isFacingLeft ? new Vector2(-1, 0) : new Vector2(1, 0);

                    dashDirection.Normalize();
                }
            }
            else
            {
                // ---- CURVA DE FUERZA ----
                float t = 1f - (dashTimer / dashDuration);
                float curve = (float)System.Math.Sin(System.Math.PI * t);    // ease-in → peak → ease-out

                // Calculamos fuerza como aceleración
                Vector2 dashAccel = dashDirection * (dashForce * curve);

                // Aplicamos aceleración a la velocidad previa
                velocity = preDashVelocity + dashAccel;

                dashTimer -= dt;

                if (dashTimer <= 0f)
                {
                    isDashing = false;

                    // NO reseteamos velocidad.
                    // Dejamos la velocidad como quedó, permitiendo inercia natural.
                }

                // ---- Movimiento + colisiones ----
                destinationRectangle.X += (int)(velocity.X * dt);
                TileMap?.CheckCollisionHorizontal(this);

                destinationRectangle.Y += (int)(velocity.Y * dt);
                onGround = false;
                TileMap?.CheckCollisionVertical(this);

                prevState = ks;
                return;
            }

            // ------------------------------------
            // 🟥 DIVE SLAM
            // ------------------------------------
            bool divePressed = ks.IsKeyDown(Keys.S) || ks.IsKeyDown(Keys.Down);

            if (!onGround && !isDashing)
            {
                if (divePressed)
                {
                    if (!isDiveSlam)
                    {
                        isDiveSlam = true;
                        diveAccumulatedTime = 0f;
                    }

                    diveAccumulatedTime += dt;
                    velocity.Y = diveSpeed * MathHelper.Clamp(1f + diveAccumulatedTime, 1f, diveMaxMultiplier);
                }
            }

            // ------------------------------------
            // Al aterrizar después del slam → daño
            // ------------------------------------
            if (onGround && isDiveSlam)
            {
                isDiveSlam = false;

                // ⚡ HACER DAÑO EN ÁREA
                float dmg = SlamDamage;
                float radius = 120f;

                // TODO: A futuro → aplicar daño a enemigos cercanos
                // por ahora solo debug:
                System.Diagnostics.Debug.WriteLine($"SLAM DAMAGE: {dmg}");
            }


            // ------------------------------------
            // MOVIMIENTO HORIZONTAL
            // ------------------------------------
            float inputVelX = 0f;

            if (ks.IsKeyDown(Keys.A) || ks.IsKeyDown(Keys.Left))
                inputVelX -= MoveSpeed;
            if (ks.IsKeyDown(Keys.D) || ks.IsKeyDown(Keys.Right))
                inputVelX += MoveSpeed;

            if (inputVelX < 0) isFacingLeft = true;
            else if (inputVelX > 0) isFacingLeft = false;

            velocity.X = inputVelX + knockbackX;

            // ------------------------------------
            // GRAVEDAD
            // ------------------------------------
            if (!isDiveSlam)
            {
                velocity.Y += Gravity * dt;
                if (velocity.Y > MaxFallSpeed)
                    velocity.Y = MaxFallSpeed;
            }

            // ------------------------------------
            // SALTO (con doble salto)
            // ------------------------------------
            if (jumpBufferCounter > 0f)
            {
                // salto normal o coyote
                if (coyoteTimeCounter > 0f && jumpsUsed == 0)
                {
                    velocity.Y = JumpSpeed;
                    onGround = false;
                    jumpsUsed = 1;
                    jumpBufferCounter = 0;
                }
                // doble salto
                else if (!onGround && jumpsUsed < maxJumps)
                {
                    velocity.Y = JumpSpeed;
                    jumpsUsed++;
                    jumpBufferCounter = 0;
                }
            }

            // ------------------------------------
            // ACTUALIZAR MOVIMIENTO Y COLISIONES
            // ------------------------------------
            if (TileMap != null)
            {
                destinationRectangle.X += (int)(velocity.X * dt);
                TileMap.CheckCollisionHorizontal(this);

                destinationRectangle.Y += (int)(velocity.Y * dt);
                onGround = false;
                TileMap.CheckCollisionVertical(this);
            }
            else
            {
                destinationRectangle.X += (int)(velocity.X * dt);
                destinationRectangle.Y += (int)(velocity.Y * dt);
            }

            // ------------------------------------
            // ATENUAR KNOCKBACK
            // ------------------------------------
            knockbackX = MathHelper.Lerp(knockbackX, 0f, 5f * dt);

            prevState = ks;
            base.Update(gameTime);
        }
    }
}
