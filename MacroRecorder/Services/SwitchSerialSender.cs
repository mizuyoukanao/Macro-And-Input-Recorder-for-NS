using System.IO.Ports;

namespace MacroRecorder.Services;

public sealed class SwitchSerialSender : IDisposable
{
    private readonly SerialPort _port;

    public SwitchSerialSender(string portName, int baudRate)
    {
        _port = new SerialPort(portName, baudRate)
        {
            WriteTimeout = 1000
        };
        _port.Open();
    }

    public void SendPacket(byte[] payload)
    {
        // UARTControllerNX framing: bytes 0-4 are 0xAA, followed by length, payload, checksum
        var frame = new byte[payload.Length + 7];
        for (var i = 0; i < 5; i++)
        {
            frame[i] = 0xAA;
        }

        frame[5] = (byte)payload.Length;
        Array.Copy(payload, 0, frame, 6, payload.Length);
        frame[^1] = CalculateChecksum(frame.AsSpan(5, payload.Length + 1));
        _port.Write(frame, 0, frame.Length);
    }

    public void Dispose()
    {
        _port.Dispose();
    }

    private static byte CalculateChecksum(ReadOnlySpan<byte> data)
    {
        byte sum = 0;
        foreach (var b in data)
        {
            sum ^= b;
        }
        return sum;
    }
}
