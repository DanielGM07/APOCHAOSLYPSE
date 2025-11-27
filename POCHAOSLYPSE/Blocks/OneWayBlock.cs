using Microsoft.Xna.Framework;

namespace POCHAOSLYPSE
{
    public class OneWayBlock : Block
    {
        public OneWayBlock(Rectangle collider, Rectangle srcRectangle)
            : base(collider, srcRectangle)
        { }

        public override void horizontalAction(Entity entity)
        {
            // Las plataformas NO bloquean horizontalmente
        }

        public override void verticalActions(Entity entity)
        {
            // Solo colisiona si viene cayendo
            if (entity.velocity.Y <= 0)
                return;

            Rectangle rect = entity.destinationRectangle;

            bool crossingTop =
                rect.Bottom > collider.Top &&
                rect.Top < collider.Top;

            if (!crossingTop)
                return;

            // Lo apoyamos arriba
            rect.Y = collider.Top - rect.Height;
            entity.destinationRectangle = rect;

            entity.velocity = new Vector2(entity.velocity.X, 0);
            entity.onGround = true;
        }
    }
}
