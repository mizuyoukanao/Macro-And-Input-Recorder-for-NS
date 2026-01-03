namespace MacroRecorder.Configuration;

public sealed class ReplayOptions
{
    public string PortName { get; init; } = "COM4";
    public int BaudRate { get; init; } = 2000000;
    public string InputPath { get; init; } = "capture.bin";
    public bool Loop { get; init; }

    public static ReplayOptions FromArgs(string[] args)
    {
        var options = new ReplayOptions();
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--port":
                    options = new ReplayOptions { PortName = args[++i] };
                    break;
                case "--baud":
                    options = new ReplayOptions { BaudRate = int.Parse(args[++i]) };
                    break;
                case "--input":
                    options = new ReplayOptions { InputPath = args[++i] };
                    break;
                case "--loop":
                    options = new ReplayOptions { Loop = true };
                    break;
            }
        }

        return options;
    }
}
