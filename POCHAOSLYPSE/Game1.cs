using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SceneManager sceneManager;
    public Item item;
    public Item item2;
    public Item item3;
    private TileMap tileMap = new(false, false);
    private Texture2D t_world;

    // 🔹 Nuevo: textura 1x1 para rectángulos (player, armas, balas)
    private Texture2D pixel;

    // 🔹 Nuevo: player y armas
    private Player player;
    private Weapon ak47;
    private Weapon shotgun;
    private Weapon rocketLauncher;
    private Weapon katana;
    private Weapon currentWeapon;

    // 🔹 Nuevo: lista de proyectiles
    private readonly List<Projectile> projectiles = new();

    // 🔹 Nuevo: estados de input previo
    private KeyboardState previousKeyboard;
    private MouseState previousMouse;

    // 🔹 Nuevo: fuente para HUD
    private SpriteFont hudFont;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Camera.Initialize(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        sceneManager = new();
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        ContentLoader.Initialize(_graphics, Content, "Content/Another Metroidvania Asset Pack Vol. 1 ver. 1.5"); // esta cosa va a tardar milenios
        sceneManager.AddScene(new MenuScene());
        base.Initialize();
    }

    protected override void LoadContent()
    {
        sceneManager.getScene().LoadContent();
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // 🔹 HUD font (asegurate de tener un SpriteFont llamado "DefaultFont")
        hudFont = Content.Load<SpriteFont>("font");

        // Textura 1x1 blanca reutilizable
        pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        // RECTÁNGULO DE PRUEBA: 100x100 centrado en la pantalla
        item3 = new Item(
            pixel,                      // textura 1x1
            new Rectangle(0, 0, 1, 1),  // source 1x1
            new Rectangle(
                50,
                50,
                100,
                100
            )
        );
        item3.color = Color.Red; // para verlo bien

        Texture2D t = ContentLoader.Instance.LoadImage("Content/Another Metroidvania Asset Pack Vol. 1 ver. 1.5/Keys/scenes_key_idle.png");
        item = new Item(t, new(0, 0, 16, 16), new(
            _graphics.PreferredBackBufferWidth / 2 - 50,
            _graphics.PreferredBackBufferHeight / 2 - 50,
            100,
            100)
        );

        item2 = new Item(t, new(0, 0, 16, 16), new(
            _graphics.PreferredBackBufferWidth / 2 - 50,
            _graphics.PreferredBackBufferHeight / 2 - 50,
            100,
            100)
        );

        tileMap.GetBlocks("tiled/test1.csv");
        t_world = ContentLoader.Instance.LoadImage("Content/Another Metroidvania Asset Pack Vol. 1 ver. 1.5/Tilesets/library/tileset_library.png");

        // 🔹 Player como rectángulo simple
        var playerRect = new Rectangle(
            _graphics.PreferredBackBufferWidth / 2 - 16,
            _graphics.PreferredBackBufferHeight / 2 - 16,
            32,
            32
        );
        player = new Player(
            pixel,
            new Rectangle(0, 0, 1, 1),
            playerRect
        );
        player.color = Color.Black;

        // 🔹 Armas como rectángulos de distintos colores
        ak47 = new AK47(
            texture: pixel,
            srcRec: new Rectangle(0, 0, 1, 1),
            destRect: new Rectangle(0, 0, 50, 10)
        );
        ak47.color = Color.Yellow;

        shotgun = new Shotgun(
            texture: pixel,
            srcRec: new Rectangle(0, 0, 1, 1),
            destRect: new Rectangle(0, 0, 60, 12)
        );
        shotgun.color = Color.OrangeRed;

        rocketLauncher = new RocketLauncher(
            texture: pixel,
            srcRec: new Rectangle(0, 0, 1, 1),
            destRect: new Rectangle(0, 0, 60, 14)
        );
        rocketLauncher.color = Color.LimeGreen;

        katana = new Katana(
            texture: pixel,
            srcRec: new Rectangle(0, 0, 1, 1),
            destRect: new Rectangle(0, 0, 10, 50)
        );
        katana.color = Color.MediumPurple;

        // Arma inicial
        currentWeapon = shotgun;
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Escape))
            Exit();

        // Escenas (no se tocan)
        sceneManager.getScene().Update(gameTime);

        // Items de prueba
        item.Update(gameTime);
        item3.Update(gameTime);
        // (item2 no se actualizaba en tu código original, lo dejo así como estaba)

        // 🔹 Actualizar player (movimiento libre)
        player.Update(keyboard, previousKeyboard, gameTime);

        // 🔹 Dirección de apuntado (player -> mouse, en mundo)
        Vector2 mouseWorld = Camera.Instance.ScreenToCamera(mouse.Position.ToVector2());
        Vector2 aimDir = mouseWorld - player.Center;
        if (aimDir != Vector2.Zero)
            aimDir.Normalize();
        else
            aimDir = Vector2.UnitX;

        // 🔹 Cambio de arma (1–4)
        if (keyboard.IsKeyDown(Keys.D1)) currentWeapon = ak47;
        if (keyboard.IsKeyDown(Keys.D2)) currentWeapon = shotgun;
        if (keyboard.IsKeyDown(Keys.D3)) currentWeapon = rocketLauncher;
        if (keyboard.IsKeyDown(Keys.D4)) currentWeapon = katana;

        // 🔹 “Atar” el arma al player: posición = centro del player + offset hacia donde apunta
        Vector2 weaponOffset = aimDir * 30f;
        if (currentWeapon is Item weaponItem)
        {
            weaponItem.position = player.Center + weaponOffset;
            weaponItem.Update(gameTime); // rota hacia el mouse en Item.Update
        }

        // 🔹 Disparo con botón izquierdo (FireRate se maneja dentro de cada arma)
        bool shootPressed = mouse.LeftButton == ButtonState.Pressed;
        if (shootPressed && currentWeapon != null)
        {
            // Muzzle un poco adelante del arma
            Vector2 muzzle = player.Center + aimDir * 50f;
            currentWeapon.Fire(muzzle, aimDir, projectiles, player);
        }

        // 🔹 Actualizar proyectiles
        for (int i = 0; i < projectiles.Count; i++)
            projectiles[i].Update(gameTime);

        projectiles.RemoveAll(p => !p.IsAlive);

        previousKeyboard = keyboard;
        previousMouse = mouse;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: Camera.Instance.Matrix);

        // Escena actual
        sceneManager.getScene().Draw(gameTime, _spriteBatch);

        // Items de prueba
        item3.Draw(
            gameTime: gameTime,
            spriteBatch: _spriteBatch
        );

        item.Draw(
            gameTime: gameTime,
            spriteBatch: _spriteBatch
        );

        item2.Draw(_spriteBatch, gameTime);

        // Tilemap
        tileMap.Draw(t_world, gameTime, _spriteBatch);

        // 🔹 Player
        player.Draw(_spriteBatch, gameTime);

        // 🔹 Arma actual (rectángulo rotando hacia el mouse)
        if (currentWeapon is Sprite weaponSprite)
        {
            weaponSprite.Draw(_spriteBatch, gameTime);
        }

        // 🔹 Proyectiles
        foreach (var proj in projectiles)
        {
            proj.Draw(_spriteBatch, pixel);
        }

        // 🔹 HUD: nombre del arma actual arriba a la izquierda
        if (hudFont != null && currentWeapon != null)
        {
            string weaponName = currentWeapon.GetType().Name;
            _spriteBatch.DrawString(
                hudFont,
                $"Arma actual: {weaponName}",
                new Vector2(10, 10),   // como usamos la matriz de la cámara, esto está en mundo (0,0)
                Color.White
            );
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
