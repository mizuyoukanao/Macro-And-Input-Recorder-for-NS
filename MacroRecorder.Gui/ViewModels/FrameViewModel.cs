using MacroRecorder.Models;
using MacroRecorder.Services;

namespace MacroRecorder.Gui.ViewModels;

public sealed class FrameViewModel
{
    public FrameViewModel(ControllerFrame frame)
    {
        TimestampMs = frame.Timestamp.TotalMilliseconds.ToString("F1");
        Buttons = ButtonStateConverter.ToCsv(frame.Buttons);
        LeftStick = $"X:{frame.LeftStick.X} Y:{frame.LeftStick.Y}";
        RightStick = $"X:{frame.RightStick.X} Y:{frame.RightStick.Y}";
        Gyro = $"R:{frame.Gyro.Roll} P:{frame.Gyro.Pitch} Y:{frame.Gyro.Yaw}";
    }

    public string TimestampMs { get; }
    public string Buttons { get; }
    public string LeftStick { get; }
    public string RightStick { get; }
    public string Gyro { get; }
}
