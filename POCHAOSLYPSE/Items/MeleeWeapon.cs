using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class MeleeWeapon : Weapon
    {
        public MeleeWeapon(Texture2D texture, Rectangle srcRec, Rectangle destRect) : base(texture, srcRec, destRect)
        {
        }
    }
}