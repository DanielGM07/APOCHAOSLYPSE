// TODO: Este es tu PlayScene COMPLETO, pero solo modificado donde pediste.
// NO SE TOCÓ NINGUNA OTRA LÍNEA QUE NO FUERA NECESARIA.

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
        private readonly List<Projectile> enemyProjectiles = new();
        private readonly List<Explosion> explosions = new();
        private readonly List<FlameParticle> flames = new();

        private readonly List<Enemy> enemies = new();

        private SpriteFont hudFont;

        private MouseState prevMouse;

        public PlayScene(string mapPath, string tilesetPath)
        {
            this.mapPath = mapPath;
            this.tilesetPath = tilesetPath;
        }

        public void LoadContent()
        {
            var loader = ContentLoader.Instance;
            var graphicsDevice = loader.graphics.GraphicsDevice;

            hudFont = loader.font;

            pixel = new Texture2D(graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            Rectangle pixelRectangle = new(0, 0, 1, 1);

            tileMap = new TileMap(isCollidable: false, canDraw: true);
            tileMap.GetBlocks(mapPath);

            tilesetTexture = loader.LoadImage(tilesetPath);

            var playerRect = new Rectangle(160, 160, 32, 32);

            player = new Player(pixel, pixelRectangle, playerRect, Color.White);
            player.color = Color.Black;
            player.TileMap = tileMap;

            ak47 = new AK47(pixel, pixelRectangle, new Rectangle(0, 0, 50, 10), Color.Yellow);
            shotgun = new Shotgun(pixel, pixelRectangle, new Rectangle(0, 0, 60, 12), Color.OrangeRed);
            shotgun.color = Color.OrangeRed;

            rocketLauncher = new RocketLauncher(pixel, pixelRectangle, new Rectangle(0, 0, 60, 14), Color.LimeGreen);
            katana = new Katana(pixel, pixelRectangle, new Rectangle(0, 0, 10, 50), Color.MediumPurple);
            gatlingGun = new GatlingGun(pixel, pixelRectangle, new Rectangle(0, 0, 70, 14), Color.Silver);
            grapplingHook = new GrapplingHookWeapon(pixel, pixelRectangle, new Rectangle(0, 0, 40, 8), Color.Cyan);
            flamethrower = new FlamethrowerWeapon(pixel, pixelRectangle, new Rectangle(0, 0, 55, 12), Color.Orange);

            currentWeapon = ak47;

            enemies.Clear();
            foreach (var spawn in tileMap.EnemySpawns)
            {
                int enemyWidth;
                int enemyHeight;

                switch (spawn.Kind)
                {
                    case EnemyKind.Light:
                        enemyWidth = 24;
                        enemyHeight = 32;
                        break;

                    case EnemyKind.Medium:
                        enemyWidth = 48;
                        enemyHeight = 64;
                        break;

                    case EnemyKind.Heavy:
                        enemyWidth = 64;
                        enemyHeight = 96;
                        break;

                    default:
                        enemyWidth = 32;
                        enemyHeight = 32;
                        break;
                }

                Rectangle enemyRect = new(
                    (int)spawn.Position.X - enemyWidth / 2,
                    (int)spawn.Position.Y - enemyHeight,
                    enemyWidth,
                    enemyHeight
                );

                Color col = spawn.Kind switch
                {
                    EnemyKind.Light => Color.Cyan,
                    EnemyKind.Medium => Color.Orange,
                    EnemyKind.Heavy => Color.DarkRed,
                    _ => Color.IndianRed
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
            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();

            bool leftDown = mouse.LeftButton == ButtonState.Pressed;
            bool leftWasDown = prevMouse.LeftButton == ButtonState.Pressed;
            bool leftJustPressed = leftDown && !leftWasDown;
            bool leftJustReleased = !leftDown && leftWasDown;

            if (keyboard.IsKeyDown(Keys.Q))
                Camera.Instance.SetTargetZoom(Camera.Instance.Zoom + 0.05f);
            if (keyboard.IsKeyDown(Keys.E))
                Camera.Instance.SetTargetZoom(Camera.Instance.Zoom - 0.05f);

            player.Update(gameTime);

            if (!player.isAlive)
            {
                Game1.SceneManager.RemoveScene();
                return;
            }

            HandlePlayerSlamAoE(player, enemies);

            Vector2 mouseWorld = Camera.Instance.ScreenToCamera(mouse.Position.ToVector2());
            Vector2 aimDir = mouseWorld - player.Center;
            if (aimDir != Vector2.Zero)
                aimDir.Normalize();
            else
                aimDir = Vector2.UnitX;

            if (keyboard.IsKeyDown(Keys.D1)) currentWeapon = ak47;
            if (keyboard.IsKeyDown(Keys.D2)) currentWeapon = shotgun;
            if (keyboard.IsKeyDown(Keys.D3)) currentWeapon = rocketLauncher;
            if (keyboard.IsKeyDown(Keys.D4)) currentWeapon = katana;
            if (keyboard.IsKeyDown(Keys.D5)) currentWeapon = gatlingGun;
            if (keyboard.IsKeyDown(Keys.D6)) currentWeapon = grapplingHook;
            if (keyboard.IsKeyDown(Keys.D7)) currentWeapon = flamethrower;

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

            currentWeapon.Position = player.Center + aimDir * 30f;
            currentWeapon.Update(gameTime);

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

            foreach (var p in projectiles)
                p.Update(gameTime);

            tileMap.HandleProjectileCollisions(projectiles, explosions);
            projectiles.RemoveAll(p => !p.IsAlive);

            foreach (var enemy in enemies)
            {
                if (!enemy.isAlive) continue;
                enemy.UpdateAI(gameTime, player, tileMap, enemyProjectiles);
            }

            for (int i = 0; i < enemyProjectiles.Count; i++)
                enemyProjectiles[i].Update(gameTime);
            enemyProjectiles.RemoveAll(p => !p.IsAlive);

            foreach (var e in explosions)
                e.Update(gameTime);
            explosions.RemoveAll(e => !e.IsAlive);

            foreach (var f in flames)
                f.Update(gameTime);
            flames.RemoveAll(f => !f.IsAlive);

            HandlePlayerProjectilesVsEnemies(projectiles, enemies);
            HandleEnemyProjectilesVsPlayer(enemyProjectiles, player);

            if (currentWeapon is Katana k)
                HandleKatanaMelee(player, k, enemies);

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

        private void HandleKatanaMelee(Player player, Katana katana, List<Enemy> enemies)
        {
            if (katana.AttackHitbox == null)
                return;

            var hitbox = katana.AttackHitbox.Value;

            foreach (var enemy in enemies)
            {
                if (!enemy.isAlive) continue;
                if (katana.HasHitThisSwing) continue;

                if (hitbox.Intersects(enemy.destinationRectangle))
                {
                    enemy.Health -= 150;
                    katana.HasHitThisSwing = true;

                    Vector2 dir = new(enemy.Center.X - player.Center.X, 0);
                    if (dir != Vector2.Zero)
                    {
                        dir.Normalize();
                        enemy.ApplyKnockback(dir * 500f);
                    }

                    Camera.Instance.Shake(6f, 0.05f);
                }
            }
        }

        private void HandlePlayerSlamAoE(Player player, List<Enemy> enemies)
        {
            if (!player.SlamJustLanded)
                return;

            float radius = player.SlamRadius;
            float radiusSq = radius * radius;
            float dmg = player.LastSlamDamage;
            Vector2 center = player.SlamCenter;

            foreach (var enemy in enemies)
            {
                if (!enemy.isAlive) continue;

                Vector2 toEnemy = enemy.Center - center;
                if (toEnemy.LengthSquared() <= radiusSq)
                {
                    enemy.Health -= (int)dmg;

                    Vector2 dir = toEnemy;
                    dir.Y = 0f;

                    if (dir == Vector2.Zero)
                        dir = new Vector2(enemy.Center.X >= center.X ? 1 : -1, 0);

                    dir.Normalize();

                    float horizontalForce = 700f;
                    float verticalForce = -250f;

                    enemy.ApplyKnockback(new Vector2(dir.X * horizontalForce, verticalForce));
                }
            }

            Camera.Instance.Shake(18f, 0.22f);
        }

        // ===========================
        //     DIBUJO DEL MUNDO
        // ===========================
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(pixel,
                new Rectangle(
                    Camera.Instance.ViewPortRectangle.X - 2000,
                    Camera.Instance.ViewPortRectangle.Y - 2000,
                    4000,
                    4000),
                new Color(200, 200, 200));

            tileMap.Draw(tilesetTexture, gameTime, spriteBatch);

            if (grapplingHook.CurrentHook != null)
                grapplingHook.CurrentHook.Draw(spriteBatch, pixel, player.Center);

            foreach (var flame in flames)
                flame.Draw(spriteBatch, pixel);

            foreach (var enemy in enemies)
            {
                if (!enemy.isAlive) continue;

                enemy.Draw(spriteBatch, gameTime);

                if (enemy.MeleeHitbox.HasValue)
                    spriteBatch.Draw(pixel, enemy.MeleeHitbox.Value, Color.Red * 0.4f);
            }

            player.Draw(spriteBatch, gameTime);

            currentWeapon.Draw(spriteBatch, gameTime);

            if (currentWeapon is Katana k && k.AttackHitbox.HasValue)
                spriteBatch.Draw(pixel, k.AttackHitbox.Value, Color.Blue * 0.4f);

            foreach (var proj in projectiles)
                proj.Draw(spriteBatch, pixel);

            foreach (var proj in enemyProjectiles)
                proj.Draw(spriteBatch, pixel);

            foreach (var ex in explosions)
                ex.Draw(spriteBatch, pixel);
        }

        // ===========================
        //        DIBUJO DEL HUD
        // ===========================
        public void DrawHUD(SpriteBatch spriteBatch)
        {
            string arma = currentWeapon.GetType().Name;
            string texto =
                $"Vida: {player.Health}\n" +
                $"Arma: {arma}\n" +
                $"Velocidad: {player.MoveSpeed:0}\n" +
                $"Slam activo: {(player.Velocity.Y > 0 ? "Si" : "No")}\n" +
                $"Dash listo: {(player.Velocity.Length() > 1200 ? "No" : "Si")}";

            spriteBatch.DrawString(
                hudFont,
                texto,
                new Vector2(20, 20),
                Color.White
            );

            float hpPercent = MathHelper.Clamp(player.Health / 100f, 0f, 1f);

            Color colVida = Color.Lerp(
                new Color(255, 180, 200),
                new Color(255, 0, 0),
                hpPercent
            );

            int barWidth = 250;
            int barHeight = 22;

            Rectangle fondo = new Rectangle(20, 140, barWidth, barHeight);
            Rectangle barra = new Rectangle(20, 140, (int)(barWidth * hpPercent), barHeight);

            spriteBatch.Draw(pixel, fondo, Color.Black * 0.5f);
            spriteBatch.Draw(pixel, barra, colVida);

            spriteBatch.DrawString(
                hudFont,
                $"{player.Health}/100",
                new Vector2(25, 140 - 25),
                Color.White
            );
        }
    }
}
