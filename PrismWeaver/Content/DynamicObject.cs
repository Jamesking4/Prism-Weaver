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
        List<GameObject> gameObjects, float MaxVelocityX = 4f) : base(startPosition, width, height)
    {
        this.graphics = graphics;
        this.gameObjects = gameObjects;
        this.MaxVelocityX = MaxVelocityX;
    }

    public override void Update(GameTime gameTime) { }
    public override void Draw(SpriteBatch spriteBatch) { }

    public void BaseMove()
    {
        Move();
        ChangeVelocityIfKnockUp();
        AffordGravity();
        (Position, Velocity) = Window.GetPositionAndVelocityInWindow(graphics, CollisionRectangle, Velocity);
    }

    private void Move()
    {
        if (Velocity.X > MaxVelocityX)
            Velocity.X = MaxVelocityX;
        if (Velocity.X < -MaxVelocityX)
            Velocity.X = -MaxVelocityX;
        
        Position = new Vector2(Position.X + Velocity.X, Position.Y + Velocity.Y);

        var rectanglesWithCollision = GetRectangleWithCollision();

        foreach (var rec in rectanglesWithCollision)
        {
            if (!CollisionRectangle.Intersects(rec))
                continue;
            
            var otherObj = gameObjects.FirstOrDefault(obj => obj.CollisionRectangle == rec);
            var otherIsPushable = otherObj?.IsPushable ?? false;
            var thisIsPlayer = this is Player;
            
            if ((CollisionRectangle.Bottom < rec.Top + rec.Height / 2)
                && !(CollisionRectangle.IsRectangleLeft(rec) || CollisionRectangle.IsRectangleRight(rec)))
            {
                Position = new Vector2(Position.X, rec.Top - collisionSize.Y);
                Velocity.Y = 0;
            }
            else if ((CollisionRectangle.Top > rec.Bottom - rec.Height / 2)
                     && !(CollisionRectangle.IsRectangleLeft(rec) || CollisionRectangle.IsRectangleRight(rec)))
            {
                Position = new Vector2(Position.X, rec.Bottom);
                Velocity.Y = 0;
            }
            else if (CollisionRectangle.Right < rec.Right - rec.Width / 2)
            {
                if (thisIsPlayer && otherIsPushable)
                    continue;

                Position = new Vector2(rec.Left - collisionSize.X, Position.Y);
                Velocity.X = 0;
            }
            else if (CollisionRectangle.Left > rec.Right - rec.Width / 2)
            {
                if (thisIsPlayer && otherIsPushable)
                    continue;

                Position = new Vector2(rec.Right, Position.Y);
                Velocity.X = 0;
            }
        }
    }

    private void ChangeVelocityIfKnockUp()
    {
        var rectangles = GetRectangleWithCollision();
        if (rectangles.Any(rect => CollisionRectangle.IsRectangleUp(rect)))
        {
            Velocity.Y = 1f;
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