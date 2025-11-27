using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class Shotgun : FireWeapon
    {
        private const int PelletCount = 7;
        private static readonly float ConeAngle = MathHelper.ToRadians(40f); // cono fijo
        private const float BulletSpeed = 500f;
        private const float BulletLifetime = 0.8f;
        private const float DamagePerPellet = 6f;

        public Shotgun(Texture2D texture, Rectangle srcRec, Rectangle destRect, Color color)
            : base(texture, srcRec, destRect, fireRate: 1.0f, knockback: 35f, color) // knockback grande
        {
        }

        public override void Fire(Vector2 muzzle, Vector2 dir,
                                  List<Projectile> projectiles,
                                  Entity owner)
        {
            if (!CanFire) return;

            float baseAngle = (float)Math.Atan2(dir.Y, dir.X);
            float startAngle = baseAngle - ConeAngle / 2f;
            float endAngle = baseAngle + ConeAngle / 2f;

            if (PelletCount <= 1)
            {
                Vector2 d = Vector2.Normalize(dir);
                var single = new Projectile(
                    muzzle,
                    d * BulletSpeed,
                    DamagePerPellet,
                    BulletLifetime,
                    Color.OrangeRed,
                    3f
                );
                projectiles.Add(single);
            }
            else
            {
                for (int i = 0; i < PelletCount; i++)
                {
                    float t = i / (float)(PelletCount - 1);
                    float angle = MathHelper.Lerp(startAngle, endAngle, t);

                    Vector2 pelletDir = new((float)Math.Cos(angle), (float)Math.Sin(angle));
                    if (pelletDir != Vector2.Zero)
                        pelletDir.Normalize();

                    var proj = new Projectile(
                        position: muzzle,
                        velocity: pelletDir * BulletSpeed,
                        damage: DamagePerPellet,
                        lifetime: BulletLifetime,
                        color: Color.OrangeRed,
                        radius: 3f
                    );

                    projectiles.Add(proj);
                }
            }

            // Knockback fuerte hacia atrás
            owner.Center -= Vector2.Normalize(dir) * Knockback;

            ResetCooldown();
        }
    }
}
