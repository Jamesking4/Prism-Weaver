using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PrismWeaver.Content.textures;

namespace PrismWeaver.Content;

public class Player : GameObject
{
    private GraphicsDeviceManager graphics;
    private List<GameObject> gameObjects;
    
    private Vector2 velocity;
    private int health = 10;
    private readonly PlayerAnimation playerAnimation;
    
    private const float MaxVelocityX = 4.0f;
    private const float DiffVelocityX = 0.1f;
    private const float Epsilon = 0.01f;
    private readonly Gravitation gravitation = new();
    private const double JumpCooldown = 0.9;
    private double timer;
    private bool isCanJump = true;
    
    public bool IsGrounded { get; private set; }
    
    public Player(ContentManager content, Vector2 startPosition)
        : base(startPosition, PlayerAnimation.RealFrameWidth, PlayerAnimation.RealFrameHeight)
    {
        timer = 0;
        velocity = Vector2.Zero;
        playerAnimation = new PlayerAnimation(content);
    }

    public void Initialize(GraphicsDeviceManager graphics, List<GameObject> gameObjects)
    {
        this.graphics = graphics;
        this.gameObjects = gameObjects;
    }
    
    public override void Update(GameTime gameTime)
    {
        CorrectVelocity();
        FindIsGrounded();
        Move();
        
        (Position, velocity) = Window.GetPositionAndVelocityInWindow(graphics, Rectangle, velocity);
        
        playerAnimation.Update(gameTime, velocity, IsGrounded);
        ChangeJumpCooldown(gameTime);
    }
    
    public override void Draw(SpriteBatch spriteBatch)
    {
        playerAnimation.Draw(spriteBatch, Position);
    }
    
    private void Move()
    {
        if (velocity.X > MaxVelocityX)
            velocity.X = MaxVelocityX;
        if (velocity.X < -MaxVelocityX)
            velocity.X = -MaxVelocityX;
        
        foreach (var gameObject in gameObjects)
        {
            if (gameObject is Player or Light)
                continue;
            
            if (!CollisionRectangle.Intersects(gameObject.CollisionRectangle)) 
                continue;
            
            if ((CollisionRectangle.Bottom <
                 gameObject.CollisionRectangle.Top + gameObject.CollisionRectangle.Height / 2)
                && !(CollisionRectangle.IsPlatformLeft(gameObject.CollisionRectangle) ||
                    CollisionRectangle.IsPlatformRight(gameObject.CollisionRectangle)))
            {
                Position = new Vector2(Position.X, gameObject.CollisionRectangle.Top - Height);
                velocity.Y = 0;
            }
            else if ((CollisionRectangle.Top >
                      gameObject.CollisionRectangle.Bottom - gameObject.CollisionRectangle.Height / 2)
                     && !(CollisionRectangle.IsPlatformLeft(gameObject.CollisionRectangle) ||
                         CollisionRectangle.IsPlatformRight(gameObject.CollisionRectangle)))
            {
                Position = new Vector2(Position.X, gameObject.CollisionRectangle.Bottom);
                velocity.Y = 0;
            }
            else if (CollisionRectangle.Right < gameObject.CollisionRectangle.Right - gameObject.CollisionRectangle.Width / 2)
            {
                Position = new Vector2(gameObject.CollisionRectangle.Left - CollisionRectangle.Width, Position.Y);
                velocity.X = 0;
            }
            else if (CollisionRectangle.Left > gameObject.CollisionRectangle.Right - gameObject.CollisionRectangle.Width / 2)
            {
                Position = new Vector2(gameObject.CollisionRectangle.Right, Position.Y);
                velocity.X = 0;
            }
        }
        
        Position = new Vector2(Position.X + velocity.X, Position.Y + velocity.Y);
    }

    public void MoveRight()
    {
        // var platformRects = gameObjects
        //     .Where(obj => obj is not Player && obj is not Light)
        //     .Select(p => p.CollisionRectangle)
        //     .ToList();
        // // if (platformRects.Any(rect => CollisionRectangle.IsPlatformLeft(rect) || CollisionRectangle.IsPlatformRight(rect)))
        // // {
        // //     velocity.X = 0;
        // //     return;
        // // }
        velocity.X += DiffVelocityX;
    }

    public void MoveLeft()
    {
        // var platformRects = gameObjects
        //     .Where(obj => obj is not Player && obj is not Light)
        //     .Select(p => p.CollisionRectangle)
        //     .ToList();
        // // if (platformRects.Any(rect => CollisionRectangle.IsPlatformLeft(rect) || CollisionRectangle.IsPlatformRight(rect)))
        // // {
        // //     velocity.X = 0;
        // //     return;
        // // }
        velocity.X -= DiffVelocityX;
    }

    public void Jump()
    {
        if (!IsGrounded || !isCanJump)
            return;
        
        var distToPlatformOver = 0;
        var minDistToPlatformOver = int.MaxValue;
        foreach (var platform in gameObjects)
        {
            distToPlatformOver = CollisionRectangle.GetDistToPlatformOver(platform.CollisionRectangle);
            if (distToPlatformOver != -1)
            {
                minDistToPlatformOver = Math.Min(distToPlatformOver, minDistToPlatformOver);
            }
        }
        
        Position = new Vector2(Position.X, Position.Y - 5);
        velocity.Y = -GetJumpVelocity(Math.Min(128, minDistToPlatformOver));
        isCanJump = false;
    }

    public void MoveNone()
    {
        if (Math.Abs(velocity.X) < 2 * DiffVelocityX)
        {
            velocity.X = 0;
        }
        else if (velocity.X > Epsilon)
        {
            velocity.X -= DiffVelocityX;
            playerAnimation.playerDirection = PlayerDirection.Right;
        }
        else if (velocity.X < -Epsilon)
        {
            velocity.X += DiffVelocityX;
            playerAnimation.playerDirection = PlayerDirection.Left;
        }
    }

    private void CorrectVelocity()
    {
        var platformRects = gameObjects
            .Where(obj => obj is not Player && obj is not Light)
            .Select(p => p.CollisionRectangle)
            .ToList();
        if (platformRects.Any(rect => CollisionRectangle.IsPlatformUp(rect)))
        {
            velocity.Y = 1f;
        }
        velocity = gravitation.AffectGravitation(graphics, velocity, CollisionRectangle, platformRects);
    }

    private void FindIsGrounded()
    {
        IsGrounded = gravitation.IsGrounded(graphics, CollisionRectangle,
            gameObjects
                .Where(obj => obj is not Player && obj is not Light)
                .Select(p => p.CollisionRectangle).ToList());
    }
    
    private float GetJumpVelocity(float height)
    {
        return (float)Math.Sqrt(height / 40 * gravitation.GetGravitationConst() * Height);
    }

    private void ChangeJumpCooldown(GameTime gameTime)
    {
        timer += gameTime.ElapsedGameTime.TotalSeconds;
        if (timer >= JumpCooldown)
        {
            timer = 0;
            isCanJump = true;
        }
    }
    
    public void TakeDamage(int damage) => health -= damage;
    public void Heal(int heal) => health += heal;
    public int GetHealth => health;
}