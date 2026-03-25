using System.IO.Ports;
using GestionDeFardos.Core.Config;

namespace GestionDeFardos.Infrastructure;

internal static class SerialSettingsHelper
{
    public static Parity ParseParity(string? configuredValue, string settingName)
    {
        if (Enum.TryParse(configuredValue, ignoreCase: true, out Parity parity))
        {
            return parity;
        }

        throw new InvalidOperationException(
            $"{settingName} '{configuredValue}' no es valido. Valores soportados: {string.Join(", ", Enum.GetNames<Parity>())}.");
    }

    public static StopBits ParseStopBits(string? configuredValue, string settingName)
    {
        if (Enum.TryParse(configuredValue, ignoreCase: true, out StopBits stopBits))
        {
            return stopBits;
        }

        throw new InvalidOperationException(
            $"{settingName} '{configuredValue}' no es valido. Valores soportados: {string.Join(", ", Enum.GetNames<StopBits>())}.");
    }

    public static Handshake ParseHandshake(string? configuredValue, string settingName)
    {
        if (Enum.TryParse(configuredValue, ignoreCase: true, out Handshake handshake))
        {
            return handshake;
        }

        throw new InvalidOperationException(
            $"{settingName} '{configuredValue}' no es valido. Valores soportados: {string.Join(", ", Enum.GetNames<Handshake>())}.");
    }

    public static string NormalizeNewLine(string? newLine)
    {
        if (string.IsNullOrEmpty(newLine))
        {
            return "\n";
        }

        return newLine
            .Replace("\\r", "\r", StringComparison.Ordinal)
            .Replace("\\n", "\n", StringComparison.Ordinal);
    }

    public static string DescribeScaleProfile(ScaleSettings settings)
    {
        return BuildProfileText(
            settings.PortName,
            settings.BaudRate,
            settings.DataBits,
            settings.Parity,
            settings.StopBits,
            settings.Handshake);
    }

    public static string DescribeButtonProfile(ButtonSettings settings)
    {
        return BuildProfileText(
            settings.PortName,
            settings.BaudRate,
            settings.DataBits,
            settings.Parity,
            settings.StopBits,
            settings.Handshake);
    }

    private static string BuildProfileText(
        string? portName,
        int baudRate,
        int dataBits,
        string? parity,
        string? stopBits,
        string? handshake)
    {
        string normalizedPort = string.IsNullOrWhiteSpace(portName) ? "--" : portName.Trim();
        string normalizedParity = string.IsNullOrWhiteSpace(parity) ? "--" : parity.Trim();
        string normalizedStopBits = string.IsNullOrWhiteSpace(stopBits) ? "--" : stopBits.Trim();
        string normalizedHandshake = string.IsNullOrWhiteSpace(handshake) ? "--" : handshake.Trim();

        return $"{normalizedPort} | {baudRate} / {dataBits} / {normalizedParity} / {normalizedStopBits} / {normalizedHandshake}";
    }
}
