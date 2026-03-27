namespace GestionDeFardos.Infrastructure;

internal enum ScaleFrameReadStatus
{
    NeedMoreData,
    InvalidFrame,
    FrameDecoded
}

internal sealed class ScaleDecodedFrame
{
    public required string DisplayFrame { get; init; }
    public int? WeightGrams { get; init; }
    public int? TareGrams { get; init; }
}

internal sealed class ScaleFrameReadResult
{
    public ScaleFrameReadStatus Status { get; init; }
    public string? ErrorMessage { get; init; }
    public ScaleDecodedFrame? Frame { get; init; }

    public static ScaleFrameReadResult NeedMoreData()
    {
        return new ScaleFrameReadResult { Status = ScaleFrameReadStatus.NeedMoreData };
    }

    public static ScaleFrameReadResult InvalidFrame(string message)
    {
        return new ScaleFrameReadResult
        {
            Status = ScaleFrameReadStatus.InvalidFrame,
            ErrorMessage = message
        };
    }

    public static ScaleFrameReadResult FrameDecoded(ScaleDecodedFrame frame)
    {
        return new ScaleFrameReadResult
        {
            Status = ScaleFrameReadStatus.FrameDecoded,
            Frame = frame
        };
    }
}
