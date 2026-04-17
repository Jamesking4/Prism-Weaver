using Microsoft.Xna.Framework;

namespace PrismWeaver;

public static class GameConfig
{
    public const int WindowWidth = 1280;
    public const int WindowHeight = 720;
    public const bool StartFullScreen = true;
    
    public const int PlatformCount = 8;
    public const int PlatformStartIndex = 2;
    public const int PlatformOffsetX = 100;
    public const int PlatformYOffsetFromBottom = 120;
    
    public const int GlassBlockSize = 40;
    public const float GlassBlockMaxSpeed = 2f;
    public static readonly Vector2 GlassBlock1Position = new(250, 0);
    public static readonly Vector2 GlassBlock2Position = new(420, 0);
    public static readonly Color GlassBlockColor = Color.White;
    public static readonly Color GlassBlockOutlineColor = Color.White * 0.3f;
    
    public const int LightSourceSize = 30;
    public static readonly Vector2 LightSourceOffsetFromBottom = new(0, 160);
    public static readonly Color LightSourceColor = Color.White;
    
    public static readonly Vector2 PlayerStartPosition = new(50, 800);
    
    public const float MusicVolume = 1f;
}