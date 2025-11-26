using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class Shotgun : FireWeapon
    {
        public Shotgun(Texture2D texture, Rectangle srcRec, Rectangle destRect) : base(texture, srcRec, destRect)
        {
        }
    }
}