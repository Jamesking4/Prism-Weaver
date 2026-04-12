using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PrismWeaver.Core;
using PrismWeaver.Entities.Interactive;
using PrismWeaver.Utilities;

namespace PrismWeaver.Entities.Lights;

public class Light : GameObject
{
    private Color color;
    private Direction direction;
    private Texture2D texture;
    private bool enabled;
    private List<GameObject> gameObjects;
    private GraphicsDeviceManager graphics;
    private LightSource source;
    private int lightWidth;
    
    public Light(LightSource source, int lightWidth, 
        Vector2 startPosition = new Vector2(), int width = 0, int height = 0) 
        : base(startPosition, width, height)
    {
        IsColliding = false;
        this.source = source;
        this.lightWidth = lightWidth;
    }

    public void Initialize(GraphicsDeviceManager graphics, Color color, bool isWork, Direction direction, 
        Texture2D texture, List<GameObject> gameObjects)
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
        CalculateNewWidthAndHeight();
        collisionSize.X = Width;
        collisionSize.Y = Height;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (enabled)
            spriteBatch.Draw(texture, DrawRectangle, color);
    }

    private void CalculateNewWidthAndHeight()
    {
        var sourceRect = source.CollisionRectangle;
        var viewport = graphics.GraphicsDevice.Viewport;
        var minDistance = float.MaxValue;

        foreach (var obj in gameObjects)
        {
            if (obj is Light or LightSource or GlassBlock) continue;
            var objRect = obj.CollisionRectangle;

            var distance = float.MaxValue;

            switch (direction)
            {
                case Direction.Up:
                    if (objRect.Bottom <= sourceRect.Top &&
                        objRect.Right > sourceRect.Center.X - lightWidth / 2f &&
                        objRect.Left < sourceRect.Center.X + lightWidth / 2f)
                    {
                        var penetration = objRect.Height / 10f;
                        distance = (sourceRect.Top - objRect.Bottom) + penetration;
                    }
                    break;

                case Direction.Down:
                    if (objRect.Top >= sourceRect.Bottom &&
                        objRect.Right > sourceRect.Center.X - lightWidth / 2f &&
                        objRect.Left < sourceRect.Center.X + lightWidth / 2f)
                    {
                        var penetration = objRect.Height / 10f;
                        distance = (objRect.Top - sourceRect.Bottom) + penetration;
                    }
                    break;

                case Direction.Left:
                    if (objRect.Right <= sourceRect.Left &&
                        objRect.Bottom > sourceRect.Center.Y - lightWidth / 2f &&
                        objRect.Top < sourceRect.Center.Y + lightWidth / 2f)
                    {
                        distance = sourceRect.Left - objRect.Center.X;
                    }
                    break;

                case Direction.Right:
                    if (objRect.Left >= sourceRect.Right &&
                        objRect.Bottom > sourceRect.Center.Y - lightWidth / 2f &&
                        objRect.Top < sourceRect.Center.Y + lightWidth / 2f)
                    {
                        distance = objRect.Center.X - sourceRect.Right;
                    }
                    break;
            }

            if (distance >= 0 && distance < minDistance)
                minDistance = distance;
        }
        
        if (Math.Abs(minDistance - float.MaxValue) < 0.001f)
        {
            switch (direction)
            {
                case Direction.Up:    minDistance = sourceRect.Top; break;
                case Direction.Down:  minDistance = viewport.Height - sourceRect.Bottom; break;
                case Direction.Left:  minDistance = sourceRect.Left; break;
                case Direction.Right: minDistance = viewport.Width - sourceRect.Right; break;
            }
        }
        
        switch (direction)
        {
            case Direction.Up:
                Width = lightWidth;
                Height = (int)minDistance;
                Position = new Vector2(sourceRect.Center.X - lightWidth / 2f, sourceRect.Top - Height);
                break;
            case Direction.Down:
                Width = lightWidth;
                Height = (int)minDistance;
                Position = new Vector2(sourceRect.Center.X - lightWidth / 2f, sourceRect.Bottom);
                break;
            case Direction.Left:
                Width = (int)minDistance;
                Height = lightWidth;
                Position = new Vector2(sourceRect.Left - Width, sourceRect.Center.Y - lightWidth / 2f);
                break;
            case Direction.Right:
                Width = (int)minDistance;
                Height = lightWidth;
                Position = new Vector2(sourceRect.Right, sourceRect.Center.Y - lightWidth / 2f);
                break;
        }
    }
}