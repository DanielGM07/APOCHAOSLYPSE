using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class FlamethrowerWeapon : Weapon
    {
        private static readonly Random rng = new();

        // Config de partículas
        private const int ParticlesPerShot = 18;       // cuántas partículas por "tick" de fuego
        private const float FlameSpeedMin = 200f;
        private const float FlameSpeedMax = 1000f;
        private const float FlameLifetime = 1.8f;    // en segundos
        private const float FlameRadius = 8f;
        private const float FlameDamagePerSec = 1.5f;      // daño por segundo por partícula

        // Spread (cono de fuego)
        private static readonly float SpreadAngle = MathHelper.ToRadians(40f); // bastante abierto

        public FlamethrowerWeapon(Texture2D texture, Rectangle srcRec, Rectangle destRect, Color color)
            // FireRate muy alto, spam de partículas
            : base(texture, srcRec, destRect, fireRate: 40f, knockback: 0f, color)
        {
            // Shake suave pero constante mientras disparás
            ShakeMagnitude = 4f;
            ShakeDuration = 0.03f;
        }

        // 🔹 No usamos el sistema estándar de proyectiles
        public override void Fire(Vector2 muzzle, Vector2 dir,
                                  List<Projectile> projectiles,
                                  Entity owner)
        {
            // vacío intencionalmente: el lanzallamas se maneja con EmitFlames()
        }

        public void EmitFlames(Vector2 origin, Vector2 dir,
                               List<FlameParticle> flames,
                               Entity owner)
        {
            if (!CanFire)
                return;

            if (dir == Vector2.Zero)
                dir = Vector2.UnitX;

            dir.Normalize();

            for (int i = 0; i < ParticlesPerShot; i++)
            {
                // Ángulo base del disparo
                float baseAngle = (float)Math.Atan2(dir.Y, dir.X);

                // Offset aleatorio dentro del cono
                float offset = ((float)rng.NextDouble() - 0.5f) * SpreadAngle;
                float angle = baseAngle + offset;

                // Dirección final
                Vector2 flameDir = new((float)Math.Cos(angle), (float)Math.Sin(angle));
                if (flameDir != Vector2.Zero)
                    flameDir.Normalize();

                // Velocidad aleatoria
                float speed = MathHelper.Lerp(FlameSpeedMin, FlameSpeedMax, (float)rng.NextDouble());

                var flame = new FlameParticle(
                    position: origin,
                    velocity: flameDir * speed,
                    lifetime: FlameLifetime,
                    radius: FlameRadius,
                    dps: FlameDamagePerSec
                );

                flames.Add(flame);
            }

            // Shake constante suave mientras está disparando
            if (ShakeMagnitude > 0f && ShakeDuration > 0f)
                Camera.Instance.Shake(ShakeMagnitude, ShakeDuration);

            ResetCooldown();
        }
    }
}
