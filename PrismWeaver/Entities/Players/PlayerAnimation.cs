using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PrismWeaver.Utilities;

namespace PrismWeaver.Entities.Players;

public class PlayerAnimation
{
    private readonly ContentManager content;
    
    private readonly Dictionary<string, Animation> animations = new();
    private Animation currentAnimation;
    private string currentState;
    private SpriteEffects effects;
    
    public PlayerDirection PlayerDirection;

    public static int FrameWidth { get; } = 128;
    public static int FrameHeight { get; } = 128;
    public static int RealFrameWidth { get; } = 52;
    public static int RealFrameHeight { get; } = 70;
    private Point frameSize;

    private int countFrameIdle = 5;
    private int countFrameRun = 6;
    private int countFrameJump = 6;
    private int countFrameDie = 8;

    private float frameTimeIdle = 0.12f;
    private float frameTimeRun = 0.08f;
    private float frameTimeJump = 0.09f;
    private float frameTimeDie = 0.1f;
    
    public PlayerAnimation(ContentManager content, Point frameSize)
    {
        this.content = content;
        this.frameSize = frameSize;
        animations.Add("idle", CreateFrame("textures/Countess_Vampire/Idle", countFrameIdle, frameTimeIdle, true));
        animations.Add("run", CreateFrame("textures/Countess_Vampire/Run", countFrameRun, frameTimeRun, true));
        animations.Add("jump", CreateFrame("textures/Countess_Vampire/Jump", countFrameJump, frameTimeJump, false));
        animations.Add("die", CreateFrame("textures/Countess_Vampire/Dead", countFrameDie, frameTimeDie, false));
        
        currentState = "idle";
        currentAnimation = animations[currentState];
    }

    private Animation CreateFrame(string path, int countFrame, float frameTime, bool isLooping)
    {
        var texture = content.Load<Texture2D>(path);
        return new Animation(texture, frameSize, countFrame, frameTime, isLooping);
    }

    public void Update(GameTime gameTime, Vector2 velocity, bool isGrounded, bool canMove)
    {
        currentAnimation.Update(gameTime);
        
        string newState;
        
        if (!isGrounded || velocity.Y != 0)
            newState = "jump";
        else if (Math.Abs(velocity.X) > 0.3f && canMove)
            newState = "run";
        else
            newState = "idle";
        
        if (velocity.X > 0)
            PlayerDirection = PlayerDirection.Right;
        else if (velocity.X < 0)
            PlayerDirection = PlayerDirection.Left;
        
        if (currentState != newState)
            ChangeState(newState);
    }
    
    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        effects = PlayerDirection == PlayerDirection.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        currentAnimation.Draw(spriteBatch, position, effects);
    }
    
    private void ChangeState(string newState)
    {
        if (currentState == newState) return;
    
        currentAnimation.Reset();
        currentState = newState;
        currentAnimation = animations[currentState];
    }
}