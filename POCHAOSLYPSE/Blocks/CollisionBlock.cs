using Microsoft.Xna.Framework;

namespace POCHAOSLYPSE
{
    public class Collisionblock : Block
    {
        public Collisionblock(Rectangle collider, Rectangle srcRectangle)
            : base(collider, srcRectangle)
        { }

        public override void horizontalAction(Entity entity)
        {
            if (entity.velocity.X == 0)
                return;

            Rectangle rect = entity.destinationRectangle;

            if (entity.velocity.X > 0)
            {
                // Viene de la izquierda → choca contra lado izquierdo del bloque
                rect.X = collider.Left - rect.Width;
            }
            else if (entity.velocity.X < 0)
            {
                // Viene de la derecha → choca contra lado derecho del bloque
                rect.X = collider.Right;
            }

            entity.destinationRectangle = rect;
            entity.velocity = new Vector2(0, entity.velocity.Y);
        }

        public override void verticalActions(Entity entity)
        {
            if (entity.velocity.Y == 0)
                return;

            Rectangle rect = entity.destinationRectangle;

            if (entity.velocity.Y > 0)
            {
                // Cayendo → apoyar arriba del bloque
                rect.Y = collider.Top - rect.Height;
                entity.onGround = true;
                entity.velocity = new Vector2(entity.velocity.X, 0);
            }
            else if (entity.velocity.Y < 0)
            {
                // Subiendo → golpear techo
                rect.Y = collider.Bottom;
                entity.velocity = new Vector2(entity.velocity.X, 0);
            }

            entity.destinationRectangle = rect;
        }
    }
}
