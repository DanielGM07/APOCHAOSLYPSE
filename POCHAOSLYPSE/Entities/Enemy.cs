using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class Enemy : Entity
    {
        public TileMap TileMap { get; set; }

        public Enemy(Texture2D texture, Rectangle srcRec, Rectangle destRec, Color color)
            : base(texture, srcRec, destRec, color)
        {
            Health = 100;
            this.color = Color.IndianRed;
        }

        public override void Update(GameTime gameTime)
        {
            // Ejemplo: enemigo quieto por ahora
            // Podés agregar IA y luego usar TileMap igual que en el Player

            base.Update(gameTime);
        }
    }
}
