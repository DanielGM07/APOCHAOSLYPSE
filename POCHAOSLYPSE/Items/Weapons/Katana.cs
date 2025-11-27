using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class Katana : MeleeWeapon
    {
        public Katana(Texture2D texture, Rectangle srcRec, Rectangle destRect)
            : base(texture, srcRec, destRect, fireRate: 2.0f, knockback: 0f)
        {
        }

        // Por ahora se comporta como un arma sin efecto visible,
        // solo cambia el rectángulo y color para probar que se "equipa".
        public override void Fire(Vector2 muzzle, Vector2 dir,
                                  List<Projectile> projectiles,
                                  Entity owner)
        {
            if (!CanFire) return;
            // Más adelante: hitbox melee
            ResetCooldown();
        }
    }
}
