using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PrismWeaver.Core;
using PrismWeaver.Entities.Lights;
using PrismWeaver.Utilities;

public abstract class OpticalBlock : PushableBlock
{
    protected GraphicsDeviceManager graphics;
    protected List<GameObject> gameObjects;
    protected Dictionary<Light, (LightSource Source, Direction Direction)> lights;
    protected HashSet<Light> addedLights;

    protected OpticalBlock(
        GraphicsDeviceManager graphics,
        Vector2 startPosition,
        int width,
        int height,
        List<GameObject> gameObjects,
        float maxVelocityX = 2
    ) : base(graphics, startPosition, width, height, gameObjects, maxVelocityX)
    {
        this.graphics = graphics;
        this.gameObjects = gameObjects;
        lights = new Dictionary<Light, (LightSource, Direction)>();
        addedLights = new HashSet<Light>();
    }

    public void AddNewLight(Light light, Direction incomingDirection, Color incomingColor, Stack<GameObject> newObjects, Rectangle intersect, int lightWidth)
    {
        if (!addedLights.Add(light))
        {
            UpdateStatus(light, true, intersect, incomingDirection);
            return;
        }

        var outgoingDirection = GetOutgoingDirection(incomingDirection);
        var mixedColor = MixColors(incomingColor, GetBlockColor());

        var lightPosition = GetLightPosition(intersect, outgoingDirection, lightWidth);
        var lightSource = new LightSource(lightPosition, lightWidth, lightWidth);
        lightSource.ParentOpticalBlock = this;

        lightSource.Initialize(
            graphics,
            new Texture2D(graphics.GraphicsDevice, 1, 1),
            mixedColor,
            outgoingDirection,
            new Texture2D(graphics.GraphicsDevice, 1, 1),
            gameObjects,
            newObjects
        );

        if (!lights.ContainsKey(light))
            lights.Add(light, (lightSource, outgoingDirection));
        lightSource.IsColliding = false;

        newObjects.Push(lightSource);
        newObjects.Push(lightSource.GetLight());
    }

    public void UpdateStatus(Light light, bool isWork, Rectangle intersect, Direction direction)
    {
        if (lights.TryGetValue(light, out var entry))
        {
            entry.Source.SetIsWork(isWork);
            var newPosition = GetLightPosition(intersect, entry.Direction, entry.Source.CollisionRectangle.Width);
            entry.Source.Move(newPosition);
        }
    }

    protected virtual Vector2 GetLightPosition(Rectangle intersect, Direction direction, int lightWidth)
    {
        return direction switch
        {
            Direction.Up    => new Vector2(intersect.Right - lightWidth + 1, intersect.Top),
            Direction.Down  => new Vector2(intersect.Right - lightWidth + 1, intersect.Bottom),
            Direction.Left  => new Vector2(intersect.Left + lightWidth, intersect.Center.Y - 1),
            Direction.Right => new Vector2(intersect.Right - lightWidth, intersect.Center.Y - 1),
            _               => new Vector2(intersect.X, intersect.Y)
        };
    }
    
    protected abstract Direction GetOutgoingDirection(Direction incoming);
    protected abstract Color MixColors(Color incoming, Color blockColor);
    protected abstract Color GetBlockColor();
}