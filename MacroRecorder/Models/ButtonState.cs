namespace MacroRecorder.Models;

[Flags]
public enum ButtonState : int
{
    None = 0,
    Y = 1 << 0,
    X = 1 << 1,
    B = 1 << 2,
    A = 1 << 3,
    //SR = 1 << 4,
    //SL = 1 << 5,
    R = 1 << 6,
    ZR = 1 << 7,
    Minus = 1 << 8,
    Plus = 1 << 9,
    RStick = 1 << 10,
    LStick = 1 << 11,
    Home = 1 << 12,
    Capture = 1 << 13,
    //UNUSEDBIT = 1 << 14,
    //CHRGRIP = 1 << 15,
    Down = 1 << 16,
    Up = 1 << 17,
    Right = 1 << 18,
    Left = 1 << 19,
    //SR2 = 1 << 20,
    //SL2 = 1 << 21,
    L = 1 << 22,
    ZL = 1 << 23,
}
