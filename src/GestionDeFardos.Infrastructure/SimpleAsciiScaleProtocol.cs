using System.Globalization;
using System.Text;
using GestionDeFardos.Core.Config;

namespace GestionDeFardos.Infrastructure;

internal sealed class SimpleAsciiScaleProtocol : IScaleProtocol
{
    public string Id => "simple-ascii";

    public ScaleFrameReadResult TryReadFrame(List<byte> buffer, ScaleSettings settings)
    {
        byte[] newLineBytes = Encoding.ASCII.GetBytes(SerialSettingsHelper.NormalizeNewLine(settings.NewLine));
        int lineBreakIndex = ByteSequenceHelper.FindSequence(buffer, newLineBytes);
        if (lineBreakIndex < 0)
        {
            TrimAsciiBuffer(buffer);
            return ScaleFrameReadResult.NeedMoreData();
        }

        byte[] frameBytes = buffer.GetRange(0, lineBreakIndex).ToArray();
        buffer.RemoveRange(0, lineBreakIndex + newLineBytes.Length);

        string line = Encoding.ASCII.GetString(frameBytes).TrimEnd('\r');
        if (string.IsNullOrWhiteSpace(line))
        {
            return ScaleFrameReadResult.InvalidFrame("Se recibio una trama vacia.");
        }

        int? grams = TryExtractFirstInteger(line);
        if (!grams.HasValue)
        {
            return ScaleFrameReadResult.InvalidFrame($"No se detecto un entero en la trama ASCII \"{SanitizeForLog(line)}\".");
        }

        return ScaleFrameReadResult.FrameDecoded(
            new ScaleDecodedFrame
            {
                DisplayFrame = line,
                WeightGrams = grams.Value,
                TareGrams = null
            });
    }

    private static int? TryExtractFirstInteger(string frame)
    {
        foreach (string token in frame.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                return value;
            }
        }

        return null;
    }

    private static void TrimAsciiBuffer(List<byte> buffer)
    {
        const int maxBufferedBytes = 4096;
        if (buffer.Count > maxBufferedBytes)
        {
            buffer.RemoveRange(0, buffer.Count - maxBufferedBytes);
        }
    }

    private static string SanitizeForLog(string text)
    {
        return text
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);
    }
}
