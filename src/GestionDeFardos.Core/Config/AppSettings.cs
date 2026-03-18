namespace GestionDeFardos.Core.Config;

public sealed class AppSettings
{
    public ScaleSettings Scale { get; set; } = new();
    public ButtonSettings Button { get; set; } = new();
    public ThresholdSettings Thresholds { get; set; } = new();
    public PasswordSettings Passwords { get; set; } = new();
    public ExportSettings Export { get; set; } = new();
}

public sealed class ScaleSettings
{
    public string PortName { get; set; } = "COM1";
    public int BaudRate { get; set; } = 9600;
    public int DataBits { get; set; } = 8;
    public string Parity { get; set; } = "None";
    public string StopBits { get; set; } = "One";
    public string NewLine { get; set; } = "\n";
}

public sealed class ButtonSettings
{
    public string InputLine { get; set; } = nameof(ButtonInputLine.Cts);
}

public enum ButtonInputLine
{
    Cts,
    Dsr
}

public sealed class ThresholdSettings
{
    public decimal MinKg { get; set; }
    public decimal MaxKg { get; set; }
}

public sealed class PasswordSettings
{
    public string Edit { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
}

public sealed class ExportSettings
{
    public string Folder { get; set; } = "exports";
}
