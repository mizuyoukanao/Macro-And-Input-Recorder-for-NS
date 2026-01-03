namespace MacroRecorder.Models;

public readonly record struct GyroState(short Roll, short Pitch, short Yaw)
{
    public static GyroState Neutral => new(0, 0, 0);

    public static GyroState FromDegrees(double roll, double pitch, double yaw)
    {
        // Conversion that mirrors joycon-quat quaternion math: deg -> radians -> scaled signed 16-bit
        const double scale = 16.384; // 8192 / 500deg/s approximated for Pro Controller
        var r = (short)Math.Clamp(roll * scale, short.MinValue, short.MaxValue);
        var p = (short)Math.Clamp(pitch * scale, short.MinValue, short.MaxValue);
        var y = (short)Math.Clamp(yaw * scale, short.MinValue, short.MaxValue);
        return new GyroState(r, p, y);
    }
}
