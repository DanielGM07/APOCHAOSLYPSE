using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE
{
    public class Player : Entity
    {
        private int numJumps;
        public int jumpCounter;

        public float MoveSpeed = 250f;

        public Player(Texture2D texture, Rectangle srcRec, Rectangle destRec) : base(texture, srcRec, destRec)
        {
        }

        public void Strafe()
        {
        }

        public void Update(KeyboardState keystate, KeyboardState prevKeystate, GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 move = Vector2.Zero;

            if (keystate.IsKeyDown(Keys.W)) move.Y -= 1;
            if (keystate.IsKeyDown(Keys.S)) move.Y += 1;
            if (keystate.IsKeyDown(Keys.A)) move.X -= 1;
            if (keystate.IsKeyDown(Keys.D)) move.X += 1;

            if (move != Vector2.Zero)
            {
                move.Normalize();
                Center += move * MoveSpeed * dt;
            }
        }
    }
}
