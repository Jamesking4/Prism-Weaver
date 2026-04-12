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
            if (this is Player)
                TryShiftStandingBlock(Velocity.X);
            
            TryMoveHorizontal(Velocity.X);
        }
        
        (Position, Velocity) = ApplyWindowBoundary(CollisionRectangle, Velocity);

        if (!CanMoveLeft && Velocity.X < 0)
            Velocity.X = 0;
        if (!CanMoveRight && Velocity.X > 0)
            Velocity.X = 0;
    }

    private void TryShiftStandingBlock(float moveDirection)
    {
        if (Math.Abs(moveDirection) < 0.01f)
            return;
        
        var blocksUnder = FindBlocksUnderPlayer();
        if (blocksUnder.Count == 0)
            return;
        
        var playerWidth = CollisionSize.X;
        var threshold = 1.5f * playerWidth;
        
        if (blocksUnder.Count == 1)
        {
            var block = blocksUnder[0];

            var minDistLeft = GetMinDistanceToObstacle(block, -1);
            var minDistRight = GetMinDistanceToObstacle(block, 1);
            var minDist = Math.Min(minDistLeft, minDistRight);
            
            if (minDist < threshold)
            {
                var step = Math.Sign(moveDirection) * Math.Min(Math.Abs(moveDirection), MaxPushStep);
                if (TryPushBlockChain(block, step))
                {
                    Position = new Vector2(Position.X + step, Position.Y);
                }
            }
        }
    }

    private List<DynamicObject> FindBlocksUnderPlayer()
    {
        var result = new List<DynamicObject>();
        var playerRect = CollisionRectangle;
        var collisions = GetRectanglesWithCollision();
        
        foreach (var rect in collisions)
        {
            var otherObj = GameObjects.FirstOrDefault(obj => obj.CollisionRectangle == rect);
            if (otherObj == null || !otherObj.IsPushable)
                continue;
            if (otherObj is not DynamicObject block)
                continue;
            
            var isOnTop = Math.Abs(playerRect.Bottom - rect.Top) <= 2 &&
                           playerRect.Right > rect.Left && playerRect.Left < rect.Right;
            if (isOnTop)
                result.Add(block);
        }
        return result;
    }

    private float GetMinDistanceToObstacle(DynamicObject block, int direction)
    {
        var maxDistance = 10000f;
        
        for (var d = block.CollisionSize.X / 2f; d <= maxDistance; d += 1f)
        {
            var checkX = block.Position.X + (direction == -1 ? -d : d + block.CollisionSize.X);
            var checkRect = new Rectangle((int)checkX, (int)block.Position.Y, 1, block.CollisionSize.Y);
            
            if (direction == -1 && checkX < 0)
                return d;
            if (direction == 1 && checkX + 1 > graphics.GraphicsDevice.Viewport.Width)
                return d;
            
            var hit = false;
            foreach (var other in GameObjects)
            {
                if (other == block)
                    continue;
                if (!other.IsColliding)
                    continue;
                if (checkRect.Intersects(other.CollisionRectangle))
                {
                    hit = true;
                    break;
                }
            }
            if (hit)
                return d;
        }
        return maxDistance;
    }

    private void TryMoveHorizontal(float deltaX)
    {
        var newX = Position.X + deltaX;
        var newRect = new Rectangle((int)newX, (int)Position.Y, CollisionSize.X, CollisionSize.Y);
        var collisions = GetRectanglesWithCollision();

        foreach (var rect in collisions)
        {
            if (!newRect.Intersects(rect))
                continue;

            var otherObj = GameObjects.FirstOrDefault(obj => obj.CollisionRectangle == rect);
            if (otherObj == null)
                continue;

            if (HandleCollisionWithObject(newRect, rect, otherObj))
                return;
        }
        
        Position = new Vector2(newX, Position.Y);
    }

    private bool HandleCollisionWithObject(Rectangle newRect, Rectangle rect, GameObject otherObj)
    {
        var otherIsPushable = otherObj.IsPushable;
        var thisIsPlayer = this is Player;

        if (thisIsPlayer && otherIsPushable)
        {
            var block = otherObj as DynamicObject;
            if (block == null)
                return false;
            
            var playerIsLeft = newRect.Center.X < rect.Center.X;
            var playerIsRight = newRect.Center.X > rect.Center.X;
            if (!(playerIsLeft || playerIsRight))
                return false;

            return HandlePushableCollision(rect, block, playerIsLeft, newRect);
        }

        HandleNonPushableCollision(rect, newRect);
        return true;
    }

    private bool HandlePushableCollision(Rectangle rect, DynamicObject block, bool playerIsLeft, Rectangle newRect)
    {
        var overlap = playerIsLeft ? newRect.Right - rect.Left : rect.Right - newRect.Left;
        var pushDir = playerIsLeft ? 1 : -1;
        var pushDistance = Math.Min(overlap, MaxPushStep) * pushDir;

        if (TryPushBlockChain(block, pushDistance))
        {
            ApplyPushSuccess(block, playerIsLeft);
            return true;
        }
        ApplyPushFailure(rect, playerIsLeft);
        return true;
    }

    private void ApplyPushSuccess(DynamicObject block, bool playerIsLeft)
    {
        var pushedCount = CountPushedBlocks(block, Math.Abs(block.Position.X - Position.X));
        var slowdown = Math.Max(0.2f, 1f - pushedCount * 0.05f);
        Velocity.X *= slowdown;
        if (Math.Abs(Velocity.X) < 0.05f) Velocity.X = 0;

        var newBlockRect = block.CollisionRectangle;
        float targetX = playerIsLeft ? newBlockRect.Left - CollisionSize.X : newBlockRect.Right;
        Position = new Vector2(targetX, Position.Y);
    }

    private void ApplyPushFailure(Rectangle rect, bool playerIsLeft)
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
    }

    private void HandleNonPushableCollision(Rectangle rect, Rectangle newRect)
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

            if (TryHandlePlayerLandingOnPushable(rect, otherObj))
                continue;

            ResolveVerticalIntersection(rect);
        }
    }

    private bool TryHandlePlayerLandingOnPushable(Rectangle rect, GameObject otherObj)
    {
        var otherIsPushable = otherObj?.IsPushable ?? false;
        var thisIsPlayer = this is Player;

        var playerLands = thisIsPlayer && otherIsPushable && Velocity.Y >= 0
                          && CollisionRectangle.Bottom > rect.Top
                          && CollisionRectangle.Bottom - rect.Top < CollisionSize.Y / 2;

        if (playerLands)
        {
            Position = new Vector2(Position.X, rect.Top - CollisionSize.Y);
            Velocity.Y = 0;
            return true;
        }
        return false;
    }

    private void ResolveVerticalIntersection(Rectangle rect)
    {
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
        {
            Position = new Vector2(Position.X, Position.Y - Velocity.Y);
        }
        Velocity.Y = 0;
    }

    private bool TryPushBlockChain(DynamicObject block, float pushDistance)
    {
        if (block == null || !block.IsPushable)
            return false;
        if (Math.Abs(pushDistance) < 0.01f)
            return true;
        if (!CanPushDirection(block, pushDistance))
            return false;

        var stack = GetCompleteStack(block);
        if (!CanPushStack(stack, pushDistance))
            return false;

        PushStack(stack, pushDistance);
        return true;
    }

    private bool CanPushDirection(DynamicObject block, float pushDistance)
    {
        if (pushDistance > 0 && !block.CanMoveRight)
            return false;
        if (pushDistance < 0 && !block.CanMoveLeft)
            return false;
        return true;
    }

    private HashSet<DynamicObject> GetCompleteStack(DynamicObject block)
    {
        var stack = new HashSet<DynamicObject>();
        GatherStackAbove(block, stack);
        return stack;
    }

    private void GatherStackAbove(DynamicObject block, HashSet<DynamicObject> stack)
    {
        if (!stack.Add(block))
            return;

        var aboveRect = new Rectangle(
            block.CollisionRectangle.X,
            block.CollisionRectangle.Y - block.CollisionSize.Y,
            block.CollisionSize.X,
            block.CollisionSize.Y);

        foreach (var obj in GameObjects)
        {
            if (obj == block || obj == this)
                continue;
            if (!obj.IsPushable)
                continue;
            if (obj is DynamicObject other && other.CollisionRectangle.Intersects(aboveRect))
                GatherStackAbove(other, stack);
        }
    }

    private bool CanPushStack(HashSet<DynamicObject> stack, float pushDistance)
    {
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
                if (!testRect.Intersects(other.CollisionRectangle))
                    continue;

                if (other.IsPushable && other is DynamicObject otherBlock)
                {
                    if (!TryPushBlockChain(otherBlock, pushDistance))
                        return false;
                }
                else
                    return false;
            }
        }
        return true;
    }

    private void PushStack(HashSet<DynamicObject> stack, float pushDistance)
    {
        foreach (var b in stack)
        {
            b.Position = new Vector2(b.Position.X + pushDistance, b.Position.Y);
            b.ResolveVerticalCollisions();
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
            if (!other.IsColliding || !other.IsPushable)
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