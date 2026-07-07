using MacroRecorder.Models;

namespace MacroRecorder.Services;

public static class JoyconQuaternionCodec
{
    private const double Scale = 16384.0;

    public static GyroState Decode(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 8)
        {
            return DecodeRawGyro(buffer);
        }

        var x = ReadInt16(buffer, 0) / Scale;
        var y = ReadInt16(buffer, 2) / Scale;
        var z = ReadInt16(buffer, 4) / Scale;
        var w = ReadInt16(buffer, 6) / Scale;
        Normalize(ref w, ref x, ref y, ref z);

        var roll = Math.Atan2(2.0 * (w * x + y * z), 1.0 - 2.0 * (x * x + y * y));
        var sinp = 2.0 * (w * y - z * x);
        var pitch = Math.Abs(sinp) >= 1.0 ? Math.CopySign(Math.PI / 2.0, sinp) : Math.Asin(sinp);
        var yaw = Math.Atan2(2.0 * (w * z + x * y), 1.0 - 2.0 * (y * y + z * z));

        return GyroState.FromDegrees(ToDegrees(roll), ToDegrees(pitch), ToDegrees(yaw));
    }

    public static void Encode(Span<byte> buffer, GyroState gyro)
    {
        if (buffer.Length < 8)
        {
            throw new ArgumentException("Quaternion motion data requires 8 bytes.", nameof(buffer));
        }

        var roll = ToRadians(gyro.Roll / GyroState.DegreesScale);
        var pitch = ToRadians(gyro.Pitch / GyroState.DegreesScale);
        var yaw = ToRadians(gyro.Yaw / GyroState.DegreesScale);

        var cy = Math.Cos(yaw * 0.5);
        var sy = Math.Sin(yaw * 0.5);
        var cp = Math.Cos(pitch * 0.5);
        var sp = Math.Sin(pitch * 0.5);
        var cr = Math.Cos(roll * 0.5);
        var sr = Math.Sin(roll * 0.5);

        var w = cr * cp * cy + sr * sp * sy;
        var x = sr * cp * cy - cr * sp * sy;
        var y = cr * sp * cy + sr * cp * sy;
        var z = cr * cp * sy - sr * sp * cy;
        Normalize(ref w, ref x, ref y, ref z);

        WriteInt16(buffer, 0, Quantize(x));
        WriteInt16(buffer, 2, Quantize(y));
        WriteInt16(buffer, 4, Quantize(z));
        WriteInt16(buffer, 6, Quantize(w));
    }

    private static GyroState DecodeRawGyro(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 6)
        {
            throw new InvalidOperationException("Packet too short to decode motion data");
        }
        return new GyroState(ReadInt16(buffer, 0), ReadInt16(buffer, 2), ReadInt16(buffer, 4));
    }

    private static short ReadInt16(ReadOnlySpan<byte> buffer, int offset) => (short)(buffer[offset] | (buffer[offset + 1] << 8));
    private static void WriteInt16(Span<byte> buffer, int offset, short value)
    {
        buffer[offset] = (byte)(value & 0xFF);
        buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
    }
    private static short Quantize(double value) => (short)Math.Clamp(Math.Round(value * Scale), short.MinValue, short.MaxValue);
    private static double ToDegrees(double radians) => radians * 180.0 / Math.PI;
    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    private static void Normalize(ref double w, ref double x, ref double y, ref double z)
    {
        var length = Math.Sqrt(w * w + x * x + y * y + z * z);
        if (length <= double.Epsilon)
        {
            w = 1.0; x = y = z = 0.0; return;
        }
        w /= length; x /= length; y /= length; z /= length;
    }
}
