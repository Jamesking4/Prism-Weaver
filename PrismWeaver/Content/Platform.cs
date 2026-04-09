using System;
using Microsoft.Xna.Framework;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace PrismWeaver.Content;

public class Platform
{
    public Vector2 position { get; }
    public int width { get; } = 100;
    public int height { get; } = 20;
    public Rectangle drawRectangle { get; }

    public Platform(Vector2 position)
    {
        this.position = position;
        drawRectangle = new Rectangle((int)position.X, (int)position.Y, width, height);
    }
}