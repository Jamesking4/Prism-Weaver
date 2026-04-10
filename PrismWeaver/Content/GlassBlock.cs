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

    public GlassBlock(GraphicsDeviceManager graphics, Vector2 startPosition, int width, int height, 
        List<GameObject> gameObjects, float maxVelocityX = 2f) 
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
        ApplyPlayerPush();
        BaseMove();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, DrawRectangle, color);
    }

    private void ApplyPlayerPush()
    {
        var player = gameObjects.OfType<Player>().FirstOrDefault();
        if (player == null)
            return;

        if (CollisionRectangle.IsRectangleRight(player.CollisionRectangle))
            Velocity.X -= 0.15f;
        else if (CollisionRectangle.IsRectangleLeft(player.CollisionRectangle))
            Velocity.X += 0.15f;
        else
            Velocity.X *= 0.8f;
    }
}