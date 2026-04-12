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
    
    public PlayerAnimation(ContentManager content)
    {
        this.content = content;
        animations.Add("idle", CreateFrame("textures/Countess_Vampire/Idle", Config.CountFrameIdle, Config.FrameTimeIdle, true));
        animations.Add("run", CreateFrame("textures/Countess_Vampire/Run", Config.CountFrameRun, Config.FrameTimeRun, true));
        animations.Add("jump", CreateFrame("textures/Countess_Vampire/Jump", Config.CountFrameJump, Config.FrameTimeJump, false));
        animations.Add("die", CreateFrame("textures/Countess_Vampire/Dead", Config.CountFrameDie, Config.FrameTimeDie, false));
        
        currentState = "idle";
        currentAnimation = animations[currentState];
    }

    private Animation CreateFrame(string path, int countFrame, float frameTime, bool isLooping)
    {
        var texture = content.Load<Texture2D>(path);
        return new Animation(texture, Config.FrameSize, countFrame, frameTime, isLooping);
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