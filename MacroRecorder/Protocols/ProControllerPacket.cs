using MacroRecorder.Models;

namespace MacroRecorder.Protocols;

public readonly record struct ProControllerPacket(byte[] Raw)
{
    public static ProControllerPacket FromHexLine(string line)
    {
        var bytes = Convert.FromHexString(line);
        return new ProControllerPacket(bytes);
    }

    public ControllerFrame ToFrame(TimeSpan timestamp)
    {
        if (Raw.Length < 18)
        {
            throw new InvalidOperationException("Packet too short to decode");
        }

        var buttons = (ButtonState)(Raw[3] | (Raw[4] << 8) | (Raw[5] << 16));
        var left = DecodeStick(Raw.AsSpan(6));
        var right = DecodeStick(Raw.AsSpan(9));
        var gyro = DecodeGyro(Raw.AsSpan(13));
        return new ControllerFrame(timestamp, buttons, left, right, gyro);
    }

    private static AnalogStickState DecodeStick(ReadOnlySpan<byte> buffer)
    {
        var x = (short)(buffer[0] | ((buffer[1] & 0x0F) << 8));
        var y = (short)((buffer[1] >> 4) | (buffer[2] << 4));
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
