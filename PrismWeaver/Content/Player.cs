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
    private Vector2 velocity;
    private int health = 10;
    private PlayerAnimation playerAnimation;
    
    private const float maxVelocityX = 4.0f;
    private const float diffVelocityX = 0.1f;
    private const float epsilon = 0.01f;
    private Gravitation gravitation = new();
    private const double jumpCooldown = 0.9;
    private double timer;
    private bool isCanJump = true;
    
    public bool IsGrounded { get; private set; }

    // Конструктор – передаём размеры из анимации в базовый класс
    public Player(ContentManager content, Vector2 startPosition)
        : base(startPosition, PlayerAnimation.RealFrameWidth, PlayerAnimation.RealFrameHeight)
    {
        timer = 0;
        velocity = Vector2.Zero;
        playerAnimation = new PlayerAnimation(content);
        
        // При необходимости настраиваем отдельный хитбокс для коллизий
        // Например, уменьшаем его на 4 пикселя с каждой стороны:
        // SetCollisionBox(new Vector2(4, 4), Width - 8, Height - 8);
    }

    // Метод обновления – теперь использует Position и Rectangle
    public void Update(GameTime gameTime, GraphicsDeviceManager graphics, List<Platform> platforms)
    {
        CorrectVelocity(graphics, platforms);
        FindIsGrounded(graphics, platforms);
        Move(platforms);
        
        // Ограничение позиции в окне (используем визуальный Rectangle)
        (Position, velocity) = Window.GetPositionAndVelocityInWindow(graphics, Rectangle, velocity);
        
        playerAnimation.Update(gameTime, velocity, IsGrounded);
        ChangeJumpCooldown(gameTime);
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        playerAnimation.Draw(spriteBatch, Position);
    }
    
    private void Move(List<Platform> platforms)
    {
        if (velocity.X > maxVelocityX)
            velocity.X = maxVelocityX;
        if (velocity.X < -maxVelocityX)
            velocity.X = -maxVelocityX;
        
        // Используем CollisionRectangle для коллизий (если не настроен отдельно, он равен Rectangle)
        foreach (var platform in platforms)
        {
            if (CollisionRectangle.Intersects(platform.CollisionRectangle))
            {
                if (CollisionRectangle.Bottom < platform.CollisionRectangle.Top + platform.CollisionRectangle.Height / 2)
                {
                    Position = new Vector2(Position.X, platform.CollisionRectangle.Top - Height);
                    velocity.Y = 0;
                }
                else if (CollisionRectangle.Top > platform.CollisionRectangle.Bottom - platform.CollisionRectangle.Height / 2)
                {
                    Position = new Vector2(Position.X, platform.CollisionRectangle.Bottom);
                    velocity.Y = 0;
                }
            }
        }
        
        Position = new Vector2(Position.X + velocity.X, Position.Y + velocity.Y);
    }

    public void MoveRight(List<Platform> platforms)
    {
        var platformRects = platforms
            .Select(p => p.CollisionRectangle)
            .ToList();
        if (platformRects.Any(rect => CollisionRectangle.IsPlatformLeft(rect) || CollisionRectangle.IsPlatformRight(rect)))
        {
            velocity.X = 0;
            return;
        }
        velocity.X += diffVelocityX;
    }

    public void MoveLeft(List<Platform> platforms)
    {
        var platformRects = platforms
            .Select(p => p.CollisionRectangle)
            .ToList();
        if (platformRects.Any(rect => CollisionRectangle.IsPlatformLeft(rect) || CollisionRectangle.IsPlatformRight(rect)))
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
        
        int distToPlatformOver = 0;
        int minDistToPlatformOver = int.MaxValue;
        foreach (var platform in platforms)
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

    public void MoveNone(List<Platform> platforms)
    {
        if (Math.Abs(velocity.X) < 2 * diffVelocityX)
        {
            velocity.X = 0;
        }
        else if (velocity.X > epsilon)
        {
            velocity.X -= diffVelocityX;
            playerAnimation.playerDirection = PlayerDirection.Right;
        }
        else if (velocity.X < -epsilon)
        {
            velocity.X += diffVelocityX;
            playerAnimation.playerDirection = PlayerDirection.Left;
        }
    }

    private void CorrectVelocity(GraphicsDeviceManager graphics, List<Platform> platforms)
    {
        var platformRects = platforms
            .Select(p => p.CollisionRectangle)
            .ToList();
        if (platformRects.Any(rect => CollisionRectangle.IsPlatformUp(rect)))
        {
            velocity.Y = 1f;
        }
        velocity = gravitation.AffectGravitation(graphics, velocity, CollisionRectangle, platformRects);
    }

    private void FindIsGrounded(GraphicsDeviceManager graphics, List<Platform> platforms)
    {
        IsGrounded = gravitation.IsGrounded(graphics, CollisionRectangle,
            platforms.Select(p => p.CollisionRectangle).ToList());
    }
    
    private float GetJumpVelocity(float height)
    {
        return (float)Math.Sqrt(height / 40 * gravitation.GetGravitationConst() * Height);
    }

    private void ChangeJumpCooldown(GameTime gameTime)
    {
        timer += gameTime.ElapsedGameTime.TotalSeconds;
        if (timer >= jumpCooldown)
        {
            timer = 0;
            isCanJump = true;
        }
    }
    
    public void TakeDamage(int damage) => health -= damage;
    public void Heal(int heal) => health += heal;
    public int GetHealth => health;
}