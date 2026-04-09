using System;
using Microsoft.Xna.Framework;

namespace PrismWeaver.Content;

public class Platform : GameObject
{
    public Platform(Vector2 startPosition, int width = 100, int height = 20)
        : base(startPosition, width, height)
    {
        
    }
}