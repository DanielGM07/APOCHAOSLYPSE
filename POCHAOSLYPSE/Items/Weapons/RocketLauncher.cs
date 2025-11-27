using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class RocketLauncher : FireWeapon
    {
        private const float RocketSpeed = 350f;
        private const float RocketLifetime = 2.5f;
        private const float RocketDamage = 40f;

        // Más adelante ExplosionRadius se puede usar cuando tengas colisiones
        public float ExplosionRadius = 80f;

        public RocketLauncher(Texture2D texture, Rectangle srcRec, Rectangle destRect, Color color)
            : base(texture, srcRec, destRect, fireRate: 0.5f, knockback: 20f, color) // knockback medio
        {
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
                radius: 5f
            );

            projectiles.Add(rocket);

            // Knockback al disparar
            owner.Center -= rocketDir * Knockback;

            ResetCooldown();
        }
    }
}
