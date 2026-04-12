using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PrismWeaver.Entities.Players;
using PrismWeaver.Physics;

namespace PrismWeaver.Core;

public abstract class DynamicObject : GameObject
{
    private readonly GraphicsDeviceManager graphics;
    protected readonly List<GameObject> GameObjects;
    protected Vector2 Velocity;
    private readonly float maxVelocityX;
    
    private const float MaxPushStep = 2f;

    protected bool IsGrounded => FindIsGrounded();
    protected bool CanMoveLeft { get; set; } = true;
    protected bool CanMoveRight { get; set; } = true;

    protected DynamicObject(GraphicsDeviceManager graphics, Vector2 startPosition, int width, int height,
        List<GameObject> gameObjects, float maxVelocityX = 4f) : base(startPosition, width, height)
    {
        this.graphics = graphics;
        this.GameObjects = gameObjects;
        this.maxVelocityX = maxVelocityX;
    }

    public override void Update(GameTime gameTime) { }
    public override void Draw(SpriteBatch spriteBatch) { }

    protected void BaseMove()
    {
        CanMoveLeft = true;
        CanMoveRight = true;
        
        AffordGravity();
        Velocity.X = Math.Clamp(Velocity.X, -maxVelocityX, maxVelocityX);
        
        MoveStep(Velocity);
        
        (Position, Velocity) = ApplyWindowBoundary(CollisionRectangle, Velocity);
    }

    private void MoveStep(Vector2 delta)
    {
        Position = new Vector2(Position.X, Position.Y + delta.Y);
        ResolveVerticalCollisions();
        
        if (delta.X != 0)
        {
            var remaining = delta.X;
            while (Math.Abs(remaining) > 0.01f)
            {
                var step = Math.Sign(remaining) * Math.Min(Math.Abs(remaining), 4f);
                if (!TryMoveHorizontal(step))
                    break;
                remaining -= step;
            }
        }
    }

    private bool TryMoveHorizontal(float deltaX)
    {
        var newX = Position.X + deltaX;
        var newRect = new Rectangle((int)newX, (int)Position.Y, collisionSize.X, collisionSize.Y);
        
        var collisions = GetRectanglesWithCollision();
        foreach (var rect in collisions)
        {
            if (newRect.Intersects(rect))
            {
                var otherObj = GameObjects.FirstOrDefault(obj => obj.CollisionRectangle == rect);
                if (otherObj == null) continue;

                var otherIsPushable = otherObj.IsPushable;
                var thisIsPlayer = this is Player;

                if (thisIsPlayer && otherIsPushable)
                {
                    var block = otherObj as DynamicObject;
                    if (block == null) continue;
                    
                    var playerIsLeft = newRect.Center.X < rect.Center.X;
                    var playerIsRight = newRect.Center.X > rect.Center.X;
                    if (playerIsLeft || playerIsRight)
                    {
                        var overlap = playerIsLeft ? newRect.Right - rect.Left : rect.Right - newRect.Left;
                        var pushDir = playerIsLeft ? 1 : -1;
                        var pushDistance = Math.Min(overlap, MaxPushStep) * pushDir;

                        if (TryPushBlockChain(block, pushDistance))
                        {
                            var pushedCount = CountPushedBlocks(block, pushDistance);
                            var slowdown = Math.Max(0.2f, 1f - pushedCount * 0.05f);
                            Velocity.X *= slowdown;

                            var newBlockRect = block.CollisionRectangle;
                            float targetX = playerIsLeft ? newBlockRect.Left - collisionSize.X : newBlockRect.Right;
                            Position = new Vector2(targetX, Position.Y);
                            return false;
                        }
                        else
                        {
                            Velocity.X = 0;
                            float targetX = playerIsLeft ? rect.Left - collisionSize.X : rect.Right;
                            Position = new Vector2(targetX, Position.Y);
                            return false;
                        }
                    }
                }
                else
                {
                    Position = newRect.Center.X < rect.Center.X 
                        ? new Vector2(rect.Left - collisionSize.X, Position.Y) 
                        : new Vector2(rect.Right, Position.Y);
                    return false;
                }
            }
        }
        
        Position = new Vector2(newX, Position.Y);
        return true;
    }

