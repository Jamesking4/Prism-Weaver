using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrismWeaver.Content;

public class Platform : GameObject
{
    private readonly Texture2D texture;

    public Platform(Vector2 startPosition, Texture2D texture, int width = 100, int height = 20)
        : base(startPosition, width, height)
    {
        this.texture = texture;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, Rectangle, Color.White);
    }
}