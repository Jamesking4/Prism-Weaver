using System;
using Microsoft.Xna.Framework;

namespace PrismWeaver.Content;

public static class RectangleExtensions
{
    public static bool IsRectangleUp(this Rectangle thisRectangle, Rectangle platformRect, float CollisionTolerance = 2f)
    {
        var verticalTouch = Math.Abs(platformRect.Bottom - thisRectangle.Top) <= CollisionTolerance;
        var horizontalOverlap = thisRectangle.Right > platformRect.Left && thisRectangle.Left < platformRect.Right;
    
        return verticalTouch && horizontalOverlap;
    }

    public static bool IsRectangleDown(this Rectangle thisRectangle, Rectangle platformRect, float CollisionTolerance = 2f)
    {
        var verticalTouch = Math.Abs(thisRectangle.Bottom - platformRect.Top) <= CollisionTolerance;
        var horizontalOverlap = thisRectangle.Right > platformRect.Left && thisRectangle.Left < platformRect.Right;
    
        return verticalTouch && horizontalOverlap;
    }

    public static bool IsRectangleLeft(this Rectangle thisRectangle, Rectangle checkRectangle, float CollisionTolerance = 2f)
    {
        var platformRect = checkRectangle;
        var horizontalTouch = Math.Abs(platformRect.Right - thisRectangle.Left) <= CollisionTolerance;
        var verticalOverlap = thisRectangle.Bottom > platformRect.Top + CollisionTolerance 
                              && thisRectangle.Top < platformRect.Bottom - CollisionTolerance;
        
        return horizontalTouch && verticalOverlap;
    }

    public static bool IsRectangleRight(this Rectangle thisRectangle, Rectangle checkRectangle, float CollisionTolerance = 2f)
    {
        var platformRect = checkRectangle;
        var horizontalTouch = Math.Abs(platformRect.Left - thisRectangle.Right) <= CollisionTolerance;
        var verticalOverlap = thisRectangle.Bottom > platformRect.Top + CollisionTolerance 
                              && thisRectangle.Top < platformRect.Bottom - CollisionTolerance;
        
        return horizontalTouch && verticalOverlap;
    }

    public static int GetDistToRectangleOver(this Rectangle thisRectangle, Rectangle checkRectangle, float CollisionTolerance = 2f)
    {
        var platformRect = checkRectangle;
        var horizontalTouch = thisRectangle.Right > checkRectangle.Left 
                              && thisRectangle.Left < checkRectangle.Right;
        var isCheckRectangleOver = horizontalTouch && thisRectangle.Top > platformRect.Bottom;
        return isCheckRectangleOver? Math.Abs(platformRect.Bottom - thisRectangle.Top) : -1;
    }
}