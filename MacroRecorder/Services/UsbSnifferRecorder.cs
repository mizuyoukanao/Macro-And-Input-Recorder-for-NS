using System.Diagnostics;
using System.IO.Ports;
using System.Text;
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
            Handshake = Handshake.RequestToSend,
            WriteTimeout = 1000,
            RtsEnable = true,
            DtrEnable = true,
            ReadTimeout = _options.PollIntervalMs,
            NewLine = "\n",
        };

        var start = DateTime.UtcNow;
        var sync = new object();
        char header = '3';
        SerialDataReceivedEventHandler handler = (_, _) =>
        {
            try
            {
                while (port.BytesToRead > 0)
                {
                    //var line = port.ReadLine();//.Trim();
                    byte[] readbuffer = new byte[port.BytesToRead];
                    port.Read(readbuffer, 0, port.BytesToRead);
                    var line =  Encoding.ASCII.GetString(readbuffer).Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                    foreach (string oneline in line)
                    {
                        Debug.WriteLine(oneline);
                        Debug.Write("\n");
                        if (string.IsNullOrWhiteSpace(oneline) || oneline[0] != header)
                        {
                            continue;
                        }
                        var packet = ProControllerPacket.FromHexLine(oneline);
                        var timestamp = DateTime.UtcNow - start;

                        lock (sync)
                        {
                            frames.Add(packet.ToFrame(timestamp, _options.MotionEncoding));
                        }
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
        port.Write("z\n");
        port.Write("z\n");
        port.Write("z\n");
        port.Write("x\n");
        port.Write("x\n");
        port.Write("x\n");
        port.Write("s\n");
        //await Task.Delay(5000);
        try
        {
            while ((DateTime.UtcNow - start).TotalSeconds < _options.Seconds)
            {
                port.Write("p\n");
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