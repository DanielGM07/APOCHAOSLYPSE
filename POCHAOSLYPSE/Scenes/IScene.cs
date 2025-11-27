using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
   public interface IScene
   {
     public void LoadContent();
     public void UnloadContent();
     public void Update(GameTime gameTime);
     public void Draw(GameTime gameTime, SpriteBatch spriteBatch);
     public void DrawUI(GameTime gameTime, SpriteBatch spriteBatch);
   }
}
