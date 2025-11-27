using Microsoft.Xna.Framework;

namespace POCHAOSLYPSE
{
    public class DecoBlock : Block
    {
        public DecoBlock(Rectangle collider, Rectangle srcRectangle)
            : base(collider, srcRectangle)
        { }

        public override void horizontalAction(Entity entity)
        {
            // No colisión
        }

        public override void verticalActions(Entity entity)
        {
            // No colisión
        }
    }
}
