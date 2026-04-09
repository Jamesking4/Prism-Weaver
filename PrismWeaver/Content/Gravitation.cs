using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace PrismWeaver.Content;

public class Gravitation
{
    private const float GravitationalConstant = 0.2f;
    
    public Vector2 AffectGravitation(GraphicsDeviceManager graphics, Vector2 velocity, Rectangle rectangle, List<Rectangle> rectangles)
    {
        if (!IsGrounded(graphics, rectangle, rectangles))
        {
            return velocity with { Y = velocity.Y + GravitationalConstant };
        }
        
        return velocity;
    }
    
    public bool IsGrounded(GraphicsDeviceManager graphics, Rectangle rectangle, List<Rectangle> rectangles)
    {
        return rectangle.Bottom >= graphics.GraphicsDevice.Viewport.Height 
               || rectangles.Any(checkRectangle => rectangle.IsPlatformDown(checkRectangle));
    }

    public float GetGravitationConst() => GravitationalConstant;
}