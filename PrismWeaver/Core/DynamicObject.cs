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
        
        Position = new Vector2(Position.X, Position.Y + Velocity.Y);
        ResolveVerticalCollisions();
        
        if (Velocity.X != 0)
        {
            TryMoveHorizontal(Velocity.X);
        }
        
        (Position, Velocity) = ApplyWindowBoundary(CollisionRectangle, Velocity);

        if (!CanMoveLeft && Velocity.X < 0) Velocity.X = 0;
        if (!CanMoveRight && Velocity.X > 0) Velocity.X = 0;
    }

    private void TryMoveHorizontal(float deltaX)
    {
        var newX = Position.X + deltaX;
        var newRect = new Rectangle((int)newX, (int)Position.Y, CollisionSize.X, CollisionSize.Y);
        
        var collisions = GetRectanglesWithCollision();
        foreach (var rect in collisions)
        {
            if (newRect.Intersects(rect))
            {
                var otherObj = GameObjects.FirstOrDefault(obj => obj.CollisionRectangle == rect);
                if (otherObj == null)
                    continue;

                var otherIsPushable = otherObj.IsPushable;
                var thisIsPlayer = this is Player;

                if (thisIsPlayer && otherIsPushable)
                {
                    var block = otherObj as DynamicObject;
                    if (block == null)
                        continue;
                    
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
                            if (Math.Abs(Velocity.X) < 0.05f)
                                Velocity.X = 0;

                            var newBlockRect = block.CollisionRectangle;
                            float targetX = playerIsLeft ? newBlockRect.Left - CollisionSize.X : newBlockRect.Right;
                            Position = new Vector2(targetX, Position.Y);
                            return;
                        }
                        else
                        {
                            Velocity.X = 0;
                            if (playerIsLeft)
                            {
                                Position = new Vector2(rect.Left - CollisionSize.X, Position.Y);
                                CanMoveRight = false;
                            }
                            else
                            {
                                Position = new Vector2(rect.Right, Position.Y);
                                CanMoveLeft = false;
                            }

                            return;
                        }
                    }
                }
                else
                {
                    if (newRect.Center.X < rect.Center.X)
                    {
                        Position = new Vector2(rect.Left - CollisionSize.X, Position.Y);
                        CanMoveRight = false;
                    }
                    else
                    {
                        Position = new Vector2(rect.Right, Position.Y);
                        CanMoveLeft = false;
                    }
                    Velocity.X = 0;
                    return;
                }
            }
        }
        
        Position = new Vector2(newX, Position.Y);
    }

    private void ResolveVerticalCollisions()
    {
        var collisions = GetRectanglesWithCollision();
        foreach (var rect in collisions)
        {
            if (!CollisionRectangle.Intersects(rect))
                continue;

            var otherObj = GameObjects.FirstOrDefault(obj => obj.CollisionRectangle == rect);
            
            if (this is not Player && otherObj is Player)
                continue;

            var otherIsPushable = otherObj?.IsPushable ?? false;
            var thisIsPlayer = this is Player;

            var playerLandsOnPushable = thisIsPlayer && otherIsPushable && Velocity.Y >= 0
                                        && CollisionRectangle.Bottom > rect.Top
                                        && CollisionRectangle.Bottom - rect.Top < CollisionSize.Y / 2;

            if (playerLandsOnPushable)
            {
                Position = new Vector2(Position.X, rect.Top - CollisionSize.Y);
                Velocity.Y = 0;
                continue;
            }

            var intersection = Rectangle.Intersect(CollisionRectangle, rect);
            if (intersection.Height < intersection.Width)
            {
                if (Velocity.Y > 0)
                    Position = new Vector2(Position.X, rect.Top - CollisionSize.Y);
                else if (Velocity.Y < 0)
                    Position = new Vector2(Position.X, rect.Bottom);
                else
                {
                    Position = CollisionRectangle.Center.Y < rect.Center.Y 
                        ? new Vector2(Position.X, rect.Top - CollisionSize.Y) 
                        : new Vector2(Position.X, rect.Bottom);
                }
            }
            else
                Position = new Vector2(Position.X, Position.Y - Velocity.Y);

            Velocity.Y = 0;
        }
    }

    private bool TryPushBlockChain(DynamicObject block, float pushDistance)
    {
        if (block == null || !block.IsPushable)
            return false;
        if (Math.Abs(pushDistance) < 0.01f)
            return true;

        if (pushDistance > 0 && !block.CanMoveRight)
            return false;
        if (pushDistance < 0 && !block.CanMoveLeft)
            return false;
        
        var stack = new HashSet<DynamicObject>();
        GatherStackAbove(block, stack);
        
        foreach (var b in stack)
        {
            var newX = b.Position.X + pushDistance;
            if (newX < 0 || newX + b.CollisionSize.X > graphics.GraphicsDevice.Viewport.Width)
                return false;
        
            var testRect = new Rectangle((int)newX, (int)b.Position.Y, b.CollisionSize.X, b.CollisionSize.Y);
            foreach (var other in GameObjects)
            {
                if (other == b || stack.Contains(other as DynamicObject))
                    continue;
                if (!other.IsColliding)
                    continue;
                if (testRect.Intersects(other.CollisionRectangle))
                {
                    if (other.IsPushable && other is DynamicObject otherBlock)
                    {
                        if (!TryPushBlockChain(otherBlock, pushDistance))
                            return false;
                    }
                    else
                        return false;
                }
            }
        }
        
        foreach (var b in stack)
        {
            b.Position = new Vector2(b.Position.X + pushDistance, b.Position.Y);
            b.ResolveVerticalCollisions();
        }
        return true;
    }
    
    private void GatherStackAbove(DynamicObject block, HashSet<DynamicObject> stack)
    {
        if (!stack.Add(block))
            return;

        var aboveRect = new Rectangle(block.CollisionRectangle.X, 
            block.CollisionRectangle.Y - block.CollisionSize.Y,
            block.CollisionSize.X, 
            block.CollisionSize.Y);
        foreach (var obj in GameObjects)
        {
            if (obj == block || obj == this)
                continue;
            if (obj.IsPushable && obj is DynamicObject other && other.CollisionRectangle.Intersects(aboveRect))
            {
                GatherStackAbove(other, stack);
            }
        }
    }
    
    private int CountPushedBlocks(DynamicObject block, float pushDistance)
    {
        if (block == null || !block.IsPushable)
            return 0;

        var count = 1;
        var testRect = new Rectangle(
            (int)(block.Position.X + pushDistance),
            (int)block.Position.Y,
            block.CollisionSize.X,
            block.CollisionSize.Y);

        foreach (var other in GameObjects)
        {
            if (other == block || other == this)
                continue;
            if (!other.IsColliding)
                continue;
            if (!other.IsPushable)
                continue;

            if (testRect.Intersects(other.CollisionRectangle) && other is DynamicObject otherBlock)
                count += CountPushedBlocks(otherBlock, pushDistance);
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
            newVelocity.X = Math.Max(0, newVelocity.X);
            CanMoveLeft = false;
        }
        if (rect.Right > graphics.GraphicsDevice.Viewport.Width)
        {
            position.X = graphics.GraphicsDevice.Viewport.Width - rect.Width;
            newVelocity.X = Math.Min(0, newVelocity.X);
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