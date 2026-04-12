using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrismWeaver.Core;

public abstract class GameObject
{
    protected Vector2 Position { get; set; }

    protected int Width { get; set; }
    protected int Height { get; set; }

    public bool IsColliding { get; set; }
    public bool IsPushable { get; set; } = false;
    
    public Rectangle DrawRectangle => new(
        (int)(Position.X - CollisionOffset.X),
        (int)(Position.Y - CollisionOffset.Y),
        Width,
        Height
    );

    protected Vector2 CollisionOffset = Vector2.Zero;
    protected Point CollisionSize;
    
    public Rectangle CollisionRectangle => new(
        (int)Position.X,
        (int)Position.Y,
        CollisionSize.X,
        CollisionSize.Y
    );

    protected GameObject(Vector2 startPosition, int width, int height, bool isColliding = true)
    {
        Position = startPosition;
        Width = width;
        Height = height;
        IsColliding = isColliding;

        CollisionSize = new Point(width, height);
    }

    protected void SetCollision(Vector2 collisionOffset, Point collisionSize)
    {
        this.CollisionOffset = collisionOffset;
        this.CollisionSize = collisionSize;
    }

    public virtual void Update(GameTime gameTime) { }
    public virtual void Draw(SpriteBatch spriteBatch) { }
}