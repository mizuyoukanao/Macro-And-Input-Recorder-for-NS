using System.IO.Ports;
using MacroRecorder.Configuration;
using MacroRecorder.Models;
using MacroRecorder.Protocols;

namespace MacroRecorder.Services;

public sealed class UsbSnifferRecorder
{
    private readonly RecorderOptions _options;

    public UsbSnifferRecorder(RecorderOptions options)
    {
        _options = options;
    }

    public async Task<CaptureSession> RecordAsync()
    {
        var frames = new List<ControllerFrame>();
        using var port = new SerialPort(_options.PortName, _options.BaudRate)
        {
            ReadTimeout = _options.PollIntervalMs,
            NewLine = "\n"
        };

        port.Open();
        port.Write("s\n");
        var start = DateTime.UtcNow;

        while ((DateTime.UtcNow - start).TotalSeconds < _options.Seconds)
        {
            port.Write("b\n");
            await Task.Delay(_options.PollIntervalMs);

            while (port.BytesToRead > 0)
            {
                try
                {
                    var line = port.ReadLine().Trim();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var packet = ProControllerPacket.FromHexLine(line);
                    var timestamp = DateTime.UtcNow - start;
                    frames.Add(packet.ToFrame(timestamp));
                }
                catch (TimeoutException)
                {
                    break;
                }
                catch (FormatException)
                {
                    // ignore malformed line
                }
                catch (InvalidOperationException)
                {
                    // ignore malformed packet
                }
            }
        }

        return new CaptureSession(frames);
    }
}
