using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE
{
    public class PlayScene : IScene
    {
        // ⭐ CAMBIO ⭐
        // Ahora la escena almacena la ruta del mapa y tileset
        private readonly string mapPath;
        private readonly string tilesetPath;

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
        private FlamethrowerWeapon flamethrower;

        private Weapon currentWeapon;

        private readonly List<Projectile> projectiles = new();
        private readonly List<Explosion> explosions = new();
        private readonly List<FlameParticle> flames = new();

        private SpriteFont hudFont;

        private MouseState prevMouse;


        // ⭐ CAMBIO ⭐
        // PlayScene ahora recibe el mapa y tileset por parámetro
        public PlayScene(string mapPath, string tilesetPath)
        {
            this.mapPath = mapPath;
            this.tilesetPath = tilesetPath;
        }


        public void LoadContent()
        {
            var loader         = ContentLoader.Instance;
            var graphicsDevice = loader.graphics.GraphicsDevice;

            hudFont = loader.font;

            pixel = new Texture2D(graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            Rectangle pixelRectangle = new(0, 0, 1, 1);

            // ⭐ CAMBIO ⭐
            // TileMap ahora usa el mapa recibido por constructor
            tileMap = new TileMap(isCollidable: false, canDraw: true);
            tileMap.GetBlocks(mapPath);

            // ⭐ CAMBIO ⭐
            // Tileset ahora también viene del constructor
            tilesetTexture = loader.LoadImage(tilesetPath);

            // Player
            var playerRect = new Rectangle(160, 160, 32, 32);

            player = new Player(pixel, pixelRectangle, playerRect, Color.White);
            player.color   = Color.Black;
            player.TileMap = tileMap;

            // Armas
            ak47 = new AK47(pixel, pixelRectangle, new Rectangle(0, 0, 50, 10), Color.Yellow);

            shotgun = new Shotgun(pixel, pixelRectangle, new Rectangle(0, 0, 60, 12), Color.OrangeRed);
            shotgun.color = Color.OrangeRed;

            rocketLauncher = new RocketLauncher(pixel, pixelRectangle, new Rectangle(0, 0, 60, 14), Color.LimeGreen);

            katana = new Katana(pixel, pixelRectangle, new Rectangle(0, 0, 10, 50), Color.MediumPurple);

            gatlingGun = new GatlingGun(pixel, pixelRectangle, new Rectangle(0, 0, 70, 14), Color.Silver);

            grapplingHook = new GrapplingHookWeapon(pixel, pixelRectangle, new Rectangle(0, 0, 40, 8), Color.Cyan);

            flamethrower = new FlamethrowerWeapon(pixel, pixelRectangle, new Rectangle(0, 0, 55, 12), Color.Orange);

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

            // Zoom
            if (keyboard.IsKeyDown(Keys.Q))
                Camera.Instance.SetTargetZoom(Camera.Instance.Zoom + 0.05f);
            if (keyboard.IsKeyDown(Keys.E))
                Camera.Instance.SetTargetZoom(Camera.Instance.Zoom - 0.05f);

            // Player
            player.Update(gameTime);

            // Aimed direction
            Vector2 mouseWorld = Camera.Instance.ScreenToCamera(mouse.Position.ToVector2());
            Vector2 aimDir     = mouseWorld - player.Center;
            if (aimDir != Vector2.Zero)
                aimDir.Normalize();
            else
                aimDir = Vector2.UnitX;

            // Cambiar arma
            if (keyboard.IsKeyDown(Keys.D1)) currentWeapon = ak47;
            if (keyboard.IsKeyDown(Keys.D2)) currentWeapon = shotgun;
            if (keyboard.IsKeyDown(Keys.D3)) currentWeapon = rocketLauncher;
            if (keyboard.IsKeyDown(Keys.D4)) currentWeapon = katana;
            if (keyboard.IsKeyDown(Keys.D5)) currentWeapon = gatlingGun;
            if (keyboard.IsKeyDown(Keys.D6)) currentWeapon = grapplingHook;
            if (keyboard.IsKeyDown(Keys.D7)) currentWeapon = flamethrower;

            // Peso de armas
            float baseMoveSpeed = 250f;

            if (currentWeapon == gatlingGun)
                player.MoveSpeed = baseMoveSpeed * 0.6f;
            else if (currentWeapon == rocketLauncher)
                player.MoveSpeed = baseMoveSpeed * 0.85f;
            else if (currentWeapon == grapplingHook)
                player.MoveSpeed = baseMoveSpeed * 0.9f;
            else if (currentWeapon == flamethrower)
                player.MoveSpeed = baseMoveSpeed * 0.8f;
            else
                player.MoveSpeed = baseMoveSpeed;

            // Posición y update del arma
            currentWeapon.Position = player.Center + aimDir * 30f;
            currentWeapon.Update(gameTime);

            // LÓGICA DE DISPARO
            if (currentWeapon is GrapplingHookWeapon hook)
            {
                Vector2 muzzle = player.Center + aimDir * 40f;

                if (leftJustPressed)
                    hook.StartGrapple(muzzle, aimDir, player);

                hook.UpdateHook(gameTime, tileMap, player, leftDown);

                if (leftJustReleased)
                    hook.Release();
            }
            else if (currentWeapon is FlamethrowerWeapon flame)
            {
                if (leftDown)
                {
                    Vector2 muzzle = player.Center + aimDir * 35f;
                    flame.EmitFlames(muzzle, aimDir, flames, player);
                }

                if (grapplingHook.CurrentHook != null)
                    grapplingHook.Release();
            }
            else
            {
                if (leftDown)
                {
                    Vector2 muzzle = player.Center + aimDir * 50f;
                    currentWeapon.Fire(muzzle, aimDir, projectiles, player);
                }

                if (grapplingHook.CurrentHook != null)
                    grapplingHook.Release();
            }

            // Proyectiles
            foreach (var p in projectiles)
                p.Update(gameTime);

            tileMap.HandleProjectileCollisions(projectiles, explosions);
            projectiles.RemoveAll(p => !p.IsAlive);

            // Explosiones
            foreach (var e in explosions)
                e.Update(gameTime);
            explosions.RemoveAll(e => !e.IsAlive);

            // Fuego
            foreach (var f in flames)
                f.Update(gameTime);
            flames.RemoveAll(f => !f.IsAlive);

            // Cámara
            Camera.Instance.FollowPlayer(gameTime, player);
            Camera.Instance.UpdateZoom(gameTime);

            prevMouse = mouse;
        }


        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            tileMap.Draw(tilesetTexture, gameTime, spriteBatch);

            if (grapplingHook.CurrentHook != null)
                grapplingHook.CurrentHook.Draw(spriteBatch, pixel, player.Center);

            foreach (var flame in flames)
                flame.Draw(spriteBatch, pixel);

            player.Draw(spriteBatch, gameTime);

            currentWeapon.Draw(spriteBatch, gameTime);

            foreach (var proj in projectiles)
                proj.Draw(spriteBatch, pixel);

            foreach (var ex in explosions)
                ex.Draw(spriteBatch, pixel);

            if (hudFont != null && currentWeapon != null)
            {
                spriteBatch.DrawString(
                    hudFont,
                    $"Arma actual: {currentWeapon.GetType().Name} (MOVE SPD: {player.MoveSpeed:0})",
                    new Vector2(10, 10),
                    Color.White
                );
            }
        }
    }
}
