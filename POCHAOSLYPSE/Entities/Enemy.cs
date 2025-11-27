using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class Enemy : Entity
    {
        private static readonly Random rng = new();

        public TileMap   TileMap   { get; set; }
        public EnemyKind Kind      { get; }
        public float     DetectionRadius { get; private set; }

        // Físicas simples
        private float MoveSpeed;
        private float Gravity      = 2000f;
        private float MaxFallSpeed = 1000f;
        private float JumpSpeed    = -650f;

        // Wander
        private float wanderTimer = 0f;
        private int   wanderDir   = 1;

        // Ataques
        private float meleeCooldown;
        private float meleeTimer;

        private float rangedCooldown;
        private float rangedTimer;

        public Rectangle? MeleeHitbox { get; private set; }
        private float meleeHitboxTimer = 0f;

        // ✅ Knockback horizontal acumulado (igual idea que en Player)
        private float knockbackX = 0f;

        public Enemy(Texture2D texture, Rectangle srcRec, Rectangle destRec, Color color, EnemyKind kind)
            : base(texture, srcRec, destRec, color)
        {
            Kind = kind;
            color = Color.IndianRed;
            ConfigureByType();
        }

        private void ConfigureByType()
        {
            switch (Kind)
            {
                case EnemyKind.Heavy:
                    Health          = 300;
                    MoveSpeed       = 80f;
                    DetectionRadius = 350f;
                    meleeCooldown   = 1.7f;
                    rangedCooldown  = 2.8f;
                    break;

                case EnemyKind.Medium:
                    Health          = 150;
                    MoveSpeed       = 220f;
                    DetectionRadius = 450f;
                    meleeCooldown   = 0.7f;
                    rangedCooldown  = 1.1f;
                    break;

                case EnemyKind.Light:
                    Health          = 40;
                    MoveSpeed       = 170f;
                    DetectionRadius = 600f;
                    meleeCooldown   = 0.0f;   // no melee real (solo "proximidad")
                    rangedCooldown  = 0.6f;
                    break;
            }
        }

        private void UpdateCommonTimers(float dt)
        {
            if (meleeTimer > 0f)  meleeTimer  -= dt;
            if (rangedTimer > 0f) rangedTimer -= dt;

            if (meleeHitboxTimer > 0f)
            {
                meleeHitboxTimer -= dt;
                if (meleeHitboxTimer <= 0f)
                    MeleeHitbox = null;
            }
        }

        private void ApplyGravity(float dt)
        {
            velocity.Y += Gravity * dt;
            if (velocity.Y > MaxFallSpeed)
                velocity.Y = MaxFallSpeed;
        }

        private void ApplyMovement(float dt)
        {
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
        }

        // ✅ Knockback estilo player: horizontal acumulado, vertical directo
        public override void ApplyKnockback(Vector2 impulse)
        {
            knockbackX += impulse.X;
            velocity.Y += impulse.Y;
        }

        public void UpdateAI(GameTime gameTime, Player player, TileMap tileMap, List<Projectile> enemyProjectiles)
        {
            TileMap = tileMap;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateCommonTimers(dt);

            Vector2 toPlayer = player.Center - this.Center;
            float   dist     = toPlayer.Length();

            bool playerInDetection = dist <= DetectionRadius;

            Vector2 desiredVel = Vector2.Zero;

            if (!playerInDetection)
            {
                // 🔹 Deambulando: izquierda/derecha random
                Wander(dt, ref desiredVel);
            }
            else
            {
                // 🔹 IA específica por tipo
                switch (Kind)
                {
                    case EnemyKind.Heavy:
                        HeavyAI(dt, player, toPlayer, dist, ref desiredVel, enemyProjectiles);
                        break;
                    case EnemyKind.Medium:
                        MediumAI(dt, player, toPlayer, dist, ref desiredVel, enemyProjectiles);
                        break;
                    case EnemyKind.Light:
                        LightAI(dt, player, toPlayer, dist, ref desiredVel, enemyProjectiles);
                        break;
                }
            }

            // ✅ velocidad horizontal = IA + knockback acumulado
            velocity.X = desiredVel.X + knockbackX;

            // ✅ fricción del knockback (igual idea que en Player)
            knockbackX = MathHelper.Lerp(knockbackX, 0f, 5f * dt);

            // Gravedad + movimiento
            ApplyGravity(dt);
            ApplyMovement(dt);
        }

        private void Wander(float dt, ref Vector2 desiredVel)
        {
            wanderTimer -= dt;
            if (wanderTimer <= 0f)
            {
                wanderTimer = 2f + (float)rng.NextDouble() * 2f; // 2–4 segundos
                int dir = rng.Next(-1, 2); // -1, 0, 1
                if (dir == 0) dir = 1;
                wanderDir = dir;
            }

            desiredVel.X = wanderDir * MoveSpeed * 0.5f;
        }

        private void HeavyAI(
            float dt,
            Player player,
            Vector2 toPlayer,
            float dist,
            ref Vector2 desiredVel,
            List<Projectile> enemyProjectiles)
        {
            // Siempre trata de acercarse horizontalmente si está lejos
            float horizontalDir = Math.Sign(toPlayer.X);
            if (Math.Abs(toPlayer.X) > 40f)
            {
                desiredVel.X = horizontalDir * MoveSpeed;
                isFacingLeft = desiredVel.X < 0;
            }

            // MELEE si muy cerca
            if (dist < 60f && meleeTimer <= 0f)
            {
                DoMeleeAttack(player, 80f, 1.0f, 35f);
            }
            // RANGED súper lento si está más lejos
            else if (dist >= 140f && rangedTimer <= 0f)
            {
                Vector2 dir = toPlayer;
                if (dir != Vector2.Zero) dir.Normalize();

                float speed   = 200f;
                float dmg     = 25f;
                float lifetime = 3.0f;

                var proj = new Projectile(
                    new(destinationRectangle.Center, new(10)),
                    velocity: dir * speed,
                    damage:   dmg,
                    lifetime: lifetime,
                    color:    Color.DarkRed
                );

                enemyProjectiles.Add(proj);
                rangedTimer = rangedCooldown;
            }
        }

        private void MediumAI(
            float dt,
            Player player,
            Vector2 toPlayer,
            float dist,
            ref Vector2 desiredVel,
            List<Projectile> enemyProjectiles)
        {
            // Perseguir al player horizontalmente
            float horizontalDir = Math.Sign(toPlayer.X);
            if (Math.Abs(toPlayer.X) > 10f)
            {
                desiredVel.X = horizontalDir * MoveSpeed;
                isFacingLeft = desiredVel.X < 0;
            }

            // Intentar saltar hacia plataformas cuando el player está arriba
            if (onGround &&
                player.Center.Y < this.Center.Y - 20f &&
                Math.Abs(toPlayer.X) < 90f)
            {
                velocity.Y = JumpSpeed;
                onGround = false;
            }

            // MELEE en corto rango
            if (dist < 55f && meleeTimer <= 0f)
            {
                DoMeleeAttack(player, 60f, 0.4f, 20f);
            }
            // RANGED si está lejos
            else if (dist >= 120f && rangedTimer <= 0f)
            {
                Vector2 dir = toPlayer;
                if (dir != Vector2.Zero) dir.Normalize();

                float speed    = 450f;
                float dmg      = 15f;
                float lifetime = 2.0f;

                var proj = new Projectile(
                    new(destinationRectangle.Center, new(5)),
                    velocity: dir * speed,
                    damage:   dmg,
                    lifetime: lifetime,
                    color:    Color.OrangeRed
                );

                enemyProjectiles.Add(proj);
                rangedTimer = rangedCooldown;
            }
        }

        private void LightAI(
            float dt,
            Player player,
            Vector2 toPlayer,
            float dist,
            ref Vector2 desiredVel,
            List<Projectile> enemyProjectiles)
        {
            float idealDistance = 280f;

            if (dist < idealDistance * 0.7f)
            {
                // Muy cerca → alejarse
                float dir = -Math.Sign(toPlayer.X);
                desiredVel.X = dir * MoveSpeed;
                isFacingLeft = desiredVel.X < 0;
            }
            else if (dist > idealDistance * 1.3f)
            {
                // Muy lejos → acercarse
                float dir = Math.Sign(toPlayer.X);
                desiredVel.X = dir * MoveSpeed;
                isFacingLeft = desiredVel.X < 0;
            }
            else
            {
                desiredVel.X = 0;
            }

            // Si estás demasiado cerca → daño brutal (casi one hit)
            if (dist < 50f)
            {
                player.Health -= 999;
            }

            // RANGED spam
            if (rangedTimer <= 0f)
            {
                Vector2 dir = toPlayer;
                if (dir != Vector2.Zero) dir.Normalize();

                float speed    = 520f;
                float dmg      = 32f;
                float lifetime = 2.0f;

                var proj = new Projectile(
                    new(destinationRectangle.Center, new(4)),
                    velocity: dir * speed,
                    damage:   dmg,
                    lifetime: lifetime,
                    color:    Color.Cyan
                );

                enemyProjectiles.Add(proj);
                rangedTimer = rangedCooldown;
            }
        }

        private void DoMeleeAttack(Player player, float rangeX, float displayTime, float damage)
        {
            int dir = isFacingLeft ? -1 : 1;

            Rectangle hitbox = new Rectangle(
                destinationRectangle.Center.X + (int)(dir * rangeX / 2f),
                destinationRectangle.Top,
                (int)rangeX,
                destinationRectangle.Height
            );

            MeleeHitbox       = hitbox;
            meleeHitboxTimer  = displayTime;
            meleeTimer        = meleeCooldown;

            if (hitbox.Intersects(player.destinationRectangle))
            {
                player.Health -= (int)damage;
            }
        }
    }
}
