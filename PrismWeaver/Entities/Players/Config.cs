using Microsoft.Xna.Framework;

namespace PrismWeaver.Entities.Players;

public static class Config
{
    public static int FrameWidth = 128;
    public static int FrameHeight = 128;
    public static int RealFrameWidth = 38;
    public static int RealFrameHeight = 70;
    public static Point FrameSize = new(FrameWidth, FrameHeight);

    public static int CountFrameIdle = 5;
    public static int CountFrameRun = 6;
    public static int CountFrameJump = 6;
    public static int CountFrameDie = 8;

    public static float FrameTimeIdle = 0.12f;
    public static float FrameTimeRun = 0.08f;
    public static float FrameTimeJump = 0.09f;
    public static float FrameTimeDie = 0.1f;
}