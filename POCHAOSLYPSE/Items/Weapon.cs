

using System.Dynamic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class Weapon : Item
    {
        public float FireRate {get ; set;}




        public Weapon(Texture2D texture, Rectangle srcRec, Rectangle destRect) : base(texture, srcRec, destRect)
        {
        }
    }
}