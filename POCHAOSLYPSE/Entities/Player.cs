using System;
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
        public Weapon  weapon;

        // 🟦 DOBLE SALTO
        private int maxJumps = 2;
        private int jumpsUsed = 0;

        // 🟪 DASH OMNIDIRECCIONAL
        private bool  isDashing      = false;
        private float dashSpeed      = 900f;
        private float dashDuration   = 0.55f;
        private float dashTimer      = 0f;
        private float dashCooldown   = 0.35f;
        private float dashCooldownTimer = 0f;
        private Vector2 dashDirection = Vector2.Zero;

        // 🟥 DIVE SLAM
        private bool  isDiveSlam          = false;
        private float diveSpeed           = 1200f;
        private float diveAccumulatedTime = 0f;
        private float diveMaxMultiplier   = 3f;

        // Daño base del slam (según cuánto tiempo estuviste cayendo)
        public float SlamDamage => 20f * MathHelper.Clamp(diveAccumulatedTime * 6f, 1f, diveMaxMultiplier);

        // Radio del slam
        public float SlamRadius => 120f;

        // Info del último impacto de slam (para que la escena aplique daño)
        public bool   SlamJustLanded { get; private set; }   // true sólo el frame en que impacta
        public Vector2 SlamCenter    { get; private set; }   // centro del impacto
        public float  LastSlamDamage { get; private set; }   // daño calculado en ese impacto

        public Player(Texture2D texture, Rectangle srcRec, Rectangle destRec, Color color)
            : base(texture, srcRec, destRec, color)
        {
            Health = 100;
        }

        public void Strafe() { }

        // KNOCKBACK REAL
        public override void ApplyKnockback(Vector2 impulse)
        {
            knockbackX += impulse.X;
            velocity.Y += impulse.Y;
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var ks = Keyboard.GetState();

            // 🔹 resetear flag de slam cada frame
            SlamJustLanded = false;

            // INPUT SALTO
            bool jumpHeld =
                ks.IsKeyDown(Keys.Space) ||
                ks.IsKeyDown(Keys.W) ||
                ks.IsKeyDown(Keys.Up);

            bool jumpPressedThisFrame =
                jumpHeld &&
                !(prevState.IsKeyDown(Keys.Space) ||
                  prevState.IsKeyDown(Keys.W) ||
                  prevState.IsKeyDown(Keys.Up));

            if (jumpPressedThisFrame)
                jumpBufferCounter = jumpBufferTime;
            else
                jumpBufferCounter -= dt;

            if (onGround)
            {
                coyoteTimeCounter = coyoteTime;
                jumpsUsed = 0;
            }
            else
                coyoteTimeCounter -= dt;

            // DASH (con cooldown solo en el piso)
            if (!isDashing)
            {
                if (onGround)
                {
                    if (dashCooldownTimer > 0f)
                        dashCooldownTimer -= dt;
                }

                if (ks.IsKeyDown(Keys.LeftControl) && dashCooldownTimer <= 0f)
                {
                    isDashing = true;
                    dashTimer = dashDuration;
                    dashCooldownTimer = dashCooldown;

                    preDashVelocity = velocity;

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
                float t = 1f - (dashTimer / dashDuration);
                float curve = (float)System.Math.Sin(System.Math.PI * t);

                Vector2 dashAccel = dashDirection * (dashForce * curve);

                velocity = preDashVelocity + dashAccel;

                dashTimer -= dt;

                if (dashTimer <= 0f)
                {
                    isDashing = false;
                }

                destinationRectangle.X += (int)(velocity.X * dt);
                TileMap?.CheckCollisionHorizontal(this);

                destinationRectangle.Y += (int)(velocity.Y * dt);
                onGround = false;
                TileMap?.CheckCollisionVertical(this);

                prevState = ks;
                return;
            }

            // DIVE SLAM
            bool diveHeld = ks.IsKeyDown(Keys.S) || ks.IsKeyDown(Keys.Down);
            bool divePressedThisFrame =
                diveHeld &&
                !(prevState.IsKeyDown(Keys.S) || prevState.IsKeyDown(Keys.Down));

            // ✅ solo se puede INICIAR el slam en el AIRE y cuando estás cayendo (velocity.Y >= 0)
            if (!onGround && !isDashing && velocity.Y >= 0f)
            {
                if (divePressedThisFrame && !isDiveSlam)
                {
                    isDiveSlam = true;
                    diveAccumulatedTime = 0f;
                }

                if (isDiveSlam)
                {
                    diveAccumulatedTime += dt;
                    velocity.Y = diveSpeed * MathHelper.Clamp(1f + diveAccumulatedTime, 1f, diveMaxMultiplier);
                }
            }

            // 🔹 cuando tocas el piso después de un dive slam
            if (onGround && isDiveSlam)
            {
                isDiveSlam = false;

                float dmg    = SlamDamage;
                float radius = SlamRadius;

                // Guardamos info para que la escena aplique el daño en área
                SlamJustLanded = true;
                LastSlamDamage = dmg;
                SlamCenter     = this.Center;

                System.Diagnostics.Debug.WriteLine($"SLAM DAMAGE: {dmg}");

                // Reseteamos acumulador para futuros slams
                diveAccumulatedTime = 0f;
            }

            // MOVIMIENTO HORIZONTAL
            float inputVelX = 0f;

            if (ks.IsKeyDown(Keys.A) || ks.IsKeyDown(Keys.Left))
                inputVelX -= MoveSpeed;
            if (ks.IsKeyDown(Keys.D) || ks.IsKeyDown(Keys.Right))
                inputVelX += MoveSpeed;

            if (inputVelX < 0) isFacingLeft = true;
            else if (inputVelX > 0) isFacingLeft = false;

            velocity.X = inputVelX + knockbackX;

            // GRAVEDAD
            if (!isDiveSlam)
            {
                velocity.Y += Gravity * dt;
                if (velocity.Y > MaxFallSpeed)
                    velocity.Y = MaxFallSpeed;
            }

            // SALTO (doble salto)
            if (jumpBufferCounter > 0f)
            {
                if (coyoteTimeCounter > 0f && jumpsUsed == 0)
                {
                    velocity.Y = JumpSpeed;
                    onGround = false;
                    jumpsUsed = 1;
                    jumpBufferCounter = 0;
                }
                else if (!onGround && jumpsUsed < maxJumps)
                {
                    velocity.Y = JumpSpeed;
                    jumpsUsed++;
                    jumpBufferCounter = 0;
                }
            }

            // MOVIMIENTO + COLISIONES
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

            knockbackX = MathHelper.Lerp(knockbackX, 0f, 5f * dt);

            prevState = ks;
            FollowPlayer(gameTime, this);
            base.Update(gameTime);
        }

        //this is what classes are for bitch
        private void FollowPlayer(GameTime gameTime, Player player)
        {
            Random rng = new();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update del shake
            if (Camera.Instance.shakeTimer > 0f)
            {
                Camera.Instance.shakeTimer -= dt;
                float t = Math.Max(Camera.Instance.shakeTimer / Camera.Instance.shakeDuration, 0f);
                float currentMag = Camera.Instance.shakeMagnitude * t;

                float dx = (float)(rng.NextDouble() * 2 - 1);
                float dy = (float)(rng.NextDouble() * 2 - 1);
                Vector2 dir = new(dx, dy);
                if (dir != Vector2.Zero)
                    dir.Normalize();
                Camera.Instance.shakeOffset = dir * currentMag;
                Camera.Instance.X +=0.1f;
            }
            else
            {
                if (Camera.Instance.shakeOffset != Vector2.Zero)
                {
                    Camera.Instance.shakeOffset = Vector2.Zero;
                    Camera.Instance.X +=0.1f;
                }
            }

            var bounds       = player.BoundingBox;
            var playerCenter = new Vector2(bounds.Center.X, bounds.Center.Y);

            var lookAhead = player.FacingLeft ? -200f : 200f;

            if (Math.Abs(player.Velocity.X) > 20f)
            {
                lookAhead = player.FacingLeft ? -600f : 600f;
            }

            var desiredFocus = new Vector2(playerCenter.X + lookAhead, playerCenter.Y);

            if (!Camera.Instance.followInitialized)
            {
                Camera.Instance.followHorizontal  = desiredFocus.X;
                Camera.Instance.followVertical    = desiredFocus.Y;
                Camera.Instance.followInitialized = true;
            }

            var lerpEase = 0.5f * dt * 60f;
            Camera.Instance.followHorizontal = MathHelper.Lerp(
                Camera.Instance.followHorizontal, desiredFocus.X, lerpEase);
            Camera.Instance.followVertical   = MathHelper.Lerp(Camera.Instance.followVertical, desiredFocus.Y, lerpEase);

            var desiredPosition = new Vector2(Camera.Instance.followHorizontal, Camera.Instance.followVertical);
            Camera.Instance.Position = Vector2.Lerp(Camera.Instance.Position, desiredPosition, 0.05f * dt * 60f);
        }
    }
}
