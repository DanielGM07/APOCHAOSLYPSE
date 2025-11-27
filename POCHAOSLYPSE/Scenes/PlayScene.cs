using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE
{
    public class PlayScene : IScene
    {
        private readonly string mapPath;
        private readonly string tilesetPath;

        private TileMap  tileMap;
        private Texture2D tilesetTexture;
        private Texture2D pixel;

        private Player player;

        private Weapon ak47;
        private Weapon shotgun;
        private Weapon rocketLauncher;
        private Weapon katana;
        private Weapon gatlingGun;
        private GrapplingHookWeapon  grapplingHook;
        private FlamethrowerWeapon   flamethrower;

        private Weapon currentWeapon;

        private readonly List<Projectile>    projectiles       = new(); // del player
        private readonly List<Projectile>    enemyProjectiles  = new(); // de enemigos (no chocan con el mapa)
        private readonly List<Explosion>     explosions        = new();
        private readonly List<FlameParticle> flames            = new();

        private readonly List<Enemy>         enemies           = new();

        private SpriteFont hudFont;

        private MouseState prevMouse;

        public PlayScene(string mapPath, string tilesetPath)
        {
            this.mapPath    = mapPath;
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

            tileMap = new TileMap(isCollidable: false, canDraw: true);
            tileMap.GetBlocks(mapPath);

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

            currentWeapon = ak47;

            // 🔹 Spawnear enemigos desde los EnemySpawns del TileMap
// 🔹 Spawnear enemigos desde los EnemySpawns del TileMap
enemies.Clear();
foreach (var spawn in tileMap.EnemySpawns)
{
    // Tamaño distinto según el tipo de enemigo
    int enemyWidth;
    int enemyHeight;

    switch (spawn.Kind)
    {
        case EnemyKind.Light:
            enemyWidth  = 24;
            enemyHeight = 32;
            break;

        case EnemyKind.Medium:
            enemyWidth  = 48;
            enemyHeight = 64;
            break;

        case EnemyKind.Heavy:
            enemyWidth  = 64;
            enemyHeight = 96;
            break;

        default:
            enemyWidth  = 32;
            enemyHeight = 32;
            break;
    }

    // Centramos el rect usando el centro del tile como posición
    Rectangle enemyRect = new(
        (int)spawn.Position.X - enemyWidth / 2,
        (int)spawn.Position.Y - enemyHeight,   // apoyado “de pie” sobre el tile
        enemyWidth,
        enemyHeight
    );

    Color col = spawn.Kind switch
    {
        EnemyKind.Light  => Color.Cyan,
        EnemyKind.Medium => Color.Orange,
        EnemyKind.Heavy  => Color.DarkRed,
        _                => Color.IndianRed
    };

    var enemy = new Enemy(pixel, pixelRectangle, enemyRect, col, spawn.Kind)
    {
        TileMap = tileMap
    };

    enemies.Add(enemy);
}
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

            // 🔴 NUEVO: si el player muere, volver al menú
            if (!player.isAlive)
            {
                // Sacamos la PlayScene del stack → queda el MenuScene
                Game1.SceneManager.RemoveScene();
                return; // IMPORTANTÍSIMO: no seguir usando player ni nada de esta escena
            }

            // Dirección hacia el mouse (mundo)
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

            // Posición arma
            currentWeapon.Position = player.Center + aimDir * 30f;
            currentWeapon.Update(gameTime);

            // Disparo según tipo
            if (currentWeapon is GrapplingHookWeapon hookWeapon)
            {
                Vector2 muzzle = player.Center + aimDir * 40f;

                if (leftJustPressed)
                    hookWeapon.StartGrapple(muzzle, aimDir, player);

                hookWeapon.UpdateHook(gameTime, tileMap, player, leftDown);

                if (leftJustReleased)
                    hookWeapon.Release();
            }
            else if (currentWeapon is FlamethrowerWeapon flameWeapon)
            {
                if (leftDown)
                {
                    Vector2 muzzle = player.Center + aimDir * 35f;
                    flameWeapon.EmitFlames(muzzle, aimDir, flames, player);
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

            // Proyectiles del player
            foreach (var p in projectiles)
                p.Update(gameTime);

            tileMap.HandleProjectileCollisions(projectiles, explosions);
            projectiles.RemoveAll(p => !p.IsAlive);

            // 🔹 IA de enemigos + proyectiles enemigos (no chocan con el mapa)
            foreach (var enemy in enemies)
            {
                if (!enemy.isAlive) continue;
                enemy.UpdateAI(gameTime, player, tileMap, enemyProjectiles);
            }

            for (int i = 0; i < enemyProjectiles.Count; i++)
                enemyProjectiles[i].Update(gameTime);
            enemyProjectiles.RemoveAll(p => !p.IsAlive);

            // 🔹 Explosiones
            foreach (var e in explosions)
                e.Update(gameTime);
            explosions.RemoveAll(e => !e.IsAlive);

            // 🔹 Fuego
            foreach (var f in flames)
                f.Update(gameTime);
            flames.RemoveAll(f => !f.IsAlive);

            // 🔹 Colisiones: proyectiles del player → enemigos
            HandlePlayerProjectilesVsEnemies(projectiles, enemies);

            // 🔹 Colisiones: proyectiles enemigos → player
            HandleEnemyProjectilesVsPlayer(enemyProjectiles, player);

            // Cámara
            Camera.Instance.FollowPlayer(gameTime, player);
            Camera.Instance.UpdateZoom(gameTime);

            prevMouse = mouse;
        }

        private void HandlePlayerProjectilesVsEnemies(List<Projectile> bullets, List<Enemy> enemies)
        {
            foreach (var bullet in bullets)
            {
                if (!bullet.IsAlive) continue;

                Rectangle projRect = new(
                    (int)(bullet.Position.X - bullet.Radius),
                    (int)(bullet.Position.Y - bullet.Radius),
                    (int)(bullet.Radius * 2f),
                    (int)(bullet.Radius * 2f)
                );

                foreach (var enemy in enemies)
                {
                    if (!enemy.isAlive) continue;

                    if (projRect.Intersects(enemy.destinationRectangle))
                    {
                        enemy.Health -= (int)bullet.Damage;
                        bullet.Lifetime = 0f;

                        if (!enemy.isAlive)
                        {
                            // opcional: mini shake o algo
                            Camera.Instance.Shake(4f, 0.1f);
                        }

                        break;
                    }
                }
            }
        }

        private void HandleEnemyProjectilesVsPlayer(List<Projectile> bullets, Player player)
        {
            foreach (var bullet in bullets)
            {
                if (!bullet.IsAlive) continue;

                Rectangle projRect = new(
                    (int)(bullet.Position.X - bullet.Radius),
                    (int)(bullet.Position.Y - bullet.Radius),
                    (int)(bullet.Radius * 2f),
                    (int)(bullet.Radius * 2f)
                );

                if (projRect.Intersects(player.destinationRectangle))
                {
                    player.Health -= (int)bullet.Damage;
                    bullet.Lifetime = 0f;
                    Camera.Instance.Shake(6f, 0.1f);
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Fondo gris clarito para que no se vea negro
            spriteBatch.Draw(pixel,
                new Rectangle(
                    Camera.Instance.ViewPortRectangle.X - 2000,
                    Camera.Instance.ViewPortRectangle.Y - 2000,
                    4000,
                    4000),
                new Color(200, 200, 200)); // gris oscuro suave

            tileMap.Draw(tilesetTexture, gameTime, spriteBatch);

            if (grapplingHook.CurrentHook != null)
                grapplingHook.CurrentHook.Draw(spriteBatch, pixel, player.Center);

            foreach (var flame in flames)
                flame.Draw(spriteBatch, pixel);

            // Enemigos
            foreach (var enemy in enemies)
            {
                if (!enemy.isAlive) continue;

                enemy.Draw(spriteBatch, gameTime);

                if (enemy.MeleeHitbox.HasValue)
                {
                    spriteBatch.Draw(pixel, enemy.MeleeHitbox.Value, Color.Red * 0.4f);
                }
            }

            player.Draw(spriteBatch, gameTime);

            currentWeapon.Draw(spriteBatch, gameTime);

            foreach (var proj in projectiles)
                proj.Draw(spriteBatch, pixel);

            foreach (var proj in enemyProjectiles)
                proj.Draw(spriteBatch, pixel);

            foreach (var ex in explosions)
                ex.Draw(spriteBatch, pixel);

            if (hudFont != null && currentWeapon != null)
            {
                spriteBatch.DrawString(
                    hudFont,
                    $"Arma actual: {currentWeapon.GetType().Name} (MOVE SPD: {player.MoveSpeed:0}) HP:{player.Health}",
                    new Vector2(10, 10),
                    Color.White
                );
            }
        }
    }
}
