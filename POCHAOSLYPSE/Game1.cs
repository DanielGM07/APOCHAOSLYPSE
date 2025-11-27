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
    public Item item3;
    private TileMap tileMap = new(false, false);
    private Texture2D t_world;

    // 🔹 Nuevo: textura 1x1 para rectángulos (player, armas, balas)
    private Texture2D pixel;

    // 🔹 Nuevo: player y armas
    private List<Sprite> sprites;
    private Player player;
    private Weapon ak47;
    private Weapon shotgun;
    private Weapon rocketLauncher;
    private Weapon katana;

    private Weapon currentWeapon;

    // 🔹 Nuevo: lista de proyectiles
    private readonly List<Projectile> projectiles = new();


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

        pixel = new Texture2D(GraphicsDevice, 1, 1); // mala practica
        pixel.SetData(new[] { Color.White });
        Rectangle pixelRectangle = new(0,0,1,1);

        item3 = new Item(
            pixel,
            pixelRectangle,
            new Rectangle(
                50,
                50,
                100,
                100
            ),
            Color.White
        );
        item3.color = Color.Red; // para verlo bien

        Texture2D t = ContentLoader.Instance.LoadImage("Content/Another Metroidvania Asset Pack Vol. 1 ver. 1.5/Keys/scenes_key_idle.png");
        item = new Item(t,
            pixelRectangle,
            new(
              _graphics.PreferredBackBufferWidth / 2 - 50,
              _graphics.PreferredBackBufferHeight / 2 - 50,
              100,
              100
            ),
            Color.White
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
            pixelRectangle,
            playerRect,
            Color.White
        );
        player.color = Color.Black;

        // 🔹 Armas como rectángulos de distintos colores
        ak47 = new AK47(
            texture: pixel,
            pixelRectangle,
            destRect: new Rectangle(0, 0, 50, 10),
            Color.Yellow
        );

        shotgun = new Shotgun(
            texture: pixel,
            pixelRectangle,
            destRect: new Rectangle(0, 0, 60, 12),
            Color.OrangeRed
        );
        shotgun.color = Color.OrangeRed;

        rocketLauncher = new RocketLauncher(
            texture: pixel,
            pixelRectangle,
            destRect: new Rectangle(0, 0, 60, 14),
            Color.LimeGreen
        );

        katana = new Katana(
            texture: pixel,
            pixelRectangle,
            destRect: new Rectangle(0, 0, 10, 50),
            Color.MediumPurple
        );

        // Arma inicial
        currentWeapon = shotgun;

        // adding everything to a list to make easier things there
        sprites = new();
        sprites.Add(item);
        sprites.Add(item3);
        sprites.Add(ak47);
        sprites.Add(player);
        sprites.Add(rocketLauncher);
        sprites.Add(katana);
        sprites.Add(shotgun);
    }

    protected override void Update(GameTime gameTime)
    {
        var mouse = Mouse.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        sceneManager.getScene().Update(gameTime);

        // Items de prueba
        item.Update(gameTime);
        item3.Update(gameTime);

        // 🔹 Actualizar player (movimiento libre)
        player.Update(gameTime);

        // 🔹 Dirección de apuntado (player -> mouse, en mundo)
        Vector2 mouseWorld = Camera.Instance.ScreenToCamera(mouse.Position.ToVector2());
        Vector2 aimDir = mouseWorld - player.Center;
        if (aimDir != Vector2.Zero)
            aimDir.Normalize();
        else
            aimDir = Vector2.UnitX;

        if (Keyboard.GetState().IsKeyDown(Keys.D1)) currentWeapon = ak47;
        if (Keyboard.GetState().IsKeyDown(Keys.D2)) currentWeapon = shotgun;
        if (Keyboard.GetState().IsKeyDown(Keys.D3)) currentWeapon = rocketLauncher;
        if (Keyboard.GetState().IsKeyDown(Keys.D4)) currentWeapon = katana;

        Vector2 weaponOffset = aimDir * 30f;
        currentWeapon.position = player.Center + weaponOffset;
        currentWeapon.Update(gameTime);

        if (mouse.LeftButton == ButtonState.Pressed)
        {
            Vector2 muzzle = player.Center + aimDir * 50f;
            currentWeapon.Fire(muzzle, aimDir, projectiles, player);
        }

        for (int i = 0; i < projectiles.Count; i++)
            projectiles[i].Update(gameTime);

        projectiles.RemoveAll(p => !p.IsAlive);

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

        // Tilemap
        tileMap.Draw(t_world, gameTime, _spriteBatch);

        // 🔹 Player
        player.Draw(_spriteBatch, gameTime);

        currentWeapon.Draw(_spriteBatch, gameTime);

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
