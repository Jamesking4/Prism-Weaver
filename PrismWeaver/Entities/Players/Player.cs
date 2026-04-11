using System;
using System.Collections.Generic;
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

    public Player(GraphicsDeviceManager graphics, ContentManager content, Vector2 startPosition, int width, int height,
        List<GameObject> gameObjects, float MaxVelocityX = 4)
        : base(graphics, startPosition, width, height, gameObjects, MaxVelocityX)
    {
        playerAnimation = new PlayerAnimation(content, new Point(width, height));
        var offset = new Vector2(
            (PlayerAnimation.FrameWidth - PlayerAnimation.RealFrameWidth) / 2f,
            PlayerAnimation.FrameHeight - PlayerAnimation.RealFrameHeight);
        var realSize = new Point(PlayerAnimation.RealFrameWidth, PlayerAnimation.RealFrameHeight);
        SetCollision(offset, realSize);
    }

    public override void Update(GameTime gameTime)
    {
        BaseMove();
        playerAnimation.Update(gameTime, Velocity, IsGrounded);
        ChangeJumpCooldown(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        var drawPosition = Position - collisionOffset;
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

        var distToPlatformOver = 0;
        var minDistToPlatformOver = int.MaxValue;
        foreach (var obj in gameObjects)
        {
            if (obj == this || !obj.IsColliding)
                continue;
            distToPlatformOver = CollisionRectangle.GetDistToRectangleOver(obj.CollisionRectangle);
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