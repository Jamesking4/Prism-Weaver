using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = System.Numerics.Vector2;
namespace PrismWeaver.Content;

public static class Window
{
    public static (Vector2, Vector2) GetPositionAndVelocityInWindow(GraphicsDeviceManager graphics, Rectangle rectangle, Microsoft.Xna.Framework.Vector2 velocity)
    {
        var position = new Vector2(rectangle.X, rectangle.Y);
        var newVelocity = new Vector2(velocity.X, velocity.Y);;
        if (rectangle.Left < 0)
        {
            position.X = 0;
            newVelocity.X = 0;
        }

        if (rectangle.Right > graphics.GraphicsDevice.Viewport.Width)
        {
            position.X = graphics.GraphicsDevice.Viewport.Width - rectangle.Width;
            newVelocity.X = 0;
        }

        if (rectangle.Top < 0)
        {
            position.Y = 0;
            newVelocity.Y = 0;
        }

        if (rectangle.Bottom > graphics.GraphicsDevice.Viewport.Height)
        {
            position.Y = graphics.GraphicsDevice.Viewport.Height - rectangle.Height;
            newVelocity.Y = 0;
        }

        return (position, newVelocity);
    }
}