namespace GestionDeFardos.Infrastructure;

internal static class ByteSequenceHelper
{
    public static int FindSequence(List<byte> buffer, byte[] sequence)
    {
        if (buffer.Count < sequence.Length)
        {
            return -1;
        }

        for (int bufferIndex = 0; bufferIndex <= buffer.Count - sequence.Length; bufferIndex++)
        {
            bool matches = true;

            for (int sequenceIndex = 0; sequenceIndex < sequence.Length; sequenceIndex++)
            {
                if (buffer[bufferIndex + sequenceIndex] != sequence[sequenceIndex])
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
            {
                return bufferIndex;
            }
        }

        return -1;
    }

    public static void TrimBufferToPartialMatch(List<byte> buffer, byte[] sequence)
    {
        int maxSuffixLength = Math.Min(buffer.Count, sequence.Length - 1);

        for (int suffixLength = maxSuffixLength; suffixLength > 0; suffixLength--)
        {
            bool matches = true;

            for (int index = 0; index < suffixLength; index++)
            {
                if (buffer[buffer.Count - suffixLength + index] != sequence[index])
                {
                    matches = false;
                    break;
                }
            }

            if (!matches)
            {
                continue;
            }

            int bytesToRemove = buffer.Count - suffixLength;
            if (bytesToRemove > 0)
            {
                buffer.RemoveRange(0, bytesToRemove);
            }

            return;
        }

        buffer.Clear();
    }
}
