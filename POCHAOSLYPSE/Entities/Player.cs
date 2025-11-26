using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE
{
    public class Player : Entity
    {

        private int numJumps;
        public int jumpCounter;

        

        public Player(Texture2D texture, Rectangle srcRec, Rectangle destRec) : base(texture, srcRec, destRec)
        {
        }

        public void Strafe()
        {
        }

        public void Update(KeyboardState keystate, KeyboardState prevKeystate, GameTime gameTime)
        {
            
        }
    }
}
