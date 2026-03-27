namespace GestionDeFardos.Core.Utils;

public static class WeightConversionHelper
{
    public static decimal RawValueToKg(int rawValue, int decimalDigits)
    {
        int normalizedDigits = NormalizeDecimalDigits(decimalDigits);
        decimal factor = 1m;

        for (int index = 0; index < normalizedDigits; index++)
        {
            factor *= 10m;
        }

        return rawValue / factor;
    }

    public static int NormalizeDecimalDigits(int decimalDigits)
    {
        if (decimalDigits < 0)
        {
            return 0;
        }

        return decimalDigits > 6 ? 6 : decimalDigits;
    }
}
