using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PrismWeaver.Core;
using PrismWeaver.Entities.Lights;
using PrismWeaver.Utilities;

public class PrismBlock : OpticalBlock
{
    public Color ColorCurrent { get; set; } = Color.White;

    public PrismBlock(
        GraphicsDeviceManager graphics,
        Vector2 startPosition,
        int width,
        int height,
        List<GameObject> gameObjects,
        float maxVelocityX = 2
    ) : base(graphics, startPosition, width, height, gameObjects, maxVelocityX)
    {
    }
    
    public override void Initialize(Color color, Texture2D texture)
    {
        ColorCurrent = color;
        base.Initialize(color, texture);
    }

    protected override Direction GetOutgoingDirection(Direction incoming) => incoming;

    protected override Color MixColors(Color incoming, Color blockColor)
    {
        var r = incoming.R + blockColor.R;
        var g = incoming.G + blockColor.G;
        var b = incoming.B + blockColor.B;

        var max = Math.Max(r, Math.Max(g, b));
        if (max > 255)
        {
            var scale = 255f / max;
            r = (int)(r * scale);
            g = (int)(g * scale);
            b = (int)(b * scale);
        }
        return new Color(r, g, b);
    }

    protected override Color GetBlockColor() => ColorCurrent;
}