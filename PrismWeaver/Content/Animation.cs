using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrismWeaver.Content;

public class Animation
{
    private readonly Texture2D texture;
    private readonly List<Rectangle> frames;
    private int currentFrame;
    private double timer;
    private double frameTime;
    private bool loop;
    private int diffHeightFrame;
    
    public Animation(Texture2D texture, int frameWidth, int frameHeight, int frameCount, int realWidth, int realHeight, double frameTime = 0.1, bool loop = true)
    {
        this.texture = texture;
        this.frameTime = frameTime;
        this.loop = loop;
        diffHeightFrame = frameHeight - realHeight;
        
        frames = [];
        
        for (var i = 0; i < frameCount; i++)
        {
            frames.Add(new Rectangle(i * frameWidth + (frameWidth - realWidth) / 2, 0, realWidth, frameHeight));
        }
        currentFrame = 0;
        timer = 0;
    }

    public void Update(GameTime gameTime)
    {
        if (IsFinished) 
            return;
        
        timer += gameTime.ElapsedGameTime.TotalSeconds;
        if (timer >= frameTime)
        {
            timer = 0;
            currentFrame = (currentFrame + 1) % frames.Count;
        }
    }

    public void ChangeFrameTime(double frameTime)
    {
        this.frameTime = frameTime;
    }
    
    public void Draw(SpriteBatch spriteBatch, Vector2 position, SpriteEffects effect = SpriteEffects.None)
    {
        position.Y -= diffHeightFrame;
        spriteBatch.Draw(texture, position, frames[currentFrame], Color.White, 0f, Vector2.Zero, 1f, effect, 0f);
    }

    public void Reset()
    {
        currentFrame = 0;
        timer = 0;
    }
    
    public bool IsFinished => !loop && currentFrame == frames.Count - 1;
}