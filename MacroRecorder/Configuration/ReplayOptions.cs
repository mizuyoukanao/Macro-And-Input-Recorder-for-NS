namespace MacroRecorder.Configuration;

public sealed record ReplayOptions
{
    public string PortName { get; init; } = "COM4";
    public int BaudRate { get; init; } = 2000000;
    public string InputPath { get; init; } = "capture.bin";
    public bool Loop { get; init; }
    public MotionEncoding MotionEncoding { get; init; } = MotionEncoding.RawGyro;

    public static ReplayOptions FromArgs(string[] args)
    {
        var options = new ReplayOptions();
        for (var i = 0; i < args.Length; i++)
        {
            options = args[i] switch
            {
                "--port" => options with { PortName = args[++i] },
                "--baud" => options with { BaudRate = int.Parse(args[++i]) },
                "--input" => options with { InputPath = args[++i] },
                "--loop" => options with { Loop = true },
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
