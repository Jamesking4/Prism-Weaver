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
    private int lightWidth = 1000;
    private int lightHeight = 5;
    private Color color;
    
    public LightSource(Vector2 startPosition, int width, int height) : base(startPosition, width, height)
    {
    }

    public void Initialize(Texture2D texture, Color color, Direction direction, Texture2D textureTemp, List<GameObject> gameObjects)
    {
        this.texture = textureTemp;
        this.direction = direction;
        this.color = color;
        
        light = new Light(GetStartLight(), lightWidth, lightHeight);
        light.Initialize(color, isWork, direction, texture, gameObjects);
    }

    public override void Update(GameTime gameTime)
    {
        
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, Rectangle, color);
    }

    public Light GetLight()
    {
        return light;
    }

    public void SetIsWork(bool isWork)
    {
        this.isWork = isWork;
    }

    public Vector2 GetStartLight()
    {
        var posX = direction == Direction.Left ? -Rectangle.Width / 2  : Rectangle.Width / 2;
        var posY = direction == Direction.Up ? -Rectangle.Height / 2 : Rectangle.Height / 2;
        return new Vector2(posX + Rectangle.X, posY + Rectangle.Y - lightHeight / 2);
    }
}