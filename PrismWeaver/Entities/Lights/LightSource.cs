using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrismWeaver.Content;

public class LightSource : GameObject
{
    private Direction direction;
    private bool isWork = true;
    private Texture2D texture;
    private Light light;
    private int lightWidth = 5;
    private Color color;
    
    public LightSource(Vector2 startPosition, int width, int height) : base(startPosition, width, height)
    {
    }

    public void Initialize(GraphicsDeviceManager graphics, Texture2D texture, Color color, Direction direction, Texture2D textureTemp, List<GameObject> gameObjects)
    {
        this.texture = textureTemp;
        this.direction = direction;
        this.color = color;
        
        light = new Light(this ,lightWidth);
        light.Initialize(graphics, color, isWork, direction, texture, gameObjects);
    }

    public override void Update(GameTime gameTime)
    {
        
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, DrawRectangle, color);
    }

    public Light GetLight()
    {
        return light;
    }

    public void SetIsWork(bool isWork)
    {
        this.isWork = isWork;
    }
}