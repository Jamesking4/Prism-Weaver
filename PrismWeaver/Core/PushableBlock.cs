using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PrismWeaver.Entities.Players;
using PrismWeaver.Utilities;

namespace PrismWeaver.Core;

public abstract class PushableBlock : DynamicObject
{
    public Color Color { get; private set; }
    private Texture2D texture;
    private const float Friction = 0.92f;
    private const float StopThreshold = 0.05f;

    protected PushableBlock(GraphicsDeviceManager graphics, Vector2 startPosition, int width, int height,
        List<GameObject> gameObjects, float maxVelocityX = 2f)
        : base(graphics, startPosition, width, height, gameObjects, maxVelocityX)
    {
        IsPushable = true;
    }

    public void Initialize(Color color, Texture2D texture)
    {
        Color = color;
        this.texture = texture;
    }

    public override void Update(GameTime gameTime)
    {
        ApplyFriction();
        BaseMove();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, DrawRectangle, Color);
    }

    private void ApplyFriction()
    {
        var player = GameObjects.OfType<Player>().FirstOrDefault();
        var isPlayerPushing = player != null &&
                               (CollisionRectangle.IsRectangleLeft(player.CollisionRectangle) ||
                                CollisionRectangle.IsRectangleRight(player.CollisionRectangle));

        if (!isPlayerPushing)
        {
            Velocity.X *= Friction;
            if (Math.Abs(Velocity.X) < StopThreshold)
                Velocity.X = 0;
        }
    }
}