using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using PrismWeaver.Core;
using PrismWeaver.Entities.Exit;
using PrismWeaver.Entities.Interactive;
using PrismWeaver.Entities.Lights;
using PrismWeaver.Entities.Platforms;
using PrismWeaver.Entities.Players;
using PrismWeaver.Utilities;
using SharpDX.Direct3D9;
using Light = PrismWeaver.Entities.Lights.Light;

namespace PrismWeaver;

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private Texture2D background;
    private Texture2D platformTexture;
    private Texture2D pixelTexture;
    private Song backgroundMusic;
    
    private List<GameObject> gameObjects = [];
    private Stack<GameObject> newObjects = [];
    private HashSet<DynamicObject> dynamicObjects = [];
    private HashSet<Light> lights = [];
    private HashSet<GameObject> otherObjects = [];
    
    private Player player;
    
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
        CreatePrismBlocks();
        CreateMirror();
        CreateTargetAndDoor();
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
        lightSource.Initialize(graphics, tempTexture, GameConfig.LightSourceColor, Direction.Left, pixelTexture, gameObjects, newObjects);
        
        gameObjects.Add(lightSource);
        gameObjects.Add(lightSource.GetLight());
        
        var lightSource2 = new LightSource(lightPosition, GameConfig.LightSourceSize, GameConfig.LightSourceSize);
        lightSource2.Initialize(graphics, tempTexture, GameConfig.LightSourceColor, Direction.Right, pixelTexture, gameObjects, newObjects);
        
        gameObjects.Add(lightSource2);
        gameObjects.Add(lightSource2.GetLight());
    }

    private void CreateGlassBlocks()
    {
        var viewport = graphics.GraphicsDevice.Viewport;
        var y = viewport.Height - GameConfig.GlassBlockSize - GameConfig.PlatformYOffsetFromBottom;
        
        var glassTexture = CreatePixelTexture(GameConfig.GlassBlockOutlineColor);
        
        var block2 = new GlassBlock(graphics, new Vector2(GameConfig.GlassBlock2Position.X, y), 
            GameConfig.GlassBlockSize, GameConfig.GlassBlockSize, gameObjects, GameConfig.GlassBlockMaxSpeed);
        block2.Initialize(GameConfig.GlassBlockColor, glassTexture);
        gameObjects.Add(block2);
    }

    private void CreatePrismBlocks()
    {
        var viewport = graphics.GraphicsDevice.Viewport;
        var y = viewport.Height - GameConfig.GlassBlockSize - GameConfig.PlatformYOffsetFromBottom;
        
        var prismTexture = CreatePixelTexture(GameConfig.GlassBlockOutlineColor);
        
        var block1 = new PrismBlock(graphics, new Vector2(GameConfig.GlassBlock1Position.X + 300, y), 
            GameConfig.GlassBlockSize, GameConfig.GlassBlockSize, gameObjects);
        block1.Initialize(Color.Blue, prismTexture);
        gameObjects.Add(block1);
        
        var block2 = new PrismBlock(graphics, new Vector2(GameConfig.GlassBlock1Position.X, y), 
            GameConfig.GlassBlockSize, GameConfig.GlassBlockSize, gameObjects);
        block2.Initialize(Color.Green, prismTexture);
        gameObjects.Add(block2);
    }

    private void CreateMirror()
    {
        var viewport = graphics.GraphicsDevice.Viewport;
        var y = viewport.Height - GameConfig.GlassBlockSize - GameConfig.PlatformYOffsetFromBottom;
        
        var prismTexture = CreatePixelTexture(GameConfig.GlassBlockOutlineColor);
        
        var block1 = new Mirror(graphics, new Vector2(GameConfig.GlassBlock1Position.X + 500, y), 
            GameConfig.GlassBlockSize, GameConfig.GlassBlockSize, gameObjects, Direction.Up);
        block1.Initialize(Color.Black, prismTexture);
        gameObjects.Add(block1);
    }
    
    private void CreateTargetAndDoor()
    {
        var doorTexture = CreatePixelTexture(Color.Black);
        
        var door = new Door(
            startPosition: new Vector2(GameConfig.WindowWidth - 200,  GameConfig.WindowHeight - 128),
            width: 64,
            height: 128,
            doorTexture
        );
        
        var targetTexture = CreatePixelTexture(Color.White);
        
        var target = new Target(
            startPosition: new Vector2(GameConfig.WindowWidth - 300, 0),
            width: 32,
            height: 32,
            func: (isActivated) => door.SetIsOpen(isActivated),
            targetTexture,
            Color.White
        );
        
        gameObjects.Add(door);
        gameObjects.Add(target);
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
            if (obj is Target target)
                target.ResetBeamCount();
        
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
        if (keyboard.IsKeyDown(Keys.Space))
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
        {
            if (obj is DynamicObject dynamicObject)
                dynamicObjects.Add(dynamicObject);
            else if (obj is Light light)
                lights.Add(light);
            else if (obj is Player)
                continue;
            else 
                otherObjects.Add(obj);
        }
        
        foreach (var light in lights)
            light.Draw(spriteBatch);
        
        foreach (var dynObj in dynamicObjects)
            dynObj.Draw(spriteBatch);

        foreach (var obj in otherObjects)
            obj.Draw(spriteBatch);
        
        player.Draw(spriteBatch);
        
        while (newObjects.Count > 0)
            gameObjects.Add(newObjects.Pop());
        
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