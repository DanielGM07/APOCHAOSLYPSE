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
        public Weapon weapon;

        public Player(Texture2D texture, Rectangle srcRec, Rectangle destRec, Color color) : base(texture, srcRec, destRec, color)
        {
        }

        public void Strafe()
        {
        }
        KeyboardState lastState;
        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 move = Vector2.Zero;

            if (Keyboard.GetState().IsKeyDown(Keys.W)) move.Y -= 1;
            if (Keyboard.GetState().IsKeyDown(Keys.S)) move.Y += 1;
            if (Keyboard.GetState().IsKeyDown(Keys.A)) move.X -= 1;
            if (Keyboard.GetState().IsKeyDown(Keys.D)) move.X += 1;

            if (move != Vector2.Zero)
            {
                move.Normalize();
                Center += move * MoveSpeed * dt;
            }
            lastState = Keyboard.GetState();
        }
    }
}
