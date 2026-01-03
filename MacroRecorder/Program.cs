using MacroRecorder.Configuration;
using MacroRecorder.Models;
using MacroRecorder.Serialization;
using MacroRecorder.Services;

var argsSpan = args.AsSpan();

if (argsSpan.Length == 0)
{
    ShowHelp();
    return;
}

switch (argsSpan[0].ToLowerInvariant())
{
    case "record":
        await RunRecorderAsync(argsSpan[1..]);
        break;
    case "replay":
        await RunReplayAsync(argsSpan[1..]);
        break;
    case "macro":
        await RunMacroAsync(argsSpan[1..]);
        break;
    case "help":
    default:
        ShowHelp();
        break;
}

static void ShowHelp()
{
    Console.WriteLine("Macro Recorder for Nintendo Switch Pro Controller");
    Console.WriteLine("Commands:");
    Console.WriteLine("  record --port <port> --baud <baud> --seconds <seconds> [--poll <ms>] --output <file>");
    Console.WriteLine("  replay --port <port> --baud <baud> --input <file> [--loop]");
    Console.WriteLine("  macro --port <port> --baud <baud> --config <macro.json>");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  dotnet run -- record --port COM5 --baud 12000000 --seconds 10 --output capture.bin");
    Console.WriteLine("  dotnet run -- replay --port /dev/ttyUSB1 --baud 2000000 --input capture.bin");
    Console.WriteLine("  dotnet run -- macro --port COM6 --baud 2000000 --config sample_macro.json");
}

static async Task RunRecorderAsync(ReadOnlySpan<string> span)
{
    var options = RecorderOptions.FromArgs(span.ToArray());
    var recorder = new UsbSnifferRecorder(options);

    Console.WriteLine($"Recording {options.Seconds} seconds from {options.PortName}...");
    var session = await recorder.RecordAsync();
    BinaryCaptureWriter.Write(options.OutputPath, session);
    Console.WriteLine($"Saved {session.Frames.Count} frames to {options.OutputPath}");
}

static async Task RunReplayAsync(ReadOnlySpan<string> span)
{
    var options = ReplayOptions.FromArgs(span.ToArray());
    var session = BinaryCaptureWriter.Read(options.InputPath);
    var sender = new SwitchSerialSender(options.PortName, options.BaudRate);
    var playback = new PacketPlayback(sender, session);
    Console.WriteLine($"Replaying {session.Frames.Count} frames to {options.PortName} (loop: {options.Loop})");
    await playback.PlayAsync(options.Loop);
    Console.WriteLine("Replay finished.");
}

static async Task RunMacroAsync(ReadOnlySpan<string> span)
{
    var options = MacroOptions.FromArgs(span.ToArray());
    var macro = MacroSerializer.Read(options.ConfigPath);
    var sender = new SwitchSerialSender(options.PortName, options.BaudRate);
    var generator = new MacroGenerator();
    var frames = generator.BuildFrames(macro);
    var playback = new PacketPlayback(sender, new CaptureSession(frames));
    Console.WriteLine($"Sending macro '{macro.Name}' with {frames.Count} frames...");
    await playback.PlayAsync(false);
    Console.WriteLine("Macro sent.");
}
