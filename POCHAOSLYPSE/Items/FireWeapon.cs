using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public abstract class FireWeapon : Weapon
    {
        protected FireWeapon(Texture2D texture, Rectangle srcRec, Rectangle destRect,
                             float fireRate, float knockback)
            : base(texture, srcRec, destRect, fireRate, knockback)
        {
        }
    }
}
