using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class AK47 : FireWeapon
    {
        private const float BulletSpeed    = 600f;
        private const float BulletLifetime = 1.5f;
        private const float DamagePerBullet = 10f;
        private static readonly float SpreadAngle = MathHelper.ToRadians(4f); // ±2°

        private static readonly Random rng = new();

        public AK47(Texture2D texture, Rectangle srcRec, Rectangle destRect, Color color)
            : base(texture, srcRec, destRect, fireRate: 10f, knockback: 150f, color)
        {
            // 🔹 Shake independiente del knockback
            ShakeMagnitude = 6f;
            ShakeDuration  = 0.07f;
        }

        public override void Fire(Vector2 muzzle, Vector2 dir,
                                  List<Projectile> projectiles,
                                  Entity owner)
        {
            if (!CanFire) return;

            float baseAngle = (float)Math.Atan2(dir.Y, dir.X);
            float offset    = ((float)rng.NextDouble() - 0.5f) * SpreadAngle;
            float angle     = baseAngle + offset;

            Vector2 finalDir = new((float)Math.Cos(angle), (float)Math.Sin(angle));
            if (finalDir != Vector2.Zero)
                finalDir.Normalize();

            var proj = new Projectile(
                position: muzzle,
                velocity: finalDir * BulletSpeed,
                damage:   DamagePerBullet,
                lifetime: BulletLifetime,
                color:    Color.Yellow,
                radius:   3f
            );

            projectiles.Add(proj);

            // 🔹 Knockback 2D + shake separado
            if (Knockback > 0f && dir != Vector2.Zero)
            {
                Vector2 kbDir = dir;
                kbDir.Normalize();
                owner.ApplyKnockback(-kbDir * Knockback);
            }

            if (ShakeMagnitude > 0f && ShakeDuration > 0f)
                Camera.Instance.Shake(ShakeMagnitude, ShakeDuration);

            ResetCooldown();
        }
    }
}
