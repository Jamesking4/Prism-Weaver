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
    
    public Light(Vector2 startPosition, int width, int height) 
        : base(startPosition, width, height)
    {
        
    }

    public void Initialize(Color color, bool isWork, Direction direction, Texture2D texture, List<GameObject> gameObjects)
    {
        this.color = color;
        this.direction = direction;
        enabled = isWork;
        this.gameObjects = gameObjects;
        
        this.texture = texture;
        texture.SetData([color]);
    }

    public override void Update(GameTime gameTime)
    {
        (Width, Height) = CalculateWidthAndHeight();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (enabled)
            spriteBatch.Draw(texture, Rectangle, color);
    }

    private Tuple<int, int> CalculateWidthAndHeight()
    {
        var width = Rectangle.Width;
        var height = Rectangle.Height;
        if (direction == Direction.Right)
        {
            width = 100000;
            foreach (var obj in gameObjects)
            {
                if (obj is LightSource or Light)
                    continue;
                
                if (obj.Rectangle.Intersects(Rectangle))
                    width = (int)(obj.Rectangle.Right - obj.Rectangle.Width / 2 - Position.X);
            }
        }
        
        else if (direction == Direction.Left)
        {
            width = 100000;
            foreach (var obj in gameObjects)
            {
                if (obj is LightSource or Light)
                    continue;
                
                if (obj.Rectangle.Intersects(Rectangle))
                    width = (int)(Position.X - obj.Rectangle.Right - obj.Rectangle.Width / 2);
            }
        }
        
        else if (direction == Direction.Down)
        {
            height = 100000;
            foreach (var obj in gameObjects)
            {
                if (obj is LightSource or Light)
                    continue;
                
                if (obj.Rectangle.Intersects(Rectangle))
                    height = (int)(obj.Rectangle.Top + obj.Rectangle.Height / 2 - Position.Y);
            }
        }
        
        else 
        {
            height = 100000;
            foreach (var obj in gameObjects)
            {
                if (obj is LightSource or Light)
                    continue;
                
                if (obj.Rectangle.Intersects(Rectangle))
                    height = (int)(Position.Y - obj.Rectangle.Top + obj.Rectangle.Height / 2);
            }
        }
        
        return new Tuple<int, int>(width, height);
    }
}