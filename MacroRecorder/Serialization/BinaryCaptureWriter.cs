using System.Text;
using MacroRecorder.Models;

namespace MacroRecorder.Serialization;

public static class BinaryCaptureWriter
{
    public static void Write(string path, CaptureSession session)
    {
        using var stream = File.Open(path, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: false);
        writer.Write(session.Frames.Count);
        foreach (var frame in session.Frames)
        {
            writer.Write(frame.Timestamp.Ticks);
            var report = frame.ToHidReport();
            writer.Write(report.Length);
            writer.Write(report);
        }
    }

    public static CaptureSession Read(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);
        var count = reader.ReadInt32();
        var frames = new List<ControllerFrame>(count);
        for (var i = 0; i < count; i++)
        {
            var ticks = reader.ReadInt64();
            var length = reader.ReadInt32();
            var data = reader.ReadBytes(length);
            var packet = new Protocols.ProControllerPacket(data);
            frames.Add(packet.ToFrame(TimeSpan.FromTicks(ticks)));
        }

        return new CaptureSession(frames);
    }
}
