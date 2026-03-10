namespace GestionDeFardos.Core.Utils;

public static class WeightConversionHelper
{
    public static decimal GramsToKg(int grams)
    {
        return grams / 1000m;
    }
}
