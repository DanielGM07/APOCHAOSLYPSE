using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SceneManager sceneManager;

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
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        sceneManager.getScene().LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        sceneManager.getScene().Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: Camera.Instance.Matrix);
        
        sceneManager.getScene().Draw(gameTime, _spriteBatch);

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
