using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PrismWeaver.Core;
using PrismWeaver.Physics;
using PrismWeaver.Utilities;

namespace PrismWeaver.Entities.Players;

public class Player : DynamicObject
{
    private int health = 10;
    private readonly PlayerAnimation playerAnimation;

    private const float DiffVelocityX = 0.1f;
    private const float Epsilon = 0.01f;
    private const double JumpCooldown = 0.9;
    private double timer;
    private bool isCanJump = true;

    public Player(
        GraphicsDeviceManager graphics, 
        ContentManager content, 
        Vector2 startPosition, 
        List<GameObject> gameObjects,
        int width, 
        int height,
        float maxVelocityX = 4
        ) : base(graphics, startPosition, width, height, gameObjects, maxVelocityX)
    {
        playerAnimation = new PlayerAnimation(content);
        
        var offset = new Vector2(
            (Config.FrameWidth - Config.RealFrameWidth) / 2f,
            Config.FrameHeight - Config.RealFrameHeight);
        
        var realSize = new Point(Config.RealFrameWidth, Config.RealFrameHeight);
        SetCollision(offset, realSize);
    }

    public override void Update(GameTime gameTime)
    {
        BaseMove();
        var canMove = CanMoveLeft && CanMoveRight;
        playerAnimation.Update(gameTime, Velocity, IsGrounded, canMove);
        ChangeJumpCooldown(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        var drawPosition = Position - CollisionOffset;
        playerAnimation.Draw(spriteBatch, drawPosition);
    }

    public void MoveRight()
    {
        Velocity.X += DiffVelocityX;
    }

    public void MoveLeft()
    {
        Velocity.X -= DiffVelocityX;
    }
    public void Jump()
    {
        if (!IsGrounded || !isCanJump)
            return;

        var minDistToPlatformOver = int.MaxValue;
        foreach (var obj in GameObjects)
        {
            if (obj == this || !obj.IsColliding)
                continue;
            
            var distToPlatformOver = CollisionRectangle.GetDistToRectangleOver(obj.CollisionRectangle);
            if (distToPlatformOver != -1)
            {
                minDistToPlatformOver = Math.Min(distToPlatformOver, minDistToPlatformOver);
            }
        }

        Position = new Vector2(Position.X, Position.Y - 5);
        Velocity.Y = -GetJumpVelocity(Math.Min(128, minDistToPlatformOver));
        isCanJump = false;
    }

    public void MoveNone()
    {
        if (Math.Abs(Velocity.X) < 2 * DiffVelocityX)
        {
            Velocity.X = 0;
        }
        else if (Velocity.X > Epsilon)
        {
            Velocity.X -= DiffVelocityX;
            playerAnimation.PlayerDirection = PlayerDirection.Right;
        }
        else if (Velocity.X < -Epsilon)
        {
            Velocity.X += DiffVelocityX;
            playerAnimation.PlayerDirection = PlayerDirection.Left;
        }
    }

    private float GetJumpVelocity(float height)
    {
        return (float)Math.Sqrt(height / 40 * Gravitation.GetGravitationConst() * CollisionRectangle.Height);
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