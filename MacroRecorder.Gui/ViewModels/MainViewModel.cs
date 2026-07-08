using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using Microsoft.Win32;
using MacroRecorder.Configuration;
using MacroRecorder.Models;
using MacroRecorder.Serialization;
using MacroRecorder.Services;

namespace MacroRecorder.Gui.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private string _capturePortName = "COM16";
    private int _captureBaudRate = 115200;
    private int _captureSeconds = 5;
    private int _capturePollIntervalMs = 50;
    private string _captureStatus = "待機中";
    private string _macroName = "macro";
    private int _macroFrameInterval = 8;
    private string _macroStatus = "";
    private string _sendPortName = "COM3";
    private int _sendBaudRate = 9600;
    private string _sendStatus = "Switch未接続";
    private MotionEncoding _captureMotionEncoding = MotionEncoding.RawGyro;
    private MotionEncoding _macroMotionEncoding = MotionEncoding.RawGyro;
    private MacroStepViewModel? _selectedMacroStep;
    private CaptureSession? _currentCapture;

    public MainViewModel()
    {
        CaptureCommand = new AsyncRelayCommand(CaptureAsync);
        LoadCaptureCommand = new RelayCommand(LoadCapture);
        BuildMacroFromCaptureCommand = new RelayCommand(BuildMacroFromCapture, () => _currentCapture is not null);
        AddStepCommand = new RelayCommand(AddStep);
        NormalizeMacroFramesCommand = new RelayCommand(NormalizeMacroFrames, () => MacroSteps.Count > 0);
        LoadMacroCommand = new RelayCommand(LoadMacro);
        SaveMacroCommand = new RelayCommand(SaveMacro);
        SendMacroCommand = new AsyncRelayCommand(SendMacroAsync, () => MacroSteps.Count > 0);
        SendCaptureCommand = new AsyncRelayCommand(SendCaptureAsync, () => _currentCapture is not null);

        MacroSteps.CollectionChanged += (_, _) =>
        {
            SendMacroCommand.RaiseCanExecuteChanged();
            NormalizeMacroFramesCommand.RaiseCanExecuteChanged();
            if (SelectedMacroStep is null || !MacroSteps.Contains(SelectedMacroStep))
            {
                SelectedMacroStep = MacroSteps.FirstOrDefault();
            }
        };
    }

    public ObservableCollection<FrameViewModel> CapturedFrames { get; } = new();
    public ObservableCollection<MacroStepViewModel> MacroSteps { get; } = new();
    public IReadOnlyList<MotionEncoding> MotionEncodingOptions { get; } = Enum.GetValues<MotionEncoding>();

    public string CapturePortName
    {
        get => _capturePortName;
        set => SetField(ref _capturePortName, value);
    }

    public int CaptureBaudRate
    {
        get => _captureBaudRate;
        set => SetField(ref _captureBaudRate, value);
    }

    public int CaptureSeconds
    {
        get => _captureSeconds;
        set => SetField(ref _captureSeconds, value);
    }

    public int CapturePollIntervalMs
    {
        get => _capturePollIntervalMs;
        set => SetField(ref _capturePollIntervalMs, value);
    }

    public string CaptureStatus
    {
        get => _captureStatus;
        set => SetField(ref _captureStatus, value);
    }

    public string MacroName
    {
        get => _macroName;
        set => SetField(ref _macroName, value);
    }

    public int MacroFrameInterval
    {
        get => _macroFrameInterval;
        set => SetField(ref _macroFrameInterval, value);
    }

    public string MacroStatus
    {
        get => _macroStatus;
        set => SetField(ref _macroStatus, value);
    }

    public string SendPortName
    {
        get => _sendPortName;
        set => SetField(ref _sendPortName, value);
    }

    public int SendBaudRate
    {
        get => _sendBaudRate;
        set => SetField(ref _sendBaudRate, value);
    }

    public string SendStatus
    {
        get => _sendStatus;
        set => SetField(ref _sendStatus, value);
    }

    public MotionEncoding CaptureMotionEncoding
    {
        get => _captureMotionEncoding;
        set => SetField(ref _captureMotionEncoding, value);
    }

    public MotionEncoding MacroMotionEncoding
    {
        get => _macroMotionEncoding;
        set => SetField(ref _macroMotionEncoding, value);
    }

    public MacroStepViewModel? SelectedMacroStep
    {
        get => _selectedMacroStep;
        set => SetField(ref _selectedMacroStep, value);
    }

    public AsyncRelayCommand CaptureCommand { get; }
    public RelayCommand LoadCaptureCommand { get; }
    public RelayCommand BuildMacroFromCaptureCommand { get; }
    public RelayCommand AddStepCommand { get; }
    public RelayCommand NormalizeMacroFramesCommand { get; }
    public RelayCommand LoadMacroCommand { get; }
    public RelayCommand SaveMacroCommand { get; }
    public AsyncRelayCommand SendMacroCommand { get; }
    public AsyncRelayCommand SendCaptureCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task CaptureAsync()
    {
        CaptureStatus = "記録中...";
        CapturedFrames.Clear();

        try
        {
            var options = new RecorderOptions
            {
                PortName = CapturePortName,
                BaudRate = CaptureBaudRate,
                Seconds = CaptureSeconds,
                PollIntervalMs = CapturePollIntervalMs,
                OutputPath = "capture.bin",
                MotionEncoding = CaptureMotionEncoding
            };

            var recorder = new UsbSnifferRecorder(options);
            _currentCapture = await recorder.RecordAsync();
            CaptureStatus = $"{_currentCapture.Frames.Count}フレーム取得";
            UpdateCapturedFrames();
            ReflectCaptureInMacroEditor();
            BuildMacroFromCaptureCommand.RaiseCanExecuteChanged();
            SendCaptureCommand.RaiseCanExecuteChanged();
        }
        catch (IOException ex)
        {
            CaptureStatus = ex.Message;
        }
        catch (UnauthorizedAccessException ex)
        {
            CaptureStatus = ex.Message;
        }
        catch (Exception ex)
        {
            CaptureStatus = $"エラー: {ex.Message}";
        }
    }

    private void LoadCapture()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Capture (*.bin)|*.bin|All files|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            _currentCapture = BinaryCaptureWriter.Read(dialog.FileName);
            CaptureStatus = $"{dialog.FileName} を読み込みました ({_currentCapture.Frames.Count}フレーム)";
            UpdateCapturedFrames();
            ReflectCaptureInMacroEditor();
            BuildMacroFromCaptureCommand.RaiseCanExecuteChanged();
            SendCaptureCommand.RaiseCanExecuteChanged();
        }
    }

    private void UpdateCapturedFrames()
    {
        CapturedFrames.Clear();
        if (_currentCapture is null)
        {
            return;
        }

        foreach (var frame in _currentCapture.Frames)
        {
            CapturedFrames.Add(new FrameViewModel(frame));
        }
    }

    private void BuildMacroFromCapture()
    {
        ReflectCaptureInMacroEditor("キャプチャからマクロを生成しました");
    }

    private void ReflectCaptureInMacroEditor(string? statusMessage = null)
    {
        if (_currentCapture is null)
        {
            MacroStatus = "キャプチャがありません";
            return;
        }

        var builder = new CaptureMacroBuilder();
        var macro = builder.BuildPreservingFrames("capture", _currentCapture);
        MacroName = macro.Name;
        MacroFrameInterval = macro.FrameIntervalMs;
        MacroSteps.Clear();
        foreach (var step in macro.Steps)
        {
            MacroSteps.Add(MacroStepViewModel.FromStep(step));
        }

        SelectedMacroStep = MacroSteps.FirstOrDefault();
        MacroStatus = statusMessage ?? $"キャプチャ内容をマクロ編集に反映しました ({MacroSteps.Count}ステップ)";
        SendMacroCommand.RaiseCanExecuteChanged();
    }

    private void AddStep()
    {
        var step = new MacroStepViewModel();
        MacroSteps.Add(step);
        SelectedMacroStep = step;
        SendMacroCommand.RaiseCanExecuteChanged();
    }


    private void NormalizeMacroFrames()
    {
        if (MacroSteps.Count == 0)
        {
            MacroStatus = "分解するステップがありません";
            return;
        }

        var expanded = MacroSteps
            .SelectMany(step => Enumerable.Range(0, Math.Max(1, step.Frames)).Select(_ => CloneAsSingleFrame(step)))
            .ToList();

        MacroSteps.Clear();
        foreach (var step in expanded)
        {
            MacroSteps.Add(step);
        }

        SelectedMacroStep = MacroSteps.FirstOrDefault();
        MacroStatus = $"{MacroSteps.Count}件の1Fステップに分解しました";
    }


    private static MacroStepViewModel CloneAsSingleFrame(MacroStepViewModel source)
    {
        var clone = MacroStepViewModel.FromStep(source.ToMacroStep());
        clone.Frames = 1;
        return clone;
    }

    private void LoadMacro()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Macro (*.json)|*.json|All files|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            var macro = MacroSerializer.Read(dialog.FileName);
            MacroName = macro.Name;
            MacroFrameInterval = macro.FrameIntervalMs;
            MacroSteps.Clear();
            foreach (var step in macro.Steps)
            {
                MacroSteps.Add(MacroStepViewModel.FromStep(step));
            }
            MacroStatus = $"{dialog.FileName} を読み込みました";
            SendMacroCommand.RaiseCanExecuteChanged();
        }
    }

    private void SaveMacro()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Macro (*.json)|*.json|All files|*.*",
            FileName = "macro.json"
        };

        if (dialog.ShowDialog() == true)
        {
            var macro = BuildMacroDefinition();
            MacroSerializer.Write(dialog.FileName, macro);
            MacroStatus = $"{dialog.FileName} に保存しました";
        }
    }

    private MacroDefinition BuildMacroDefinition()
    {
        var macro = new MacroDefinition
        {
            Name = MacroName,
            FrameIntervalMs = MacroFrameInterval,
            Steps = MacroSteps.Select(s => s.ToMacroStep()).ToList()
        };
        return macro;
    }

    private async Task SendMacroAsync()
    {
        try
        {
            var macro = BuildMacroDefinition();
            var frames = new MacroGenerator().BuildFrames(macro);
            await SendFramesAsync(new CaptureSession(frames));
            SendStatus = $"マクロ '{macro.Name}' を送信しました ({MacroMotionEncoding})";
        }
        catch (Exception ex)
        {
            SendStatus = $"送信エラー: {ex.Message}";
        }
    }

    private async Task SendCaptureAsync()
    {
        if (_currentCapture is null)
        {
            SendStatus = "キャプチャがありません";
            return;
        }

        try
        {
            await SendFramesAsync(_currentCapture);
            SendStatus = $"キャプチャを送信しました ({MacroMotionEncoding})";
        }
        catch (Exception ex)
        {
            SendStatus = $"送信エラー: {ex.Message}";
        }
    }

    private async Task SendFramesAsync(CaptureSession session)
    {
        using var sender = new SwitchSerialSender(SendPortName, SendBaudRate);
        var playback = new PacketPlayback(sender, session, MacroMotionEncoding);
        await playback.PlayAsync(false);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (!Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
