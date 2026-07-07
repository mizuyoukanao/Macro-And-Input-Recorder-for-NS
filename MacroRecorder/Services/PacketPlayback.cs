using MacroRecorder.Configuration;
using MacroRecorder.Models;

namespace MacroRecorder.Services;

public sealed class PacketPlayback
{
    private readonly SwitchSerialSender _sender;
    private readonly CaptureSession _session;
    private readonly MotionEncoding _motionEncoding;

    public PacketPlayback(SwitchSerialSender sender, CaptureSession session, MotionEncoding motionEncoding = MotionEncoding.RawGyro)
    {
        _sender = sender;
        _session = session;
        _motionEncoding = motionEncoding;
    }

    public async Task PlayAsync(bool loop)
    {
        do
        {
            var start = DateTime.UtcNow;
            foreach (var frame in _session.Frames)
            {
                var delay = frame.Timestamp - (DateTime.UtcNow - start);
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay);
                }
                _sender.SendPacket(frame.ToHidReport(_motionEncoding));
            }
        } while (loop);
    }
}
