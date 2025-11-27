using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE
{
    public class PlayScene : IScene
    {
        private TileMap tileMap;
        private Texture2D tilesetTexture;
        private Texture2D pixel;

        private Player player;

        private Weapon ak47;
        private Weapon shotgun;
        private Weapon rocketLauncher;
        private Weapon katana;
        private Weapon gatlingGun;
        private GrapplingHookWeapon grapplingHook;

        private Weapon currentWeapon;

        private readonly List<Projectile> projectiles = new();
        private readonly List<Explosion>  explosions  = new();

        private SpriteFont hudFont;

        private MouseState prevMouse;

        public void LoadContent()
        {
            var loader         = ContentLoader.Instance;
            var graphicsDevice = loader.graphics.GraphicsDevice;

            // Fuente para HUD
            hudFont = loader.font;

            // Pixel 1x1 para todo
            pixel = new Texture2D(graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            Rectangle pixelRectangle = new(0, 0, 1, 1);

            // Tilemap
            tileMap = new TileMap(isCollidable: false, canDraw: true);
            tileMap.GetBlocks("tiled/test1.csv");

            tilesetTexture = loader.LoadImage(
                "Content/Another Metroidvania Asset Pack Vol. 1 ver. 1.5/Tilesets/library/tileset_library.png"
            );

            // Player
            var playerRect = new Rectangle(
                160,
                160,
                32,
                32
            );

            player = new Player(
                pixel,
                pixelRectangle,
                playerRect,
                Color.White
            );
            player.color   = Color.Black;
            player.TileMap = tileMap;

            // Armas
            ak47 = new AK47(
                texture:  pixel,
                srcRec:   pixelRectangle,
                destRect: new Rectangle(0, 0, 50, 10),
                color:    Color.Yellow
            );

            shotgun = new Shotgun(
                texture:  pixel,
                srcRec:   pixelRectangle,
                destRect: new Rectangle(0, 0, 60, 12),
                color:    Color.OrangeRed
            );
            shotgun.color = Color.OrangeRed;

            rocketLauncher = new RocketLauncher(
                texture:  pixel,
                srcRec:   pixelRectangle,
                destRect: new Rectangle(0, 0, 60, 14),
                color:    Color.LimeGreen
            );

            katana = new Katana(
                texture:  pixel,
                srcRec:   pixelRectangle,
                destRect: new Rectangle(0, 0, 10, 50),
                color:    Color.MediumPurple
            );

            gatlingGun = new GatlingGun(
                texture:  pixel,
                srcRec:   pixelRectangle,
                destRect: new Rectangle(0, 0, 70, 14),
                color:    Color.Silver
            );

            grapplingHook = new GrapplingHookWeapon(
                texture:  pixel,
                srcRec:   pixelRectangle,
                destRect: new Rectangle(0, 0, 40, 8),
                color:    Color.Cyan
            );

            // Arma inicial
            currentWeapon = shotgun;
        }

        public void UnloadContent()
        {
            pixel?.Dispose();
            tilesetTexture?.Dispose();
        }

        public void Update(GameTime gameTime)
        {
            var mouse    = Mouse.GetState();
            var keyboard = Keyboard.GetState();

            bool leftDown       = mouse.LeftButton == ButtonState.Pressed;
            bool leftWasDown    = prevMouse.LeftButton == ButtonState.Pressed;
            bool leftJustPressed  = leftDown && !leftWasDown;
            bool leftJustReleased = !leftDown && leftWasDown;

            // 🔹 Controles de zoom (Q = zoom in, E = zoom out)
            if (keyboard.IsKeyDown(Keys.Q))
                Camera.Instance.SetTargetZoom(Camera.Instance.Zoom + 0.05f);
            if (keyboard.IsKeyDown(Keys.E))
                Camera.Instance.SetTargetZoom(Camera.Instance.Zoom - 0.05f);

            // Player (movimiento + colisiones)
            player.Update(gameTime);

            // Dirección de apuntado (hacia el mouse en mundo)
            Vector2 mouseWorld = Camera.Instance.ScreenToCamera(mouse.Position.ToVector2());
            Vector2 aimDir     = mouseWorld - player.Center;
            if (aimDir != Vector2.Zero)
                aimDir.Normalize();
            else
                aimDir = Vector2.UnitX;

            // 🔹 Cambio de arma (D1–D6)
            if (keyboard.IsKeyDown(Keys.D1)) currentWeapon = ak47;
            if (keyboard.IsKeyDown(Keys.D2)) currentWeapon = shotgun;
            if (keyboard.IsKeyDown(Keys.D3)) currentWeapon = rocketLauncher;
            if (keyboard.IsKeyDown(Keys.D4)) currentWeapon = katana;
            if (keyboard.IsKeyDown(Keys.D5)) currentWeapon = gatlingGun;
            if (keyboard.IsKeyDown(Keys.D6)) currentWeapon = grapplingHook;

            // 🔹 Peso de armas: modificar MoveSpeed del player
            float baseMoveSpeed = 250f;

            if (currentWeapon == gatlingGun)
            {
                player.MoveSpeed = baseMoveSpeed * 0.6f;
            }
            else if (currentWeapon == rocketLauncher)
            {
                player.MoveSpeed = baseMoveSpeed * 0.85f;
            }
            else if (currentWeapon == grapplingHook)
            {
                player.MoveSpeed = baseMoveSpeed * 0.9f; // un poquito más pesada si querés
            }
            else
            {
                player.MoveSpeed = baseMoveSpeed;
            }

            // Posición del arma (para todas)
            Vector2 weaponOffset   = aimDir * 30f;
            currentWeapon.Position = player.Center + weaponOffset;
            currentWeapon.Update(gameTime);

            // 🔹 Lógica de disparo según tipo de arma
            if (currentWeapon is GrapplingHookWeapon hookWeapon)
            {
                // Punto de salida del hook (un poco adelante del arma)
                Vector2 muzzle = player.Center + aimDir * 40f;

                if (leftJustPressed)
                {
                    hookWeapon.StartGrapple(muzzle, aimDir, player);
                }

                // Actualizar hook (viaje, colisiones, atracción)
                hookWeapon.UpdateHook(gameTime, tileMap, player, isHoldingButton: leftDown);

                if (leftJustReleased)
                {
                    // Soltaste el botón: si no estaba enganchado, desaparece;
                    // si estaba enganchado, deja al player con el momentum actual.
                    hookWeapon.Release();
                }
            }
            else
            {
                // Armas normales
                if (leftDown)
                {
                    Vector2 muzzle = player.Center + aimDir * 50f;
                    currentWeapon.Fire(muzzle, aimDir, projectiles, player);
                }

                // Si cambiamos de arma, nos aseguramos de que el hook no siga activo
                if (grapplingHook != null && grapplingHook.CurrentHook != null)
                {
                    grapplingHook.Release();
                }
            }

            // 🔹 Proyectiles normales
            for (int i = 0; i < projectiles.Count; i++)
                projectiles[i].Update(gameTime);

            // Colisiones bala ↔ bloques (mata balas y genera explosiones de rocket)
            tileMap.HandleProjectileCollisions(projectiles, explosions);

            projectiles.RemoveAll(p => !p.IsAlive);

            // Explosiones (rectángulos rojos)
            for (int i = 0; i < explosions.Count; i++)
                explosions[i].Update(gameTime);
            explosions.RemoveAll(e => !e.IsAlive);

            // Cámara: follow + zoom suave
            Camera.Instance.FollowPlayer(gameTime, player);
            Camera.Instance.UpdateZoom(gameTime);

            prevMouse = mouse;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Mapa
            tileMap.Draw(tilesetTexture, gameTime, spriteBatch);

            // Grappling hook (si la instancia existe y está viva)
            if (grapplingHook != null && grapplingHook.CurrentHook != null && grapplingHook.CurrentHook.IsAlive)
            {
                grapplingHook.CurrentHook.Draw(spriteBatch, pixel, player.Center);
            }

            // Player
            player.Draw(spriteBatch, gameTime);

            // Arma
            currentWeapon.Draw(spriteBatch, gameTime);

            // Balas
            foreach (var proj in projectiles)
                proj.Draw(spriteBatch, pixel);

            // Explosiones (rectángulo rojo)
            foreach (var ex in explosions)
                ex.Draw(spriteBatch, pixel);

            // HUD
            if (hudFont != null && currentWeapon != null)
            {
                string weaponName = currentWeapon.GetType().Name;
                spriteBatch.DrawString(
                    hudFont,
                    $"Arma actual: {weaponName} (MOVE SPD: {player.MoveSpeed:0})",
                    new Vector2(10, 10),
                    Color.White
                );
            }
        }
    }
}
