using GestionDeFardos.Core.Config;

namespace GestionDeFardos.Infrastructure;

internal sealed class W180TScaleProtocol : IScaleProtocol
{
    private const byte Stx = 0x02;
    private const byte Cr = 0x0D;
    private const byte Lf = 0x0A;
    private const int FrameLength = 18;

    public string Id => "w180-t";

    public ScaleFrameReadResult TryReadFrame(List<byte> buffer, ScaleSettings settings)
    {
        int stxIndex = buffer.IndexOf(Stx);
        if (stxIndex < 0)
        {
            if (buffer.Count > 0)
            {
                buffer.Clear();
                return ScaleFrameReadResult.InvalidFrame("Se descartaron bytes porque no se encontro STX.");
            }

            return ScaleFrameReadResult.NeedMoreData();
        }

        if (stxIndex > 0)
        {
            buffer.RemoveRange(0, stxIndex);
            return ScaleFrameReadResult.InvalidFrame("Se descartaron bytes previos al STX.");
        }

        if (buffer.Count < FrameLength)
        {
            return ScaleFrameReadResult.NeedMoreData();
        }

        byte[] frameBytes = buffer.GetRange(0, FrameLength).ToArray();

        if (frameBytes[16] != Cr || frameBytes[17] != Lf)
        {
            buffer.RemoveAt(0);
            return ScaleFrameReadResult.InvalidFrame("La trama W180-T no termina en CRLF.");
        }

        if (!TryParseSixDigitNumber(frameBytes, 4, out int weight))
        {
            buffer.RemoveAt(0);
            return ScaleFrameReadResult.InvalidFrame("No se pudo interpretar el campo PESO de la trama W180-T.");
        }

        if (!TryParseSixDigitNumber(frameBytes, 10, out int tare))
        {
            buffer.RemoveAt(0);
            return ScaleFrameReadResult.InvalidFrame("No se pudo interpretar el campo TARA de la trama W180-T.");
        }

        buffer.RemoveRange(0, FrameLength);

        return ScaleFrameReadResult.FrameDecoded(
            new ScaleDecodedFrame
            {
                DisplayFrame = ByteFrameFormatter.FormatChunk(frameBytes, frameBytes.Length),
                WeightGrams = weight,
                TareGrams = tare
            });
    }

    private static bool TryParseSixDigitNumber(byte[] frameBytes, int offset, out int value)
    {
        value = 0;

        for (int index = 0; index < 6; index++)
        {
            byte current = frameBytes[offset + index];
            if (current is < (byte)'0' or > (byte)'9')
            {
                return false;
            }

            value = (value * 10) + (current - '0');
        }

        return true;
    }
}
