using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class Enemy : Entity
    {
        public Enemy(Texture2D texture, Rectangle srcRec, Rectangle destRec, Color color)
            : base(texture, srcRec, destRec, color)
        {
            Health = 100;
            color = Color.IndianRed;
        }

        public override void Update(GameTime gameTime)
        {
            // Enemigo estático por ahora
        }
    }
}
