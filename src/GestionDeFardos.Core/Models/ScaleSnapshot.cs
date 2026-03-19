namespace GestionDeFardos.Core.Models;

public sealed class ScaleSnapshot
{
    public string RawFrame { get; set; } = string.Empty;
    public int? RawGrams { get; set; }
    public decimal? WeightKg { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.MinValue;
    public bool IsConnected { get; set; }
    public string? LastError { get; set; }
}
