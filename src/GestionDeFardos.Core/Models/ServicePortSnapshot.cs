namespace GestionDeFardos.Core.Models;

public sealed class ServicePortSnapshot
{
    public string RawFrame { get; set; } = string.Empty;
    public int? RawGrams { get; set; }
    public decimal? WeightKg { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.MinValue;
    public bool IsConnected { get; set; }
    public string? LastError { get; set; }
    public string ConfiguredButtonLine { get; set; } = string.Empty;
    public bool? CtsState { get; set; }
    public bool? DsrState { get; set; }
    public ServiceButtonState ButtonState { get; set; } = ServiceButtonState.Unknown;
    public DateTime? LastButtonPressedAt { get; set; }
    public string? ButtonLastError { get; set; }
}

public enum ServiceButtonState
{
    Unknown,
    Released,
    Pressed
}
