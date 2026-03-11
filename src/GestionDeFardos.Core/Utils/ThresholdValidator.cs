using GestionDeFardos.Core.Config;

namespace GestionDeFardos.Core.Utils;

public static class ThresholdValidator
{
    public static bool IsWithinRange(decimal weightKg, ThresholdSettings thresholds)
    {
        return weightKg >= thresholds.MinKg && weightKg <= thresholds.MaxKg;
    }
}
