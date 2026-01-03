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
            WriteTimeout = 1000,
            RtsEnable = true,
            DtrEnable = true,
            ReadTimeout = _options.PollIntervalMs,
            NewLine = "\n",
        };

        var start = DateTime.UtcNow;
        var sync = new object();

        SerialDataReceivedEventHandler handler = (_, _) =>
        {
            try
            {
                while (port.BytesToRead > 0)
                {
                    var line = port.ReadLine().Trim();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var packet = ProControllerPacket.FromHexLine(line);
                    var timestamp = DateTime.UtcNow - start;

                    lock (sync)
                    {
                        frames.Add(packet.ToFrame(timestamp));
                    }
                }
            }
            catch (TimeoutException)
            {
                // ignore
            }
            catch (FormatException)
            {
                // ignore malformed line
            }
            catch (InvalidOperationException)
            {
                // ignore malformed packet
            }
        };

        port.Open();
        port.DataReceived += handler;
        port.Write("s\n");

        try
        {
            while ((DateTime.UtcNow - start).TotalSeconds < _options.Seconds)
            {
                port.Write("b\n");
                await Task.Delay(_options.PollIntervalMs);
            }
        }
        finally
        {
            port.DataReceived -= handler;
        }

        return new CaptureSession(frames);
    }
}
