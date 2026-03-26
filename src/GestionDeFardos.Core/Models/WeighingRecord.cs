namespace GestionDeFardos.Core.Models;

public sealed class WeighingRecord
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal WeightKg { get; set; }
    public int? RawGrams { get; set; }
    public string? RawFrame { get; set; }
    public bool IsEditedToZero { get; set; }
    public DateTime? EditedAt { get; set; }
}
