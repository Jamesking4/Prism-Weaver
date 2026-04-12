using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using PrismWeaver.Core;
using PrismWeaver.Entities.Interactive;
using PrismWeaver.Entities.Lights;
using PrismWeaver.Entities.Platforms;
using PrismWeaver.Entities.Players;
using PrismWeaver.Utilities;

namespace PrismWeaver;

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private Texture2D background;
    private Texture2D platformTexture;
    private Texture2D pixelTexture;
    private Song backgroundMusic;
    
    private Player player;
    private List<GameObject> gameObjects = [];
    
    private KeyboardState previousKeyboardState;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        graphics.PreferredBackBufferWidth = GameConfig.WindowWidth;
        graphics.PreferredBackBufferHeight = GameConfig.WindowHeight;
        graphics.IsFullScreen = GameConfig.StartFullScreen;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        LoadTextures();
        LoadAudio();
        CreatePlatforms();
        CreateLightSource();
        CreateGlassBlocks();
        CreatePlayer();
    }

    private void LoadTextures()
    {
        background = Content.Load<Texture2D>("textures/background");
        platformTexture = Content.Load<Texture2D>("textures/platform");
        pixelTexture = CreatePixelTexture(Color.White);
    }

    private Texture2D CreatePixelTexture(Color color)
    {
        var texture = new Texture2D(GraphicsDevice, 1, 1);
        texture.SetData([color]);
        return texture;
    }

    private void LoadAudio()
    {
        backgroundMusic = Content.Load<Song>("sounds/background_music");
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = GameConfig.MusicVolume;
        MediaPlayer.Play(backgroundMusic);
    }

    private void CreatePlatforms()
    {
        var startX = GameConfig.PlatformOffsetX * GameConfig.PlatformStartIndex;
        var y = graphics.GraphicsDevice.Viewport.Height - GameConfig.PlatformYOffsetFromBottom;
        
        for (var i = 0; i < GameConfig.PlatformCount; i++)
        {
            var position = new Vector2(startX + GameConfig.PlatformOffsetX * i, y);
            var platform = new Platform(position, platformTexture);
            gameObjects.Add(platform);
        }
    }

    private void CreateLightSource()
    {
        var viewport = graphics.GraphicsDevice.Viewport;
        var lightPosition = new Vector2(
            viewport.Width / 2f,
            viewport.Height - GameConfig.LightSourceOffsetFromBottom.Y
        );
        
        var tempTexture = new Texture2D(GraphicsDevice, 1, 1);
        var lightSource = new LightSource(lightPosition, GameConfig.LightSourceSize, GameConfig.LightSourceSize);
        lightSource.Initialize(graphics, tempTexture, GameConfig.LightSourceColor, Direction.Left, pixelTexture, gameObjects);
        
        gameObjects.Add(lightSource);
        gameObjects.Add(lightSource.GetLight());
    }

    private void CreateGlassBlocks()
    {
        var viewport = graphics.GraphicsDevice.Viewport;
        var y = viewport.Height - GameConfig.GlassBlockSize - GameConfig.PlatformYOffsetFromBottom;
        
        var glassTexture = CreatePixelTexture(GameConfig.GlassBlockOutlineColor);
        
        var block1 = new GlassBlock(graphics, new Vector2(GameConfig.GlassBlock1Position.X, y), 
            GameConfig.GlassBlockSize, GameConfig.GlassBlockSize, gameObjects, GameConfig.GlassBlockMaxSpeed);
        block1.Initialize(GameConfig.GlassBlockColor, glassTexture);
        gameObjects.Add(block1);
        
        var block2 = new GlassBlock(graphics, new Vector2(GameConfig.GlassBlock2Position.X, y), 
            GameConfig.GlassBlockSize, GameConfig.GlassBlockSize, gameObjects, GameConfig.GlassBlockMaxSpeed);
        block2.Initialize(GameConfig.GlassBlockColor, glassTexture);
        gameObjects.Add(block2);
    }

    private void CreatePlayer()
    {
        player = new Player(graphics, Content, GameConfig.PlayerStartPosition, gameObjects, 
            Config.FrameWidth, Config.FrameHeight);
        gameObjects.Add(player);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        
        if (ShouldExit(keyboard))
            Exit();
        
        ToggleFullScreen(keyboard);
        ControlPlayer(keyboard);
        
        foreach (var obj in gameObjects)
            obj.Update(gameTime);
        
        previousKeyboardState = keyboard;
        base.Update(gameTime);
    }

    private bool ShouldExit(KeyboardState keyboard)
    {
        return GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
               keyboard.IsKeyDown(Keys.Escape);
    }

    private void ToggleFullScreen(KeyboardState keyboard)
    {
        if (keyboard.IsKeyDown(Keys.F11) && !previousKeyboardState.IsKeyDown(Keys.F11))
        {
            graphics.IsFullScreen = !graphics.IsFullScreen;
            graphics.ApplyChanges();
        }
    }

    private void ControlPlayer(KeyboardState keyboard)
    {
        if (keyboard.IsKeyDown(Keys.A))
            player.MoveLeft();
        if (keyboard.IsKeyDown(Keys.D))
            player.MoveRight();
        if (keyboard.IsKeyDown(Keys.Space) && !previousKeyboardState.IsKeyDown(Keys.Space))
            player.Jump();

        if (!keyboard.IsKeyDown(Keys.A) && !keyboard.IsKeyDown(Keys.D))
            player.MoveNone();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        spriteBatch.Begin();
        
        DrawBackground();
        
        foreach (var obj in gameObjects)
            obj.Draw(spriteBatch);
        
        spriteBatch.End();
        
        base.Draw(gameTime);
    }

    private void DrawBackground()
    {
        var viewport = graphics.GraphicsDevice.Viewport;
        var destination = new Rectangle(0, 0, viewport.Width, viewport.Height);
        spriteBatch.Draw(background, destination, Color.White);
    }
}