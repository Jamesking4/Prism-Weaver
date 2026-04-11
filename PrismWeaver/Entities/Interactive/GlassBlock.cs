using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PrismWeaver.Content;

public class GlassBlock : PushableBlock
{
    public GlassBlock(GraphicsDeviceManager graphics, Vector2 startPosition, int width, 
        int height, List<GameObject> gameObjects, float maxVelocityX = 3) : base(graphics, startPosition, width, height, gameObjects, maxVelocityX)
    {
    }
}