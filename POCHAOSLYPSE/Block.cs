
using Microsoft.Xna.Framework;

namespace POCHAOSLYPSE;
public abstract class Block (Rectangle collier, Rectangle srcRectangle)
{
    public Rectangle collider = collier;
    public Rectangle srcRectangle = srcRectangle;
    public abstract void horizontalAction(Sprite sprite);
    public abstract void verticalActions(Sprite sprite);

}