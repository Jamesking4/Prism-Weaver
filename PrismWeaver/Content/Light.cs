using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PrismWeaver.Content;

public class Light : GameObject
{
    private Color color;
    private Direction direction;
    private Texture2D texture;
    private bool enabled;
    private List<GameObject> gameObjects;
    private GraphicsDeviceManager graphics;
    
    public Light(Vector2 startPosition, int width, int height) 
        : base(startPosition, width, height)
    {
        IsColliding = false;
    }

    public void Initialize(GraphicsDeviceManager graphics, Color color, bool isWork, Direction direction, Texture2D texture, List<GameObject> gameObjects)
    {
        this.color = color;
        this.direction = direction;
        enabled = isWork;
        this.gameObjects = gameObjects;
        this.graphics = graphics;
        
        this.texture = texture;
        texture.SetData([color]);
    }

    public override void Update(GameTime gameTime)
    {
        (Width, Height) = CalculateWidthAndHeight();
        collisionSize.X = Width;
        collisionSize.Y = Height;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (enabled)
            spriteBatch.Draw(texture, DrawRectangle, color);
    }

    private Tuple<int, int> CalculateWidthAndHeight()
    {
        var width = CollisionRectangle.Width;
        var height = CollisionRectangle.Height;
        if (direction == Direction.Right)
        {
            width = graphics.GraphicsDevice.Viewport.Width;
            foreach (var obj in gameObjects)
            {
                if (obj is LightSource or Light)
                    continue;
                
                if (obj.CollisionRectangle.Intersects(CollisionRectangle))
                    width = (int)(obj.CollisionRectangle.Right - obj.CollisionRectangle.Width / 2 - Position.X);
            }
        }
        
        else if (direction == Direction.Left)
        {
            width = graphics.GraphicsDevice.Viewport.Width;
            foreach (var obj in gameObjects)
            {
                if (obj is LightSource or Light)
                    continue;
                
                if (obj.CollisionRectangle.Intersects(CollisionRectangle))
                    width = (int)(Position.X - obj.CollisionRectangle.Right - obj.CollisionRectangle.Width / 2);
            }
        }
        
        else if (direction == Direction.Down)
        {
            height = graphics.GraphicsDevice.Viewport.Height;
            foreach (var obj in gameObjects)
            {
                if (obj is LightSource or Light)
                    continue;
                
                if (obj.CollisionRectangle.Intersects(CollisionRectangle))
                    height = (int)(obj.CollisionRectangle.Top + obj.CollisionRectangle.Height / 2 - Position.Y);
            }
        }
        
        else 
        {
            height = graphics.GraphicsDevice.Viewport.Height;
            foreach (var obj in gameObjects)
            {
                if (obj is LightSource or Light)
                    continue;
                
                if (obj.CollisionRectangle.Intersects(CollisionRectangle))
                    height = (int)(Position.Y - obj.CollisionRectangle.Top + obj.CollisionRectangle.Height / 2);
            }
        }
        
        return new Tuple<int, int>(width, height);
    }
    
    private List<Rectangle> GetRectangles()
    {
        return gameObjects
            .Where(obj => obj is not LightSource && obj != this)
            .Select(p => p.CollisionRectangle)
            .ToList();
    }
}