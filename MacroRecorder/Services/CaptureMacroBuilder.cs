using MacroRecorder.Models;

namespace MacroRecorder.Services;

public sealed class CaptureMacroBuilder
{
    public MacroDefinition Build(string name, CaptureSession session)
    {
        var macro = new MacroDefinition
        {
            Name = name,
            FrameIntervalMs = CalculateInterval(session)
        };

        if (session.Frames.Count == 0)
        {
            return macro;
        }

        ControllerFrame? previous = null;
        MacroStep? currentStep = null;

        foreach (var frame in session.Frames.OrderBy(f => f.Timestamp))
        {
            if (previous is not null && AreSameState(previous, frame) && currentStep is not null)
            {
                currentStep.Frames++;
            }
            else
            {
                currentStep = new MacroStep
                {
                    Frames = 1,
                    Buttons = frame.Buttons,
                    LeftStick = frame.LeftStick,
                    RightStick = frame.RightStick,
                    Gyro = frame.Gyro
                };
                macro.Steps.Add(currentStep);
            }

            previous = frame;
        }

        return macro;
    }

    private static bool AreSameState(ControllerFrame first, ControllerFrame second)
    {
        return first.Buttons == second.Buttons
               && first.LeftStick == second.LeftStick
               && first.RightStick == second.RightStick
               && first.Gyro == second.Gyro;
    }

    private static int CalculateInterval(CaptureSession session)
    {
        if (session.Frames.Count < 2)
        {
            return 8;
        }

        var deltas = new List<double>();
        var ordered = session.Frames.OrderBy(f => f.Timestamp).ToList();
        for (var i = 1; i < ordered.Count; i++)
        {
            deltas.Add((ordered[i].Timestamp - ordered[i - 1].Timestamp).TotalMilliseconds);
        }

        var average = deltas.Average();
        return Math.Max(1, (int)Math.Round(average));
    }
}
