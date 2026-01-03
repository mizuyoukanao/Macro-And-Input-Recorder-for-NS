namespace MacroRecorder.Configuration;

public sealed class RecorderOptions
{
    public string PortName { get; init; } = "COM3";
    public int BaudRate { get; init; } = 12000000;
    public int Seconds { get; init; } = 10;
    public int PollIntervalMs { get; init; } = 50;
    public string OutputPath { get; init; } = "capture.bin";

    public static RecorderOptions FromArgs(string[] args)
    {
        var options = new RecorderOptions();
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--port":
                    options = new RecorderOptions { PortName = args[++i] };
                    break;
                case "--baud":
                    options = new RecorderOptions { BaudRate = int.Parse(args[++i]) };
                    break;
                case "--seconds":
                    options = new RecorderOptions { Seconds = int.Parse(args[++i]) };
                    break;
                case "--poll":
                    options = new RecorderOptions { PollIntervalMs = int.Parse(args[++i]) };
                    break;
                case "--output":
                    options = new RecorderOptions { OutputPath = args[++i] };
                    break;
            }
        }

        return options;
    }
}
