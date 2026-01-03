using System.ComponentModel;
using System.Runtime.CompilerServices;
using MacroRecorder.Models;
using MacroRecorder.Services;

namespace MacroRecorder.Gui.ViewModels;

public sealed class MacroStepViewModel : INotifyPropertyChanged
{
    private int _frames = 1;
    private string _buttonsText = nameof(ButtonState.None);
    private short _leftX;
    private short _leftY;
    private short _rightX;
    private short _rightY;
    private short _roll;
    private short _pitch;
    private short _yaw;

    public int Frames
    {
        get => _frames;
        set => SetField(ref _frames, value);
    }

    public string ButtonsText
    {
        get => _buttonsText;
        set => SetField(ref _buttonsText, value);
    }

    public short LeftX
    {
        get => _leftX;
        set => SetField(ref _leftX, value);
    }

    public short LeftY
    {
        get => _leftY;
        set => SetField(ref _leftY, value);
    }

    public short RightX
    {
        get => _rightX;
        set => SetField(ref _rightX, value);
    }

    public short RightY
    {
        get => _rightY;
        set => SetField(ref _rightY, value);
    }

    public short Roll
    {
        get => _roll;
        set => SetField(ref _roll, value);
    }

    public short Pitch
    {
        get => _pitch;
        set => SetField(ref _pitch, value);
    }

    public short Yaw
    {
        get => _yaw;
        set => SetField(ref _yaw, value);
    }

    public MacroStep ToMacroStep()
    {
        return new MacroStep
        {
            Frames = Math.Max(1, Frames),
            Buttons = ButtonStateConverter.Parse(ButtonsText),
            LeftStick = new AnalogStickState(LeftX, LeftY),
            RightStick = new AnalogStickState(RightX, RightY),
            Gyro = new GyroState(Roll, Pitch, Yaw)
        };
    }

    public static MacroStepViewModel FromStep(MacroStep step)
    {
        return new MacroStepViewModel
        {
            Frames = step.Frames,
            ButtonsText = ButtonStateConverter.ToCsv(step.Buttons),
            LeftX = step.LeftStick.X,
            LeftY = step.LeftStick.Y,
            RightX = step.RightStick.X,
            RightY = step.RightStick.Y,
            Roll = step.Gyro.Roll,
            Pitch = step.Gyro.Pitch,
            Yaw = step.Gyro.Yaw
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (!Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
