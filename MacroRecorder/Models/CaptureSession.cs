namespace MacroRecorder.Models;

public sealed class CaptureSession
{
    public CaptureSession(IEnumerable<ControllerFrame> frames)
    {
        Frames = frames.OrderBy(f => f.Timestamp).ToList();
    }

    public List<ControllerFrame> Frames { get; }
}
