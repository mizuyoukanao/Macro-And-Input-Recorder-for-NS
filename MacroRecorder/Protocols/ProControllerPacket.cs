using MacroRecorder.Configuration;
using MacroRecorder.Models;
using MacroRecorder.Services;

namespace MacroRecorder.Protocols;

public readonly record struct ProControllerPacket(byte[] Raw)
{
    public static ProControllerPacket FromHexLine(string line)
    {
        var bytes = Convert.FromHexString(line);
        return new ProControllerPacket(bytes);
    }

    public ControllerFrame ToFrame(TimeSpan timestamp, MotionEncoding motionEncoding = MotionEncoding.RawGyro)
    {
        var layout = DetectLayout(motionEncoding);
        if (Raw.Length < layout.RequiredLength)
        {
            throw new InvalidOperationException("Packet too short to decode");
        }

        var buttons = (ButtonState)(Raw[layout.ButtonOffset] | (Raw[layout.ButtonOffset + 1] << 8) | (Raw[layout.ButtonOffset + 2] << 16));
        var left = DecodeStick(Raw.AsSpan(layout.LeftStickOffset));
        var right = DecodeStick(Raw.AsSpan(layout.RightStickOffset));
        var gyro = DecodeMotion(Raw.AsSpan(layout.MotionOffset), motionEncoding);
        return new ControllerFrame(timestamp, buttons, left, right, gyro);
    }

    private PacketLayout DetectLayout(MotionEncoding motionEncoding)
    {
        // Standard Switch input reports keep buttons at bytes 3-5, sticks at 6/9,
        // accelerometer at 13-18 and gyroscope at 19-24. The compact reports this
        // app serializes for playback omit timer/battery and acceleration fields.
        if (Raw.Length >= 25)
        {
            return new PacketLayout(25, 3, 6, 9, motionEncoding == MotionEncoding.Quaternion ? 13 : 19);
        }

        return motionEncoding == MotionEncoding.Quaternion
            ? new PacketLayout(20, 1, 4, 8, 12)
            : new PacketLayout(18, 1, 4, 8, 12);
    }

    private static AnalogStickState DecodeStick(ReadOnlySpan<byte> buffer)
    {
        var x = (short)(buffer[0] | ((buffer[1] & 0x0F) << 8));
        var y = (short)((buffer[1] >> 4) | (buffer[2] << 4));
        return new AnalogStickState(x, y).Clamp();
    }

    private static GyroState DecodeMotion(ReadOnlySpan<byte> buffer, MotionEncoding motionEncoding)
    {
        return motionEncoding == MotionEncoding.Quaternion
            ? JoyconQuaternionCodec.Decode(buffer)
            : DecodeGyro(buffer);
    }

    private static GyroState DecodeGyro(ReadOnlySpan<byte> buffer)
    {
        var roll = (short)((buffer[1] << 8) | buffer[0]);
        var pitch = (short)((buffer[3] << 8) | buffer[2]);
        var yaw = (short)((buffer[5] << 8) | buffer[4]);
        return new GyroState(roll, pitch, yaw);
    }

    private readonly record struct PacketLayout(
        int RequiredLength,
        int ButtonOffset,
        int LeftStickOffset,
        int RightStickOffset,
        int MotionOffset);
}
