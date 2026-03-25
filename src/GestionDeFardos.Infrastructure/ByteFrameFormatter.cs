using System.Text;

namespace GestionDeFardos.Infrastructure;

internal static class ByteFrameFormatter
{
    public static string FormatChunk(byte[] chunk, int count)
    {
        if (chunk is null || count <= 0)
        {
            return "--";
        }

        return $"HEX: {ToHex(chunk, count)} | ASCII: {ToVisibleAscii(chunk, count)}";
    }

    public static string FormatChunk(IReadOnlyList<byte> bytes)
    {
        if (bytes.Count == 0)
        {
            return "--";
        }

        byte[] copy = new byte[bytes.Count];
        for (int index = 0; index < bytes.Count; index++)
        {
            copy[index] = bytes[index];
        }

        return FormatChunk(copy, copy.Length);
    }

    private static string ToHex(byte[] bytes, int count)
    {
        var builder = new StringBuilder(count * 3);
        for (int index = 0; index < count; index++)
        {
            if (index > 0)
            {
                builder.Append(' ');
            }

            builder.Append(bytes[index].ToString("X2"));
        }

        return builder.ToString();
    }

    private static string ToVisibleAscii(byte[] bytes, int count)
    {
        var builder = new StringBuilder(count);
        for (int index = 0; index < count; index++)
        {
            byte value = bytes[index];
            builder.Append(value is >= 32 and <= 126 ? (char)value : '.');
        }

        return builder.ToString();
    }
}
