using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PrismWeaver.Core;

namespace PrismWeaver.Entities.Interactive;

public class Target : GameObject
{
    public Color ColorTarget { get; }
    
    private readonly Action<bool> onActivationChanged;
    Texture2D texture;
    private int activeBeamsCount;

    public Target(Vector2 startPosition, int width, int height, Action<bool> func, Texture2D texture, Color color, bool isColliding = true) 
        : base(startPosition, width, height, isColliding)
    {
        onActivationChanged = func;
        ColorTarget = color;
        this.texture = texture;
        activeBeamsCount = 0;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, DrawRectangle, ColorTarget);
    }
    
    public void ResetBeamCount()
    {
        activeBeamsCount = 0;
    }
    
    public void AddBeam()
    {
        activeBeamsCount++;
    }
    
    public void ApplyActivation()
    {
        var shouldBeActive = activeBeamsCount > 0;
        onActivationChanged(shouldBeActive);
    }
}