    private void ResolveVerticalCollisions()
    {
        var collisions = GetRectanglesWithCollision();
        foreach (var rect in collisions)
        {
            if (!CollisionRectangle.Intersects(rect)) continue;

            var otherObj = GameObjects.FirstOrDefault(obj => obj.CollisionRectangle == rect);
            var otherIsPushable = otherObj?.IsPushable ?? false;
            var thisIsPlayer = this is Player;

            var playerLandsOnPushable = thisIsPlayer && otherIsPushable && Velocity.Y >= 0
                && CollisionRectangle.Bottom > rect.Top
                && CollisionRectangle.Bottom - rect.Top < collisionSize.Y / 2;

            if (playerLandsOnPushable)
            {
                Position = new Vector2(Position.X, rect.Top - collisionSize.Y);
                Velocity.Y = 0;
                continue;
            }

            var intersection = Rectangle.Intersect(CollisionRectangle, rect);
            if (intersection.Height < intersection.Width)
            {
                if (Velocity.Y > 0)
                {
                    Position = new Vector2(Position.X, rect.Top - collisionSize.Y);
                }
                else if (Velocity.Y < 0)
                {
                    Position = new Vector2(Position.X, rect.Bottom);
                }
                else
                {
                    Position = CollisionRectangle.Center.Y < rect.Center.Y 
                        ? new Vector2(Position.X, rect.Top - collisionSize.Y) 
                        : new Vector2(Position.X, rect.Bottom);
                }
            }
            else
            {
                Position = new Vector2(Position.X, Position.Y - Velocity.Y);
            }

            Velocity.Y = 0;
        }
    }

    private bool TryPushBlockChain(DynamicObject block, float pushDistance)
    {
        if (block == null || !block.IsPushable) return false;
        if (Math.Abs(pushDistance) < 0.01f) return true;

        if (pushDistance > 0 && !block.CanMoveRight) return false;
        if (pushDistance < 0 && !block.CanMoveLeft) return false;

        var newX = block.Position.X + pushDistance;
        if (newX < 0 || newX + block.collisionSize.X > graphics.GraphicsDevice.Viewport.Width)
            return false;

        var oldPos = block.Position;
        block.Position = new Vector2(newX, block.Position.Y);

        foreach (var other in GameObjects)
        {
            if (other == block) continue;
            if (!other.IsColliding) continue;

            if (block.CollisionRectangle.Intersects(other.CollisionRectangle))
            {
                if (other == this)
                {
                    block.Position = oldPos;
                    return false;
                }

                if (other.IsPushable && other is DynamicObject otherBlock)
                {
                    if (!TryPushBlockChain(otherBlock, pushDistance))
                    {
                        block.Position = oldPos;
                        return false;
                    }
                }
                else
                {
                    block.Position = oldPos;
                    return false;
                }
            }
        }
        return true;
    }
    
    private int CountPushedBlocks(DynamicObject block, float pushDistance)
    {
        if (block == null || !block.IsPushable) return 0;

        var count = 1;
        var testRect = new Rectangle(
            (int)(block.Position.X + pushDistance),
            (int)block.Position.Y,
            block.collisionSize.X,
            block.collisionSize.Y);

        foreach (var other in GameObjects)
        {
            if (other == block || other == this) continue;
            if (!other.IsColliding) continue;
            if (!other.IsPushable) continue;

            if (testRect.Intersects(other.CollisionRectangle) && other is DynamicObject otherBlock)
            {
                count += CountPushedBlocks(otherBlock, pushDistance);
            }
        }
        return count;
    }

    private void AffordGravity()
    {
        var rectangles = GetRectanglesWithCollision();
        Velocity = Gravitation.AffectGravitation(graphics, Velocity, CollisionRectangle, rectangles);
    }

    private List<Rectangle> GetRectanglesWithCollision()
    {
        return GameObjects
            .Where(obj => obj.IsColliding && obj != this)
            .Select(p => p.CollisionRectangle)
            .ToList();
    }

    private bool FindIsGrounded()
    {
        return Gravitation.IsGrounded(graphics, CollisionRectangle, GetRectanglesWithCollision());
    }

    private (Vector2, Vector2) ApplyWindowBoundary(Rectangle rect, Vector2 velocity)
    {
        var position = new Vector2(rect.X, rect.Y);
        var newVelocity = velocity;

        if (rect.Left < 0)
        {
            position.X = 0;
            newVelocity.X = 0;
            CanMoveLeft = false;
        }
        if (rect.Right > graphics.GraphicsDevice.Viewport.Width)
        {
            position.X = graphics.GraphicsDevice.Viewport.Width - rect.Width;
            newVelocity.X = 0;
            CanMoveRight = false;
        }
        if (rect.Top < 0)
        {
            position.Y = 0;
            newVelocity.Y = 0;
        }
        if (rect.Bottom > graphics.GraphicsDevice.Viewport.Height)
        {
            position.Y = graphics.GraphicsDevice.Viewport.Height - rect.Height;
            newVelocity.Y = 0;
        }
        return (position, newVelocity);
    }
}