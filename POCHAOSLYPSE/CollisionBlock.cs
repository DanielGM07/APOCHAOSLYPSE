using Microsoft.Xna.Framework;

namespace POCHAOSLYPSE;

public class Collisionblock : Block
{
    public Collisionblock(Rectangle collier, Rectangle srcRectangle) : base(collier, srcRectangle)
    { }

    public override void horizontalAction(Sprite sprite)
    {
    }

    public override void verticalActions(Sprite sprite)
    {
    }
}