using Microsoft.Xna.Framework;

namespace PrismWeaver.Content;

public abstract class GameObject
{
    public Vector2 Position { get; set; }
    
    public int Width { get; set; }
    public int Height { get; set; }

    public Rectangle Rectangle => new Rectangle(
        (int)Position.X, (int)Position.Y, Width, Height
    );
    
    protected Vector2 collisionOffset = Vector2.Zero;
    protected Point collisionSize;

    public Rectangle CollisionRectangle => new Rectangle(
        (int)(Position.X + collisionOffset.X),
        (int)(Position.Y + collisionOffset.Y),
        collisionSize.X,
        collisionSize.Y
    );

    protected GameObject(Vector2 startPosition, int width, int height)
    {
        Position = startPosition;
        Width = width;
        Height = height;
        
        collisionSize = new Point(width, height);
    }

    protected void SetCollisionBox(Vector2 offset, int width, int height)
    {
        collisionOffset = offset;
        collisionSize = new Point(width, height);
    }

    protected void SetCollisionOffset(Vector2 offset)
    {
        collisionOffset = offset;
    }

    protected void SetCollisionSize(int width, int height)
    {
        collisionSize = new Point(width, height);
    }

    public void SetPosition(Vector2 position)
    {
        Position = position;
    }
}