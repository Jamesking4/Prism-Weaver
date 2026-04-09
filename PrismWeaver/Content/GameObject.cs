using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrismWeaver.Content;

public abstract class GameObject
{
    public Vector2 Position { get; set; }
    
    public int Width { get; set; }
    public int Height { get; set; }

    public Rectangle Rectangle => new((int)Position.X, (int)Position.Y, Width, Height);
    
    protected Vector2 collisionOffset = Vector2.Zero;
    protected Point collisionSize;

    public Rectangle CollisionRectangle => new(
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
    
    public virtual void Update(GameTime gameTime)
    {
        // Базовая реализация пуста – переопределяется в наследниках
    }
    
    public virtual void Draw(SpriteBatch spriteBatch)
    {
        // Базовая реализация пуста – переопределяется в наследниках
    }
}