using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PrismWeaver.Content;

public class Light : GameObject
{
    private Color color;
    private Direction direction;
    private Texture2D texture;
    private bool enabled;
    
    public Light(Vector2 startPosition, int width, int height, Color color, bool isWork, Direction direction, Texture2D texture) 
        : base(startPosition, width, height)
    {
        this.color = color;
        this.direction = direction;
        enabled = isWork;
        
        this.texture = texture;
        texture.SetData([color]);
    }

    public override void Update(GameTime gameTime)
    {
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (enabled)
            spriteBatch.Draw(texture, Rectangle, color);
    }
}