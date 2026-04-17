using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PrismWeaver.Core;
using PrismWeaver.Utilities;

namespace PrismWeaver.Entities.Lights;

public class LightSource : GameObject
{
    public OpticalBlock ParentOpticalBlock { get; set; }
    private Direction direction;
    private bool isWork = true;
    private Texture2D texture;
    private Light light;
    private int lightWidth = 5;
    private Color color;
    
    public LightSource(Vector2 startPosition, int width, int height) : base(startPosition, width, height)
    {
    }

    public void Initialize(GraphicsDeviceManager graphics, Texture2D texture, Color color, Direction direction, 
        Texture2D textureTemp, List<GameObject> gameObjects, Stack<GameObject> newObjects)
    {
        this.texture = textureTemp;
        this.direction = direction;
        this.color = color;
        
        light = new Light(this ,lightWidth);
        light.Initialize(graphics, color, direction, texture, gameObjects, newObjects);
    }

    public override void Update(GameTime gameTime)
    {
    }

    public void Move(Vector2 position)
    {
        Position = position;
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
    
    public bool IsWork() =>  this.isWork;
}