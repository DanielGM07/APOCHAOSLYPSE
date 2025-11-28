using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class RocketLauncher : FireWeapon
    {
        private const float RocketSpeed = 350f;
        private const float RocketLifetime = 2.5f;
        private const float RocketDamage = 50f;

        public float ExplosionRadius = 80f;

        public RocketLauncher(Texture2D texture, Rectangle srcRec, Rectangle destRect, Color color)
            : base(texture, srcRec, destRect, fireRate: 0.5f, knockback: 2000f, color)
        {
            ShakeMagnitude = 25f;
            ShakeDuration = 0.22f;
        }

        public override void Fire(Vector2 muzzle, Vector2 dir,
                                List<Projectile> projectiles,
                                Entity owner)
        {
            if (!CanFire) return;

            Vector2 rocketDir = dir;
            if (rocketDir != Vector2.Zero)
                rocketDir.Normalize();

            var rocket = new Projectile(
                position: muzzle,
                velocity: rocketDir * RocketSpeed,
                damage: RocketDamage,
                lifetime: RocketLifetime,
                color: Color.LightGreen,
                radius: 5f,
                isExplosive: true,
                explosionRadius: ExplosionRadius   // 80f en tu clase
            );

            projectiles.Add(rocket);

            // Knockback 2D medio
            if (Knockback > 0f && rocketDir != Vector2.Zero)
            {
                owner.ApplyKnockback(-rocketDir * Knockback);
            }

            // Shake al disparar
            if (ShakeMagnitude > 0f && ShakeDuration > 0f)
                Camera.Instance.Shake(ShakeMagnitude, ShakeDuration);

            ResetCooldown();
        }

    }
}
