using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class MeleeWeapon : Weapon
    {
        public MeleeWeapon(Texture2D texture, Rectangle srcRec, Rectangle destRect,
                           float fireRate, float knockback)
            : base(texture, srcRec, destRect, fireRate, knockback)
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
