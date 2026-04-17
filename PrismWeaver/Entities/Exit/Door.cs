using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PrismWeaver.Core;

namespace PrismWeaver.Entities.Exit;

public class Door : GameObject
{
    public bool isOpen { get; set; }
    Texture2D texture;
    
    public Door(Vector2 startPosition, int width, int height, Texture2D texture,bool isColliding = false) 
        : base(startPosition, width, height, isColliding)
    {
        this.texture = texture;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!isOpen)
            spriteBatch.Draw(texture, DrawRectangle, Color.Black);
    }

    public void SetIsOpen(bool isOpen)
    {
        this.isOpen = isOpen;
    }
}