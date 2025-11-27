using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE
{
    public class Entity : Sprite
    {
        public bool onGround { get; set; } = false;
        public int Health { get; set; }
        public bool isAlive => Health < 0; // lo dejo como lo tenías
        public Vector2 velocity { get; set; }
        public bool isFacingLeft = false;

        public Entity(Texture2D texture, Rectangle srcRec, Rectangle destRec, Color color) : base(texture, srcRec, destRec, color)
        {
        }

        public void Update()
        {
        }
    }
}
