namespace MacroRecorder.Configuration;

public sealed record MacroOptions
{
    public string PortName { get; init; } = "COM5";
    public int BaudRate { get; init; } = 2000000;
    public string ConfigPath { get; init; } = "macro.json";
    public MotionEncoding MotionEncoding { get; init; } = MotionEncoding.RawGyro;

    public static MacroOptions FromArgs(string[] args)
    {
        var options = new MacroOptions();
        for (var i = 0; i < args.Length; i++)
        {
            options = args[i] switch
            {
                "--port" => options with { PortName = args[++i] },
                "--baud" => options with { BaudRate = int.Parse(args[++i]) },
                "--config" => options with { ConfigPath = args[++i] },
                "--motion" => options with { MotionEncoding = ParseMotionEncoding(args[++i]) },
                "--quat" or "--quaternion" => options with { MotionEncoding = MotionEncoding.Quaternion },
                _ => options
            };
        }
        return options;
    }

    private static MotionEncoding ParseMotionEncoding(string value) =>
        value.Equals("quaternion", StringComparison.OrdinalIgnoreCase) || value.Equals("quat", StringComparison.OrdinalIgnoreCase)
            ? MotionEncoding.Quaternion
            : MotionEncoding.RawGyro;
}
