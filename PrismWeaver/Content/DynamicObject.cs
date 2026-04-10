using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrismWeaver.Content;

public abstract class DynamicObject : GameObject
{
    protected GraphicsDeviceManager graphics;
    protected List<GameObject> gameObjects;
    protected Vector2 Velocity;
    protected float MaxVelocityX;

    public bool IsGrounded => FindIsGrounded();

    public DynamicObject(GraphicsDeviceManager graphics, Vector2 startPosition, int width, int height,
        List<GameObject> gameObjects, float maxVelocityX = 4f) : base(startPosition, width, height)
    {
        this.graphics = graphics;
        this.gameObjects = gameObjects;
        MaxVelocityX = maxVelocityX;
    }

    public override void Update(GameTime gameTime) { }
    public override void Draw(SpriteBatch spriteBatch) { }

    public void BaseMove()
    {
        AffordGravity();
        (Position, Velocity) = Window.GetPositionAndVelocityInWindow(graphics, CollisionRectangle, Velocity);
        Move();
    }

   private void Move()
    {
        Velocity.X = Math.Clamp(Velocity.X, -MaxVelocityX, MaxVelocityX);
        var collisions = GetRectangleWithCollision();

        Position = new Vector2(Position.X, Position.Y + Velocity.Y);
        foreach (var rect in collisions)
        {
            if (!CollisionRectangle.Intersects(rect))
                continue;

            var otherObj = gameObjects.FirstOrDefault(obj => obj.CollisionRectangle == rect);
            var thisIsPushable = IsPushable;
            var otherIsPushable = otherObj?.IsPushable ?? false;
            var thisIsPlayer = this is Player;
            var otherIsPlayer = otherObj is Player;

            var playerLandsOnPushable = thisIsPlayer && otherIsPushable && Velocity.Y >= 0 
                && CollisionRectangle.Bottom > rect.Top && CollisionRectangle.Bottom - rect.Top < collisionSize.Y / 2;

            if (!playerLandsOnPushable)
            {
                if ((thisIsPushable && otherIsPlayer) || (thisIsPlayer && otherIsPushable))
                    continue;
            }

            var intersection = Rectangle.Intersect(CollisionRectangle, rect);
            if (intersection.Height < intersection.Width)
            {
                if (Velocity.Y > 0)
                {
                    Position = new Vector2(Position.X, rect.Top - collisionSize.Y);
                    Velocity.Y = 0;
                }
                else if (Velocity.Y < 0)
                {
                    Position = new Vector2(Position.X, rect.Bottom);
                    Velocity.Y = 0;
                }
                else
                {
                    if (CollisionRectangle.Center.Y < rect.Center.Y)
                        Position = new Vector2(Position.X, rect.Top - collisionSize.Y);
                    else
                        Position = new Vector2(Position.X, rect.Bottom);
                    Velocity.Y = 0;
                }
            }
            else
            {
                Position = new Vector2(Position.X, Position.Y - Velocity.Y);
                Velocity.Y = 0;
            }
        }

        Position = new Vector2(Position.X + Velocity.X, Position.Y);
        foreach (var rect in collisions)
        {
            if (!CollisionRectangle.Intersects(rect))
                continue;

            var otherObj = gameObjects.FirstOrDefault(obj => obj.CollisionRectangle == rect);
            var otherIsPushable = otherObj?.IsPushable ?? false;
            var thisIsPlayer = this is Player;
            var intersection = Rectangle.Intersect(CollisionRectangle, rect);

            if (intersection.Width < intersection.Height)
            {
                if (CollisionRectangle.Center.X < rect.Center.X)
                {
                    if (!(thisIsPlayer && otherIsPushable))
                    {
                        Position = new Vector2(rect.Left - collisionSize.X, Position.Y);
                        Velocity.X = 0;
                    }
                }
                else
                {
                    if (!(thisIsPlayer && otherIsPushable))
                    {
                        Position = new Vector2(rect.Right, Position.Y);
                        Velocity.X = 0;
                    }
                }
            }
            else
            {
                Position = new Vector2(Position.X - Velocity.X, Position.Y);
                Velocity.X = 0;
            }
        }
    }

    private void AffordGravity()
    {
        var rectangles = GetRectangleWithCollision();
        Velocity = Gravitation.AffectGravitation(graphics, Velocity, CollisionRectangle, rectangles);
    }

    private List<Rectangle> GetRectangleWithCollision()
    {
        return gameObjects
            .Where(obj => obj.IsColliding && obj != this)
            .Select(p => p.CollisionRectangle)
            .ToList();
    }

    private bool FindIsGrounded()
    {
        return Gravitation.IsGrounded(graphics, CollisionRectangle, GetRectangleWithCollision());
    }
}