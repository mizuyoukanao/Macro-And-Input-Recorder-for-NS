namespace MacroRecorder.Models;

public sealed class MacroDefinition
{
    public string Name { get; set; } = "macro";
    public int FrameIntervalMs { get; set; } = 8;
    public List<MacroStep> Steps { get; set; } = new();
}

public sealed class MacroStep
{
    public int Frames { get; set; } = 1;
    public ButtonState Buttons { get; set; }
    public AnalogStickState LeftStick { get; set; } = AnalogStickState.Center;
    public AnalogStickState RightStick { get; set; } = AnalogStickState.Center;
    public GyroState Gyro { get; set; } = GyroState.Neutral;
}
