using System.Globalization;
using System.IO.Ports;
using System.Text;
using GestionDeFardos.Core.Config;
using GestionDeFardos.Core.Interfaces;
using GestionDeFardos.Core.Models;
using GestionDeFardos.Core.Utils;

namespace GestionDeFardos.Infrastructure;

public sealed class SerialScaleReader : IScaleReader
{
    private readonly ScaleSettings _settings;
    private readonly object _sync = new();
    private readonly StringBuilder _buffer = new();
    private ScaleSnapshot _snapshot = new();
    private SerialPort? _serialPort;

    public SerialScaleReader(ScaleSettings settings)
    {
        _settings = settings;
    }

    public void Start()
    {
        lock (_sync)
        {
            if (_serialPort is not null)
            {
                return;
            }

            try
            {
                var serialPort = BuildSerialPort();
                serialPort.DataReceived += OnDataReceived;
                serialPort.Open();
                _serialPort = serialPort;

                _snapshot = new ScaleSnapshot
                {
                    IsConnected = true,
                    UpdatedAt = DateTime.Now,
                    LastError = null
                };
            }
            catch (Exception ex)
            {
                _snapshot = new ScaleSnapshot
                {
                    IsConnected = false,
                    UpdatedAt = DateTime.Now,
                    LastError = $"No se pudo abrir el puerto {_settings.PortName}: {ex.Message}"
                };
            }
        }
    }

    public void Stop()
    {
        lock (_sync)
        {
            if (_serialPort is null)
            {
                return;
            }

            try
            {
                _serialPort.DataReceived -= OnDataReceived;
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
            }
            finally
            {
                _serialPort.Dispose();
                _serialPort = null;
                _buffer.Clear();
                _snapshot.IsConnected = false;
            }
        }
    }

    public ScaleSnapshot GetSnapshot()
    {
        lock (_sync)
        {
            return new ScaleSnapshot
            {
                RawFrame = _snapshot.RawFrame,
                RawGrams = _snapshot.RawGrams,
                WeightKg = _snapshot.WeightKg,
                UpdatedAt = _snapshot.UpdatedAt,
                IsConnected = _snapshot.IsConnected,
                LastError = _snapshot.LastError
            };
        }
    }

    private SerialPort BuildSerialPort()
    {
        var parity = Enum.Parse<Parity>(_settings.Parity, ignoreCase: true);
        var stopBits = Enum.Parse<StopBits>(_settings.StopBits, ignoreCase: true);

        var serialPort = new SerialPort(_settings.PortName, _settings.BaudRate, parity, _settings.DataBits, stopBits)
        {
            Encoding = Encoding.ASCII,
            NewLine = NormalizeNewLine(_settings.NewLine)
        };

        return serialPort;
    }

    private static string NormalizeNewLine(string newLine)
    {
        if (string.IsNullOrEmpty(newLine))
        {
            return "\n";
        }

        return newLine
            .Replace("\\r", "\r", StringComparison.Ordinal)
            .Replace("\\n", "\n", StringComparison.Ordinal);
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        lock (_sync)
        {
            if (_serialPort is null)
            {
                return;
            }

            try
            {
                var chunk = _serialPort.ReadExisting();
                if (string.IsNullOrEmpty(chunk))
                {
                    return;
                }

                _buffer.Append(chunk);
                ProcessBufferLines();
            }
            catch (Exception ex)
            {
                _snapshot.IsConnected = false;
                _snapshot.LastError = $"Error de lectura serial: {ex.Message}";
                _snapshot.UpdatedAt = DateTime.Now;
            }
        }
    }

    private void ProcessBufferLines()
    {
        while (TryReadLine(_buffer, out var line))
        {
            var grams = TryExtractFirstInteger(line);
            decimal? kilograms = grams.HasValue ? WeightConversionHelper.GramsToKg(grams.Value) : null;

            _snapshot.RawFrame = line;
            _snapshot.RawGrams = grams;
            _snapshot.WeightKg = kilograms;
            _snapshot.UpdatedAt = DateTime.Now;
            _snapshot.IsConnected = true;
            _snapshot.LastError = grams.HasValue ? null : "No se detectó valor numérico en la trama.";
        }
    }

    private static bool TryReadLine(StringBuilder buffer, out string line)
    {
        var content = buffer.ToString();
        var lineBreakIndex = content.IndexOf('\n');
        if (lineBreakIndex < 0)
        {
            line = string.Empty;
            return false;
        }

        var rawLine = content[..lineBreakIndex].TrimEnd('\r');
        buffer.Remove(0, lineBreakIndex + 1);
        line = rawLine;
        return true;
    }

    private static int? TryExtractFirstInteger(string frame)
    {
        if (string.IsNullOrWhiteSpace(frame))
        {
            return null;
        }

        foreach (var token in frame.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }
        }

        return null;
    }
}
