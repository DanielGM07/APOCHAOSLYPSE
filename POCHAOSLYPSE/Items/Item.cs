using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE
{
    public class Item : Sprite
    {
        public bool isEquipped;

        // Centro lógico del item en coordenadas de mundo
        public Vector2 position;

        public Item(Texture2D texture, Rectangle srcRec, Rectangle destRect)
            : base(texture, srcRec, destRect)
        {
            // Usamos el centro del source como origen para rotar
            origin = new Vector2(
                srcRec.Width  / 2f,
                srcRec.Height / 2f
            );

            // Posición inicial: centro del rect destino
            Position = destRect.Center.ToVector2();
        }

        /// <summary>
        /// Acceso cómodo a la posición. Mantiene sincronizado el destinationRectangle.
        /// </summary>
        public Vector2 Position
        {
            get => position;
            set
            {
                position = value;
                destinationRectangle.X = (int)(position.X - destinationRectangle.Width / 2f);
                destinationRectangle.Y = (int)(position.Y - destinationRectangle.Height / 2f);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Rotar hacia donde apunta el mouse
            UpdateRotationToMouse();

            // Mantener el destRect centrado en la posición
            destinationRectangle.X = (int)(position.X - destinationRectangle.Width / 2f);
            destinationRectangle.Y = (int)(position.Y - destinationRectangle.Height / 2f);
        }

        private Vector2 GetMousePositionWorld()
        {
            // Pasar de pantalla -> mundo usando la cámara
            return Camera.Instance.ScreenToCamera(Mouse.GetState().Position.ToVector2());
        }

        private void UpdateRotationToMouse()
        {
            Vector2 mousePos   = GetMousePositionWorld();
            Vector2 itemCenter = position; // ya es el centro en mundo

            Vector2 dir = mousePos - itemCenter;

            if (dir != Vector2.Zero)
                rotation = MathF.Atan2(dir.Y, dir.X);
        }
    }
}
