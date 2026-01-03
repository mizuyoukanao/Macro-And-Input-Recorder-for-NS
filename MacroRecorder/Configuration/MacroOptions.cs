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
                    options = new MacroOptions { PortName = args[++i] };
                    break;
                case "--baud":
                    options = new MacroOptions { BaudRate = int.Parse(args[++i]) };
                    break;
                case "--config":
                    options = new MacroOptions { ConfigPath = args[++i] };
                    break;
            }
        }

        return options;
    }
}
