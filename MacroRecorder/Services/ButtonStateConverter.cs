using MacroRecorder.Models;

namespace MacroRecorder.Services;

public static class ButtonStateConverter
{
    public static string ToCsv(ButtonState buttons)
    {
        if (buttons == ButtonState.None)
        {
            return nameof(ButtonState.None);
        }

        var values = Enum.GetValues<ButtonState>()
            .Where(v => v != ButtonState.None && buttons.HasFlag(v))
            .Select(v => v.ToString());

        return string.Join(',', values);
    }

    public static ButtonState Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Equals(nameof(ButtonState.None), StringComparison.OrdinalIgnoreCase))
        {
            return ButtonState.None;
        }

        var parts = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        ButtonState buttons = ButtonState.None;
        foreach (var part in parts)
        {
            if (Enum.TryParse<ButtonState>(part, true, out var value))
            {
                buttons |= value;
            }
        }

        return buttons;
    }
}
