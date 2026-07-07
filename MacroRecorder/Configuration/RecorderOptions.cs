namespace MacroRecorder.Configuration;

public sealed record RecorderOptions
{
    public string PortName { get; init; } = "COM3";
    public int BaudRate { get; init; } = 12000000;
    public int Seconds { get; init; } = 10;
    public int PollIntervalMs { get; init; } = 50;
    public string OutputPath { get; init; } = "capture.bin";
    public MotionEncoding MotionEncoding { get; init; } = MotionEncoding.RawGyro;

    public static RecorderOptions FromArgs(string[] args)
    {
        var options = new RecorderOptions();
        for (var i = 0; i < args.Length; i++)
        {
            options = args[i] switch
            {
                "--port" => options with { PortName = args[++i] },
                "--baud" => options with { BaudRate = int.Parse(args[++i]) },
                "--seconds" => options with { Seconds = int.Parse(args[++i]) },
                "--poll" => options with { PollIntervalMs = int.Parse(args[++i]) },
                "--output" => options with { OutputPath = args[++i] },
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
