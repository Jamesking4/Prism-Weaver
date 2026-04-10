using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PrismWeaver.Content.textures;

namespace PrismWeaver.Content;

public class GlassBlock : DynamicObject
{
    private Color color;
    private Texture2D texture;
    private Vector2 velocity;
    private float MaxVelocityX = 2f;
    private float DiffVelocityX = 0.1f;
    private float Epsilon = 0.05f;


    public GlassBlock(GraphicsDeviceManager graphics, Vector2 startPosition, int width, int height, 
        List<GameObject> gameObjects, float MaxVelocityX = 4) : base(graphics, startPosition, width, height, gameObjects, MaxVelocityX)
    {
    }

    public void Initialize(Color color, Texture2D texture)
    {
        this.color = color;
        this.texture = texture;
    }

    public override void Update(GameTime gameTime)
    {
        MoveByPlayer();
        var rectangles = gameObjects
            .Where(obj => obj is not GlassBlock && obj is not Light)
            .Select(obj => obj.CollisionRectangle)
            .ToList();
        velocity = Gravitation.AffectGravitation(graphics, velocity, CollisionRectangle, rectangles);
        BaseMove();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, DrawRectangle, color);
    }

    private void Move()
    {
        if (velocity.X > MaxVelocityX)
            velocity.X = MaxVelocityX;
        if (velocity.X < -MaxVelocityX)
            velocity.X = -MaxVelocityX;

        Position = new Vector2(Position.X + velocity.X, Position.Y + velocity.Y);

        foreach (var gameObject in gameObjects)
        {
            if (gameObject is GlassBlock or Light or Player)
                continue;

            if (!CollisionRectangle.Intersects(gameObject.CollisionRectangle))
                continue;
            
            if ((CollisionRectangle.Bottom <
                 gameObject.CollisionRectangle.Top + gameObject.CollisionRectangle.Height / 2)
                && !(CollisionRectangle.IsPlatformLeft(gameObject.CollisionRectangle) ||
                     CollisionRectangle.IsPlatformRight(gameObject.CollisionRectangle)))
            {
                Position = new Vector2(Position.X, gameObject.CollisionRectangle.Top - collisionSize.Y);
                velocity.Y = 0;
            }
            else if ((CollisionRectangle.Top >
                      gameObject.CollisionRectangle.Bottom - gameObject.CollisionRectangle.Height / 2)
                     && !(CollisionRectangle.IsPlatformLeft(gameObject.CollisionRectangle) ||
                          CollisionRectangle.IsPlatformRight(gameObject.CollisionRectangle)))
            {
                Position = new Vector2(Position.X, gameObject.CollisionRectangle.Bottom);
                velocity.Y = 0;
            }

            else if (CollisionRectangle.Right < gameObject.CollisionRectangle.Right - gameObject.CollisionRectangle.Width / 2)
            {
                Position = new Vector2(gameObject.CollisionRectangle.Left - collisionSize.X, Position.Y);
                velocity.X = 0;
            }
            else if (CollisionRectangle.Left > gameObject.CollisionRectangle.Right - gameObject.CollisionRectangle.Width / 2)
            {
                Position = new Vector2(gameObject.CollisionRectangle.Right, Position.Y);
                velocity.X = 0;
            }
        }
    }

    public void MoveNone()
    {
        if (Math.Abs(velocity.X) < 2 * DiffVelocityX)
        {
            velocity.X = 0;
        }
        else if (velocity.X > Epsilon)
        {
            velocity.X -= DiffVelocityX;
        }
        else if (velocity.X < -Epsilon)
        {
            velocity.X += DiffVelocityX;
        }
    }

    private void MoveByPlayer()
    {
        var player = gameObjects.OfType<Player>().FirstOrDefault();
        if (player == null) return;
        if (CollisionRectangle.IsPlatformRight(player.CollisionRectangle))
        {
            velocity.X -= 0.1f;
            Move();
        }
        else if (CollisionRectangle.IsPlatformLeft(player.CollisionRectangle))
        {
            velocity.X += 0.1f;
            Move();
        }
        else
        {
            MoveNone();
            Move();
        }
    }
}