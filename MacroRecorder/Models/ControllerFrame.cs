using MacroRecorder.Configuration;
using MacroRecorder.Services;

namespace MacroRecorder.Models;

public sealed record ControllerFrame(
    TimeSpan Timestamp,
    ButtonState Buttons,
    AnalogStickState LeftStick,
    AnalogStickState RightStick,
    GyroState Gyro)
{
    public byte[] ToHidReport(MotionEncoding motionEncoding = MotionEncoding.RawGyro)
    {
        // HID report layout derived from dekuNukem Nintendo Switchプロトコル
        var report = new byte[motionEncoding == MotionEncoding.Quaternion ? 20 : 18];
        report[0] = 0x30; // standard input report id
        report[1] = (byte)((int)Buttons & 0xFF);
        report[2] = (byte)((int)Buttons >> 8);
        report[3] = (byte)((int)Buttons >> 16);
        InsertStick(report, 4, LeftStick);
        InsertStick(report, 8, RightStick);
        InsertMotion(report, 12, Gyro, motionEncoding);
        return report;
    }

    private static void InsertStick(byte[] buffer, int offset, AnalogStickState stick)
    {
        var clamped = stick.Clamp();
        var x = (ushort)(clamped.X + 2048);
        var y = (ushort)(clamped.Y + 2048);
        buffer[offset + 0] = (byte)(x & 0xFF);
        buffer[offset + 1] = (byte)(x >> 8);
        buffer[offset + 2] = (byte)(y & 0xFF);
        buffer[offset + 3] = (byte)(y >> 8);
    }

    private static void InsertMotion(byte[] buffer, int offset, GyroState gyro, MotionEncoding motionEncoding)
    {
        if (motionEncoding == MotionEncoding.Quaternion)
        {
            JoyconQuaternionCodec.Encode(buffer.AsSpan(offset, 8), gyro);
            return;
        }

        InsertGyro(buffer, offset, gyro);
    }

    private static void InsertGyro(byte[] buffer, int offset, GyroState gyro)
    {
        buffer[offset + 0] = (byte)(gyro.Roll & 0xFF);
        buffer[offset + 1] = (byte)((gyro.Roll >> 8) & 0xFF);
        buffer[offset + 2] = (byte)(gyro.Pitch & 0xFF);
        buffer[offset + 3] = (byte)((gyro.Pitch >> 8) & 0xFF);
        buffer[offset + 4] = (byte)(gyro.Yaw & 0xFF);
        buffer[offset + 5] = (byte)((gyro.Yaw >> 8) & 0xFF);
    }
}
