using System.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE
{
    public class Entity : Sprite
    {
        public bool onGround {get; set;} = false;
        public int health {get;set;}
        public bool isAlive => health < 0;
        public Vector2 velocity {get;set;}
        public bool isFacingLeft = false;
        
        public Entity(Texture2D texture, Rectangle srcRec, Rectangle destRec) : base(texture, srcRec, destRec)
        {
        }
        
        public void Update()
        {
        }
        
    }
}
