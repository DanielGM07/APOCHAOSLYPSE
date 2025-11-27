using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class MeleeWeapon : Weapon
    {
        public MeleeWeapon(Texture2D texture, Rectangle srcRec, Rectangle destRect, float fireRate, float knockback, Color color)
            : base(texture, srcRec, destRect, fireRate, knockback, color)
        {
        }

        public override void Fire(Vector2 muzzle, Vector2 dir,
                                  List<Projectile> projectiles,
                                  Entity owner)
        {
            // Más adelante le metemos hitbox melee.
            if (!CanFire) return;
            ResetCooldown();
        }
    }
}
