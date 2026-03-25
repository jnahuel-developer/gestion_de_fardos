namespace GestionDeFardos.Core.Models;

public sealed class ServicePortSnapshot
{
    public string ScaleProtocol { get; set; } = string.Empty;
    public string ScalePortProfile { get; set; } = string.Empty;
    public string ScaleRawChunk { get; set; } = string.Empty;
    public string ScaleLastDecodedFrame { get; set; } = string.Empty;
    public int? RawGrams { get; set; }
    public int? RawTareGrams { get; set; }
    public decimal? WeightKg { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.MinValue;
    public bool IsConnected { get; set; }
    public string? LastError { get; set; }

    public string ButtonPortProfile { get; set; } = string.Empty;
    public bool ButtonIsConfigured { get; set; }
    public bool ButtonIsConnected { get; set; }
    public ServiceButtonState ButtonStatus { get; set; } = ServiceButtonState.Disabled;
    public string LastButtonRawChunk { get; set; } = string.Empty;
    public string LastButtonFrame { get; set; } = string.Empty;
    public string LastButtonResponse { get; set; } = string.Empty;
    public DateTime? LastButtonPressedAt { get; set; }
    public string? ButtonLastError { get; set; }
}

public enum ServiceButtonState
{
    Disabled,
    Disconnected,
    Listening,
    Error
}
