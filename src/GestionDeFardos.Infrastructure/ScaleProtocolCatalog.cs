namespace GestionDeFardos.Infrastructure;

internal static class ScaleProtocolCatalog
{
    private static readonly IScaleProtocol[] AllProtocols =
    [
        new SimpleAsciiScaleProtocol(),
        new W180TScaleProtocol()
    ];

    public static bool TryResolve(string? configuredValue, out IScaleProtocol? protocol)
    {
        protocol = null;

        if (string.IsNullOrWhiteSpace(configuredValue))
        {
            return false;
        }

        foreach (IScaleProtocol candidate in AllProtocols)
        {
            if (!string.Equals(candidate.Id, configuredValue.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            protocol = candidate;
            return true;
        }

        return false;
    }

    public static string DescribeSupportedValues()
    {
        return string.Join(", ", AllProtocols.Select(protocol => protocol.Id));
    }
}
