using MacroRecorder.Models;

namespace MacroRecorder.Protocols;

public readonly record struct ProControllerPacket(byte[] Raw)
{
    public static ProControllerPacket FromHexLine(string line)
    {
        var bytes = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(b => Convert.ToByte(b, 16))
            .ToArray();
        return new ProControllerPacket(bytes);
    }

    public ControllerFrame ToFrame(TimeSpan timestamp)
    {
        if (Raw.Length < 18)
        {
            throw new InvalidOperationException("Packet too short to decode");
        }

        var buttons = (ButtonState)(Raw[1] | (Raw[2] << 8) | (Raw[3] << 16));
        var left = DecodeStick(Raw.AsSpan(4));
        var right = DecodeStick(Raw.AsSpan(8));
        var gyro = DecodeGyro(Raw.AsSpan(12));
        return new ControllerFrame(timestamp, buttons, left, right, gyro);
    }

    private static AnalogStickState DecodeStick(ReadOnlySpan<byte> buffer)
    {
        var x = (short)(((buffer[1] << 8) | buffer[0]) - 2048);
        var y = (short)(((buffer[3] << 8) | buffer[2]) - 2048);
        return new AnalogStickState(x, y).Clamp();
    }

    private static GyroState DecodeGyro(ReadOnlySpan<byte> buffer)
    {
        var roll = (short)((buffer[1] << 8) | buffer[0]);
        var pitch = (short)((buffer[3] << 8) | buffer[2]);
        var yaw = (short)((buffer[5] << 8) | buffer[4]);
        return new GyroState(roll, pitch, yaw);
    }
}
