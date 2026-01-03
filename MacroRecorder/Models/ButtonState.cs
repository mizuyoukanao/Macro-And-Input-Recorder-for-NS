namespace MacroRecorder.Models;

[Flags]
public enum ButtonState : int
{
    None = 0,
    Y = 1 << 0,
    X = 1 << 1,
    B = 1 << 2,
    A = 1 << 3,
    R = 1 << 4,
    ZR = 1 << 5,
    L = 1 << 6,
    ZL = 1 << 7,
    Minus = 1 << 8,
    Plus = 1 << 9,
    RStick = 1 << 10,
    LStick = 1 << 11,
    Home = 1 << 12,
    Capture = 1 << 13,
    Up = 1 << 14,
    Down = 1 << 15,
    Left = 1 << 16,
    Right = 1 << 17
}
