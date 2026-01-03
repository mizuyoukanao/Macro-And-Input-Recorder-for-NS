namespace MacroRecorder.Models;

public readonly record struct AnalogStickState(short X, short Y)
{
    public static AnalogStickState Center => new(0, 0);

    public AnalogStickState Clamp(short min = -2048, short max = 2047)
    {
        var clampedX = Math.Clamp(X, min, max);
        var clampedY = Math.Clamp(Y, min, max);
        return new AnalogStickState((short)clampedX, (short)clampedY);
    }
}
