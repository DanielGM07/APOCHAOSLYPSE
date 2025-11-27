using Microsoft.Xna.Framework;

namespace POCHAOSLYPSE
{
    public abstract class Block
    {
        public Rectangle collider;
        public Rectangle srcRectangle;

        protected Block(Rectangle collider, Rectangle srcRectangle)
        {
            this.collider = collider;
            this.srcRectangle = srcRectangle;
        }

        public abstract void horizontalAction(Entity entity);
        public abstract void verticalActions(Entity entity);
    }
}
