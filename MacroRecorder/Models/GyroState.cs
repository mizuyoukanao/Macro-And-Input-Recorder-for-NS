namespace MacroRecorder.Models;

public readonly record struct GyroState(short Roll, short Pitch, short Yaw)
{
    public const double DegreesScale = 16.384;
    public static GyroState Neutral => new(0, 0, 0);

    public static GyroState FromDegrees(double roll, double pitch, double yaw)
    {
        // Conversion that mirrors joycon-quat quaternion math: deg -> radians -> scaled signed 16-bit
        var r = (short)Math.Clamp(roll * DegreesScale, short.MinValue, short.MaxValue);
        var p = (short)Math.Clamp(pitch * DegreesScale, short.MinValue, short.MaxValue);
        var y = (short)Math.Clamp(yaw * DegreesScale, short.MinValue, short.MaxValue);
        return new GyroState(r, p, y);
    }
}
