

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace POCHAOSLYPSE
{
    public class Item : Sprite
    {
        // TODO: These properties could change

        public bool isEquipped;
        public Vector2 position;

        public float Rotation { get; private set; }  // <- ángulo en radianes


        public Item(Texture2D texture, Rectangle srcRec, Rectangle destRect) : base(texture, srcRec, destRect)
        {
            origin = new(
                srcRec.Width/2,
                srcRec.Height/2
            );
            position = destRect.Center.ToVector2();
            // Ensure destinationRectangle is placed from position (top-left)
            destinationRectangle.X = (int)(position.X - destinationRectangle.Width / 2f);
            destinationRectangle.Y = (int)(position.Y - destinationRectangle.Height / 2f);
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        
            UpdateRotationToMouse();
            destinationRectangle.X = (int)(position.X - destinationRectangle.Width / 2f);
            destinationRectangle.Y = (int)(position.Y - destinationRectangle.Height / 2f);
        }

        public Vector2 GetMousePosition()
        {
            return Camera.Instance.ScreenToCamera(Mouse.GetState().Position.ToVector2());
        }

        public Vector2 getDistanceFromMouseToItem()
        {
            Vector2 mouse = GetMousePosition();

            // Centro del item
            return mouse - position;
        }

        private Vector2 GetItemCenterOnScreen()
        {
            return position;
        }

        private void UpdateRotationToMouse()
        {
            Vector2 mousePos = GetMousePosition();
            Vector2 itemCenter = GetItemCenterOnScreen();

            Vector2 dir = mousePos - itemCenter;

            if (dir != Vector2.Zero)
            {
                // Ángulo en radianes entre el item y el mouse
                rotation = MathF.Atan2(dir.Y, dir.X);
            }
        }
   
    }
}   