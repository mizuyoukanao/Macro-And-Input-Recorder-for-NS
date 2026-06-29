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

        var orderedFrames = session.Frames.OrderBy(f => f.Timestamp).ToList();
        var currentStep = CreateStep(orderedFrames[0]);

        for (var i = 1; i < orderedFrames.Count; i++)
        {
            var previous = orderedFrames[i - 1];
            var current = orderedFrames[i];
            var framesForPrevious = CalculateFramesForInterval(previous.Timestamp, current.Timestamp, macro.FrameIntervalMs);

            if (AreSameState(previous, current))
            {
                currentStep.Frames += framesForPrevious;
                continue;
            }

            currentStep.Frames += framesForPrevious;
            macro.Steps.Add(currentStep);
            currentStep = CreateStep(current);
        }

        if (currentStep.Frames == 0)
        {
            currentStep.Frames = 1;
        }
        macro.Steps.Add(currentStep);

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

    private static MacroStep CreateStep(ControllerFrame frame)
    {
        return new MacroStep
        {
            Frames = 0,
            Buttons = frame.Buttons,
            LeftStick = frame.LeftStick,
            RightStick = frame.RightStick,
            Gyro = frame.Gyro
        };
    }

    private static int CalculateFramesForInterval(TimeSpan previous, TimeSpan current, int intervalMs)
    {
        if (intervalMs <= 0)
        {
            return 1;
        }

        var deltaMs = (current - previous).TotalMilliseconds;
        return Math.Max(1, (int)Math.Round(deltaMs / intervalMs));
    }
}
