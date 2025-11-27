using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class GatlingGun : FireWeapon
    {
        // Velocidad de las balas
        private const float BulletSpeed    = 850f;
        private const float BulletLifetime = 1.5f;

        // Daño bajito por bala
        private const float DamagePerBullet = 3f;

        // Muchísima dispersión: cono grande
        private static readonly float SpreadAngle = MathHelper.ToRadians(60f); // ~30°

        private static readonly Random rng = new();

        public GatlingGun(Texture2D texture, Rectangle srcRec, Rectangle destRect, Color color)
            // fireRate: 25 disparos por segundo, knockback por bala chico pero acumulable
            : base(texture, srcRec, destRect, fireRate: 30f, knockback: 100f, color)
        {
            // Shake moderado por tiro (se siente, pero no tanto como la escopeta)
            ShakeMagnitude = 5f;
            ShakeDuration  = 0.05f;
        }

        public override void Fire(Vector2 muzzle, Vector2 dir,
                                  List<Projectile> projectiles,
                                  Entity owner)
        {
            if (!CanFire) return;

            if (dir == Vector2.Zero)
                dir = Vector2.UnitX;

            // Ángulo base del disparo (dirección hacia el mouse)
            float baseAngle = (float)Math.Atan2(dir.Y, dir.X);

            // Offset aleatorio dentro de un cono grande
            float offset = ((float)rng.NextDouble() - 0.5f) * SpreadAngle;
            float angle  = baseAngle + offset;

            Vector2 finalDir = new((float)Math.Cos(angle), (float)Math.Sin(angle));
            if (finalDir != Vector2.Zero)
                finalDir.Normalize();

            // Crear proyectil (no explosivo)
            var proj = new Projectile(
                new(muzzle.ToPoint(), new(3)),
                velocity: finalDir * BulletSpeed,
                damage:   DamagePerBullet,
                lifetime: BulletLifetime,
                color:    Color.LightGray
            );

            projectiles.Add(proj);

            // 🔹 Knockback 2D leve pero MUY frecuente
            if (Knockback > 0f)
            {
                Vector2 kbDir = dir;
                if (kbDir != Vector2.Zero)
                {
                    kbDir.Normalize();
                    owner.ApplyKnockback(-kbDir * Knockback);
                }
            }

            // 🔹 Shake de cámara suave por tiro
            if (ShakeMagnitude > 0f && ShakeDuration > 0f)
                Camera.Instance.Shake(ShakeMagnitude, ShakeDuration);

            ResetCooldown();
        }
    }
}
