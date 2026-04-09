using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using PrismWeaver.Content;
using Vector2 = System.Numerics.Vector2;

namespace PrismWeaver;

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private Texture2D background;
    private Texture2D platformTexture;
    private Texture2D pixelTexture;
    private Song background_music;
    
    private Player player;
    private List<GameObject> gameObjects = [];
    
    private int windowWidth;
    private int windowHeight;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        graphics.PreferredBackBufferWidth = 1080;
        graphics.PreferredBackBufferHeight = 720;
        graphics.IsFullScreen = true;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
        windowWidth = graphics.GraphicsDevice.Viewport.Width;
        windowHeight = graphics.GraphicsDevice.Viewport.Height;
        
        for (var i = 0; i < 8; i++)
        {
            var platform = new Platform(new Vector2(100 * (i + 2), windowHeight - 120), pixelTexture);
            gameObjects.Add(platform);
        }
        
        player = new Player(Content, new Vector2(0, 800));
        player.Initialize(graphics, GetPlatforms());
        gameObjects.Add(player);
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        background = Content.Load<Texture2D>("textures/background");
        platformTexture = Content.Load<Texture2D>("textures/platform");
        background_music = Content.Load<Song>("sounds/background_music");
        pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        pixelTexture.SetData([Color.White]);

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 1f;
        MediaPlayer.Play(background_music);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            keyboard.IsKeyDown(Keys.Escape))
            Exit();
        
        ControlPlayer(keyboard);
        
        foreach (var obj in gameObjects)
            obj.Update(gameTime);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        spriteBatch.Begin();
        
        spriteBatch.Draw(background, new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width,
            graphics.GraphicsDevice.Viewport.Height), Color.White);
        
        foreach (var obj in gameObjects)
            obj.Draw(spriteBatch);
        
        spriteBatch.End();
        base.Draw(gameTime);
    }

    private void ControlPlayer(KeyboardState keyboard)
    {
        if (keyboard.IsKeyDown(Keys.D))
            player.MoveRight();
        
        if (keyboard.IsKeyDown(Keys.A))
            player.MoveLeft();
        
        if (keyboard.IsKeyDown(Keys.Space))
            player.Jump();

        if (!keyboard.IsKeyDown(Keys.A) && !keyboard.IsKeyDown(Keys.D))
        {
            player.MoveNone();
        }
    }
    
    private List<Platform> GetPlatforms()
    {
        return gameObjects
            .Where(obj => obj is Platform)
            .Cast<Platform>()
            .ToList();
    }
}