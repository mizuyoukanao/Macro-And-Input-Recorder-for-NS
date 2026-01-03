using MacroRecorder.Models;

namespace MacroRecorder.Services;

public sealed class MacroGenerator
{
    public List<ControllerFrame> BuildFrames(MacroDefinition macro)
    {
        var frames = new List<ControllerFrame>();
        var frameDuration = TimeSpan.FromMilliseconds(macro.FrameIntervalMs);
        var time = TimeSpan.Zero;

        foreach (var step in macro.Steps)
        {
            for (var i = 0; i < step.Frames; i++)
            {
                frames.Add(new ControllerFrame(
                    time,
                    step.Buttons,
                    step.LeftStick,
                    step.RightStick,
                    step.Gyro));
                time += frameDuration;
            }
        }

        return frames;
    }
}
