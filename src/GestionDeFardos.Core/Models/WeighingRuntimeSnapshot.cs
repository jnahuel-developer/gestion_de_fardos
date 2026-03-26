namespace GestionDeFardos.Core.Models;

public sealed class WeighingRuntimeSnapshot
{
    public bool ScaleIsConnected { get; set; }
    public int? CurrentWeightGrams { get; set; }
    public decimal? CurrentWeightKg { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.MinValue;
    public WeighingCaptureStatus LastCaptureStatus { get; set; } = WeighingCaptureStatus.Idle;
    public string LastCaptureMessage { get; set; } = "Esperando opresion del pulsador.";
    public DateTime? LastCaptureAt { get; set; }
    public WeighingRecordSummary? LastSavedRecord { get; set; }
}

public sealed class WeighingRecordSummary
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal WeightKg { get; set; }
}

public enum WeighingCaptureStatus
{
    Idle,
    Saved,
    RejectedNoWeight,
    RejectedOutOfRange,
    SaveError
}
