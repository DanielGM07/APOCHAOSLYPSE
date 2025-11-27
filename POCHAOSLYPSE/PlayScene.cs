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
        private FlamethrowerWeapon flamethrower;   // 🔥 nuevo

        private Weapon currentWeapon;

        private readonly List<Projectile>   projectiles = new();
        private readonly List<Explosion>    explosions  = new();
        private readonly List<FlameParticle> flames     = new();  // 🔥 partículas de fuego

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

            flamethrower = new FlamethrowerWeapon(
                texture:  pixel,
                srcRec:   pixelRectangle,
                destRect: new Rectangle(0, 0, 55, 12),
                color:    Color.Orange
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

            bool leftDown         = mouse.LeftButton == ButtonState.Pressed;
            bool leftWasDown      = prevMouse.LeftButton == ButtonState.Pressed;
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

            // 🔹 Cambio de arma
            if (keyboard.IsKeyDown(Keys.D1)) currentWeapon = ak47;
            if (keyboard.IsKeyDown(Keys.D2)) currentWeapon = shotgun;
            if (keyboard.IsKeyDown(Keys.D3)) currentWeapon = rocketLauncher;
            if (keyboard.IsKeyDown(Keys.D4)) currentWeapon = katana;
            if (keyboard.IsKeyDown(Keys.D5)) currentWeapon = gatlingGun;
            if (keyboard.IsKeyDown(Keys.D6)) currentWeapon = grapplingHook;
            if (keyboard.IsKeyDown(Keys.D7)) currentWeapon = flamethrower;

            // 🔹 Peso de armas
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
                player.MoveSpeed = baseMoveSpeed * 0.9f;
            }
            else if (currentWeapon == flamethrower)
            {
                player.MoveSpeed = baseMoveSpeed * 0.8f; // algo pesado
            }
            else
            {
                player.MoveSpeed = baseMoveSpeed;
            }

            // Posición del arma
            Vector2 weaponOffset   = aimDir * 30f;
            currentWeapon.Position = player.Center + weaponOffset;
            currentWeapon.Update(gameTime);

            // 🔹 Lógica de disparo según tipo de arma
            if (currentWeapon is GrapplingHookWeapon hookWeapon)
            {
                Vector2 muzzle = player.Center + aimDir * 40f;

                if (leftJustPressed)
                {
                    hookWeapon.StartGrapple(muzzle, aimDir, player);
                }

                hookWeapon.UpdateHook(gameTime, tileMap, player, isHoldingButton: leftDown);

                if (leftJustReleased)
                {
                    hookWeapon.Release();
                }
            }
            else if (currentWeapon is FlamethrowerWeapon flameWeapon)
            {
                // 🔥 Lanzallamas: mantener click = emitir fuego
                if (leftDown)
                {
                    Vector2 muzzle = player.Center + aimDir * 35f;
                    flameWeapon.EmitFlames(muzzle, aimDir, flames, player);
                }

                // Si cambiamos de arma, asegurate que el hook no siga activo
                if (grapplingHook != null && grapplingHook.CurrentHook != null)
                {
                    grapplingHook.Release();
                }
            }
            else
            {
                // Armas normales (balas/proyectiles)
                if (leftDown)
                {
                    Vector2 muzzle = player.Center + aimDir * 50f;
                    currentWeapon.Fire(muzzle, aimDir, projectiles, player);
                }

                if (grapplingHook != null && grapplingHook.CurrentHook != null)
                {
                    grapplingHook.Release();
                }
            }

            // 🔹 Proyectiles "normales"
            for (int i = 0; i < projectiles.Count; i++)
                projectiles[i].Update(gameTime);

            tileMap.HandleProjectileCollisions(projectiles, explosions);

            projectiles.RemoveAll(p => !p.IsAlive);

            // 🔹 Explosiones
            for (int i = 0; i < explosions.Count; i++)
                explosions[i].Update(gameTime);
            explosions.RemoveAll(e => !e.IsAlive);

            // 🔹 Partículas de fuego
            for (int i = 0; i < flames.Count; i++)
                flames[i].Update(gameTime);
            flames.RemoveAll(f => !f.IsAlive);

            // FUTURO: acá podrías pasar la lista de enemigos a cada flame.ApplyDamage(enemies, dt)

            // 🔹 Cámara
            Camera.Instance.FollowPlayer(gameTime, player);
            Camera.Instance.UpdateZoom(gameTime);

            prevMouse = mouse;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Mapa
            tileMap.Draw(tilesetTexture, gameTime, spriteBatch);

            // Hook (cuerda + cabeza) si existe
            if (grapplingHook != null && grapplingHook.CurrentHook != null && grapplingHook.CurrentHook.IsAlive)
            {
                grapplingHook.CurrentHook.Draw(spriteBatch, pixel, player.Center);
            }

            // 🔥 Partículas de fuego
            foreach (var flame in flames)
                flame.Draw(spriteBatch, pixel);

            // Player
            player.Draw(spriteBatch, gameTime);

            // Arma
            currentWeapon.Draw(spriteBatch, gameTime);

            // Balas
            foreach (var proj in projectiles)
                proj.Draw(spriteBatch, pixel);

            // Explosiones
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
