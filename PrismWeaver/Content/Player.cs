
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PrismWeaver.Content.textures;
using Vector2 = System.Numerics.Vector2;

namespace PrismWeaver.Content;

public class Player
{
    private Vector2 position;
    private Vector2 velocity;
    private int health = 10;
    PlayerAnimation playerAnimation;
    
    public Rectangle drawRectangle => new((int)position.X, (int)position.Y, widthPlayer, heightPlayer);
    private int widthPlayer = PlayerAnimation.RealFrameWidth;
    private int heightPlayer = PlayerAnimation.RealFrameHeight;
    
    private const float maxVelocityX = 4.0f;
    private const float diffVelocityX = 0.1f;

    private const float epsilon = 0.01f;
    Gravitation gravitation =  new();
    private const double jumpСooldown = 0.9;
    private double timer;
    private bool isCanJump = true;
    public bool IsGrounded {  get; private set; }

    public Player(ContentManager content, Vector2 position)
    {
        this.position = position;
        timer = 0;
        velocity = Vector2.Zero;
        playerAnimation = new PlayerAnimation(content); 
    }

    public void Update(GameTime gameTime, GraphicsDeviceManager graphics, List<Platform> platforms)
    {
        CorrectVelocity(graphics, platforms);
        FindIsGrounded(graphics, platforms);
        Move(platforms);
        (position, velocity) = Window.GetPositionAndVelocityInWindow(graphics, drawRectangle, velocity);
        playerAnimation.Update(gameTime, velocity, IsGrounded);
        ChangeJumpCooldown(gameTime);
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        playerAnimation.Draw(spriteBatch, position);
    }
    
    public void Move(List<Platform> platforms)
    {
        if (velocity.X > maxVelocityX)
            velocity.X = maxVelocityX;

        if (velocity.X < -maxVelocityX)
            velocity.X = -maxVelocityX;
        
        foreach (var platform in platforms)
        {
            if (drawRectangle.Intersects(platform.drawRectangle))
            {
                if (drawRectangle.Bottom < platform.drawRectangle.Top + platform.drawRectangle.Height / 2)
                {
                    position.Y = Math.Min(drawRectangle.Bottom, platform.drawRectangle.Top) - heightPlayer;
                    velocity.Y = 0;
                }
                else if (drawRectangle.Top > platform.drawRectangle.Bottom - platform.drawRectangle.Height / 2)
                {
                    position.Y = Math.Max(drawRectangle.Bottom, platform.drawRectangle.Top);
                    velocity.Y = 0;
                }
            }
        }
        
        position.X += velocity.X;
        position.Y += velocity.Y;
    }

    public void MoveRight(List<Platform> platforms)
    {
        var platformRects = platforms
            .Select(platform => platform.drawRectangle)
            .ToList();
        if (platformRects
            .Any(rect => drawRectangle.IsPlatformLeft(rect) || drawRectangle.IsPlatformRight(rect)))
        {
            velocity.X = 0;
            return;
        }
        velocity.X += diffVelocityX;
    }

    public void MoveLeft(List<Platform> platforms)
    {
        var platformRects = platforms
            .Select(platform => platform.drawRectangle)
            .ToList();
        if (platformRects
            .Any(rect => drawRectangle.IsPlatformLeft(rect) || drawRectangle.IsPlatformRight(rect)))
        {
            velocity.X = 0;
            return;
        }
        velocity.X -= diffVelocityX;
    }

    public void Jump(List<Platform> platforms)
    {
        if (!IsGrounded || !isCanJump)
            return;
        var distToPlatformOver = 0;
        var minDistToPlatformOver = int.MaxValue;
        foreach (var platform in platforms)
        {
            distToPlatformOver = drawRectangle.GetDistToPlatformOver(platform.drawRectangle);
            if (distToPlatformOver != -1)
            {
                minDistToPlatformOver = Math.Min(distToPlatformOver, minDistToPlatformOver);
            }
        }
        
        position.Y -= 5;
        velocity.Y = -GetJumpVelocity(Math.Min(128, minDistToPlatformOver));
        isCanJump = false;
    }

    public void MoveNone(List<Platform> platforms)
    {
        if (Math.Abs(velocity.X) < 2 * diffVelocityX)
        {
            velocity.X = 0;
        }
        
        else switch (velocity.X)
        {
            case > epsilon:
                velocity.X -= diffVelocityX;
                playerAnimation.playerDirection = PlayerDirection.Right;
                break;
            case < -epsilon:
                velocity.X += diffVelocityX;
                playerAnimation.playerDirection = PlayerDirection.Left;
                break;
        }
    }

    private void CorrectVelocity(GraphicsDeviceManager graphics, List<Platform> platforms)
    {
        var platformRects = platforms
            .Select(platform => platform.drawRectangle)
            .ToList();

        if (platformRects.Any(rect => drawRectangle.IsPlatformUp(rect)))
        {
            velocity.Y = 1f;
        }
        
        velocity = gravitation.AffectGravitation(graphics, velocity, drawRectangle, platformRects);
    }

    private void FindIsGrounded(GraphicsDeviceManager graphics, List<Platform> platforms)
    {
        IsGrounded = gravitation.IsGrounded(graphics, drawRectangle, platforms
            .Select(platform => platform.drawRectangle)
            .ToList());
    }
    
    private float GetJumpVelocity(float height)
    {
        return (float)Math.Sqrt(height / 40 * gravitation.GetGravitationConst() * heightPlayer);
    }

    private void ChangeJumpCooldown(GameTime gameTime)
    {
        timer += gameTime.ElapsedGameTime.TotalSeconds;
        if (timer >= jumpСooldown)
        {
            timer = 0;
            isCanJump = true;
        }
    }
    
    public void TakeDamage(int damage)
    {
        health -= damage;
    }

    public void Heal(int heal)
    {
        health += heal;
    }
    
    public Vector2 GetPosition => position;
    public int GetHealth => health;
}