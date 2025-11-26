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
        
        Texture2D pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        // RECTÁNGULO DE PRUEBA: 100x100 centrado en la pantalla
        item3 = new Item(
            pixel,                      // textura 1x1
            new Rectangle(0, 0, 1, 1),  // source 1x1
            new Rectangle(
                _graphics.PreferredBackBufferWidth / 2 - 50,
                _graphics.PreferredBackBufferHeight / 2 - 50,
                100,
                100
            )
        );
        item3.color = Color.Red; // para verlo bien

        Texture2D t = ContentLoader.Instance.LoadImage("Content/Another Metroidvania Asset Pack Vol. 1 ver. 1.5/Keys/scenes_key_idle.png");
        item = new Item(t, new(0,0,16,16), new(
            _graphics.PreferredBackBufferWidth/2 - 50,
            _graphics.PreferredBackBufferHeight/2 - 50,
            100,
            100)
        );

        item2 = new Item(t, new(0,0,16,16), new(
            _graphics.PreferredBackBufferWidth/2 - 50,
            _graphics.PreferredBackBufferHeight/2 - 50,
            100,
            100)
        );
        tileMap.GetBlocks("tiled/test1.csv");
        t_world = ContentLoader.Instance.LoadImage("Content/Another Metroidvania Asset Pack Vol. 1 ver. 1.5/Tilesets/library/tileset_library.png");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        sceneManager.getScene().Update(gameTime);
        item.Update(gameTime);
        item3.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: Camera.Instance.Matrix);
        
        sceneManager.getScene().Draw(gameTime, _spriteBatch);


        item3.Draw(
            gameTime: gameTime,
            spriteBatch: _spriteBatch
        );

        item.Draw(
            gameTime: gameTime,
            spriteBatch: _spriteBatch
        );
        item2.Draw(_spriteBatch, gameTime);
        tileMap.Draw(t_world, gameTime, _spriteBatch);

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
