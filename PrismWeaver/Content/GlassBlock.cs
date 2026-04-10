using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrismWeaver.Content;

public class GlassBlock : DynamicObject
{
    private Color color;
    private Texture2D texture;
    
    private const float PushForce = 1f;
    private const float Friction = 0.8f;
    private const float StopThreshold = 0.05f;

    public GlassBlock(GraphicsDeviceManager graphics, Vector2 startPosition, int width, int height, 
        List<GameObject> gameObjects, float maxVelocityX = 3f) 
        : base(graphics, startPosition, width, height, gameObjects, maxVelocityX)
    {
        IsPushable = true;
    }

    public void Initialize(Color color, Texture2D texture)
    {
        this.color = color;
        this.texture = texture;
    }

    public override void Update(GameTime gameTime)
    {
        ApplyPlayerForce();
        ApplyFriction();
        BaseMove();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, DrawRectangle, color);
    }

    private void ApplyPlayerForce()
    {
        var player = gameObjects.OfType<Player>().FirstOrDefault();
        if (player == null) return;

        if (CollisionRectangle.IsRectangleRight(player.CollisionRectangle))
            Velocity.X -= PushForce;
        else if (CollisionRectangle.IsRectangleLeft(player.CollisionRectangle))
            Velocity.X += PushForce;
    }
    
    private void ApplyFriction()
    {
        var player = gameObjects.OfType<Player>().FirstOrDefault();
        var isPlayerPushing = player != null && 
            (CollisionRectangle.IsRectangleRight(player.CollisionRectangle) ||
             CollisionRectangle.IsRectangleLeft(player.CollisionRectangle));

        if (!isPlayerPushing)
        {
            Velocity.X *= Friction;
            if (Math.Abs(Velocity.X) < StopThreshold)
                Velocity.X = 0;
        }
    }
}