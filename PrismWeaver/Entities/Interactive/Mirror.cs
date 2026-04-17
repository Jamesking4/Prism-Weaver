using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PrismWeaver.Core;
using PrismWeaver.Entities.Lights;
using PrismWeaver.Utilities;

public class Mirror : OpticalBlock
{
    private readonly Direction reflectionDirection;

    public Mirror(
        GraphicsDeviceManager graphics,
        Vector2 startPosition,
        int width,
        int height,
        List<GameObject> gameObjects,
        Direction reflectionDirection,
        float maxVelocityX = 2
    ) : base(graphics, startPosition, width, height, gameObjects, maxVelocityX)
    {
        this.reflectionDirection = reflectionDirection;
    }

    protected override Direction GetOutgoingDirection(Direction incoming) => reflectionDirection;

    protected override Color MixColors(Color incoming, Color blockColor) => incoming;

    protected override Color GetBlockColor() => Color.White;
}