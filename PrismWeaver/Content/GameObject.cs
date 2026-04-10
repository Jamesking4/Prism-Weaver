using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrismWeaver.Content;

public abstract class GameObject
{
    protected Vector2 Position { get; set; }

    protected int Width { get; set; }
    protected int Height { get; set; }

    public bool IsColliding { get; set; }
    public bool IsPushable { get; set; } = false;
    
    public Rectangle DrawRectangle => new(
        (int)(Position.X - collisionOffset.X),
        (int)(Position.Y - collisionOffset.Y),
        Width,
        Height
    );

    protected Vector2 collisionOffset = Vector2.Zero;
    protected Point collisionSize;
    
    public Rectangle CollisionRectangle => new(
        (int)Position.X,
        (int)Position.Y,
        collisionSize.X,
        collisionSize.Y
    );

    protected GameObject(Vector2 startPosition, int width, int height, bool isColliding = true)
    {
        Position = startPosition;
        Width = width;
        Height = height;
        IsColliding = isColliding;

        collisionSize = new Point(width, height);
    }

    protected void SetCollision(Vector2 collisionOffset, Point collisionSize)
    {
        this.collisionOffset = collisionOffset;
        this.collisionSize = collisionSize;
    }

    public virtual void Update(GameTime gameTime) { }
    public virtual void Draw(SpriteBatch spriteBatch) { }
}