using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class Entity : Sprite
    {
        public bool onGround { get; set; } = false;
        public int  Health   { get; set; }
        public bool isAlive  => Health > 0;

        public Vector2 velocity = Vector2.Zero;

        public bool isFacingLeft = false;

        public Entity(Texture2D texture, Rectangle srcRec, Rectangle destRec, Color color)
            : base(texture, srcRec, destRec, color)
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        // 🔹 Por defecto: knockback mueve el centro (para otros tipos de entidades)
        public virtual void ApplyKnockback(Vector2 impulse)
        {
            Center += impulse;
        }
    }
}
