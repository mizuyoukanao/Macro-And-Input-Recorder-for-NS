namespace MacroRecorder.Configuration;

public sealed class MacroOptions
{
    public string PortName { get; init; } = "COM5";
    public int BaudRate { get; init; } = 2000000;
    public string ConfigPath { get; init; } = "macro.json";

    public static MacroOptions FromArgs(string[] args)
    {
        var options = new MacroOptions();
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--port":
                    options = options with { PortName = args[++i] };
                    break;
                case "--baud":
                    options = options with { BaudRate = int.Parse(args[++i]) };
                    break;
                case "--config":
                    options = options with { ConfigPath = args[++i] };
                    break;
            }
        }

        return options;
    }
}
