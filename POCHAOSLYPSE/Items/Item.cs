

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace POCHAOSLYPSE
{
    public class Item : Sprite
    {
        public bool isEquipped;
        public Vector2 position;

        public Item(Texture2D texture, Rectangle srcRec, Rectangle destRect)
            : base(texture, srcRec, destRect)
        {
            // centro del source (16x16, etc.)
            origin = new Vector2(
                srcRec.Width  / 2f,
                srcRec.Height / 2f
            );

            position = destRect.Center.ToVector2();
            destinationRectangle.X = (int)(position.X - destinationRectangle.Width / 2f);
            destinationRectangle.Y = (int)(position.Y - destinationRectangle.Height / 2f);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            UpdateRotationToMouse();

            // mantener destRect centrado en position
            destinationRectangle.X = (int)(position.X - destinationRectangle.Width / 2f);
            destinationRectangle.Y = (int)(position.Y - destinationRectangle.Height / 2f);
        }

        public Vector2 GetMousePosition()
        {
            // Pasar de pantalla -> mundo
            return Camera.Instance.ScreenToCamera(Mouse.GetState().Position.ToVector2());
        }

        private void UpdateRotationToMouse()
        {
            Vector2 mousePos   = GetMousePosition();
            Vector2 itemCenter = position; // ya es el centro en mundo

            Vector2 dir = mousePos - itemCenter;

            if (dir != Vector2.Zero)
                rotation = MathF.Atan2(dir.Y, dir.X);
        }
    }

}   