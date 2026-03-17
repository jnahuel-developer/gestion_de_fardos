using System.Globalization;
using System.IO.Ports;
using System.Text;
using GestionDeFardos.Core.Config;
using GestionDeFardos.Core.Interfaces;
using GestionDeFardos.Core.Models;
using GestionDeFardos.Core.Utils;

namespace GestionDeFardos.Infrastructure;

public sealed class SerialServicePortMonitor : IServicePortMonitor
{
    private readonly ScaleSettings _scaleSettings;
    private readonly ButtonSettings _buttonSettings;
    private readonly object _sync = new();
    private readonly StringBuilder _buffer = new();

    private ServicePortSnapshot _snapshot = new();
    private SerialPort? _serialPort;
    private ButtonInputLine? _buttonInputLine;
    private bool? _lastButtonSignalState;

    public SerialServicePortMonitor(ScaleSettings scaleSettings, ButtonSettings buttonSettings)
    {
        _scaleSettings = scaleSettings;
        _buttonSettings = buttonSettings;
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
                serialPort.PinChanged += OnPinChanged;
                serialPort.Open();

                _serialPort = serialPort;
                _buffer.Clear();
                _snapshot = new ServicePortSnapshot
                {
                    IsConnected = true,
                    UpdatedAt = DateTime.Now,
                    LastError = null,
                    ButtonState = ServiceButtonState.Unknown,
                    ButtonLastError = null
                };

                InitializeButtonMonitoring();
            }
            catch (Exception ex)
            {
                string errorMessage = $"No se pudo abrir el puerto {_scaleSettings.PortName}: {ex.Message}";

                _snapshot = new ServicePortSnapshot
                {
                    IsConnected = false,
                    UpdatedAt = DateTime.Now,
                    LastError = errorMessage,
                    ButtonState = ServiceButtonState.Unknown,
                    ButtonLastError = errorMessage
                };

                _buttonInputLine = null;
                _lastButtonSignalState = null;
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
                _serialPort.PinChanged -= OnPinChanged;

                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
            }
            finally
            {
                _serialPort.Dispose();
                _serialPort = null;
                _buttonInputLine = null;
                _lastButtonSignalState = null;
                _buffer.Clear();

                _snapshot.IsConnected = false;
                _snapshot.ButtonState = ServiceButtonState.Unknown;
                _snapshot.UpdatedAt = DateTime.Now;
            }
        }
    }

    public ServicePortSnapshot GetSnapshot()
    {
        lock (_sync)
        {
            return new ServicePortSnapshot
            {
                RawFrame = _snapshot.RawFrame,
                RawGrams = _snapshot.RawGrams,
                WeightKg = _snapshot.WeightKg,
                UpdatedAt = _snapshot.UpdatedAt,
                IsConnected = _snapshot.IsConnected,
                LastError = _snapshot.LastError,
                ButtonState = _snapshot.ButtonState,
                LastButtonPressedAt = _snapshot.LastButtonPressedAt,
                ButtonLastError = _snapshot.ButtonLastError
            };
        }
    }

    private void InitializeButtonMonitoring()
    {
        _lastButtonSignalState = null;

        if (!TryResolveButtonInputLine(_buttonSettings.InputLine, out var inputLine))
        {
            _buttonInputLine = null;
            _snapshot.ButtonState = ServiceButtonState.Unknown;
            _snapshot.ButtonLastError =
                $"Button.InputLine '{_buttonSettings.InputLine}' no es válido. Valores soportados: Cts, Dsr.";
            return;
        }

        _buttonInputLine = inputLine;

        try
        {
            bool currentState = ReadButtonState();
            _lastButtonSignalState = currentState;
            _snapshot.ButtonState = currentState ? ServiceButtonState.Pressed : ServiceButtonState.Released;
            _snapshot.ButtonLastError = null;
        }
        catch (Exception ex)
        {
            _buttonInputLine = null;
            _snapshot.ButtonState = ServiceButtonState.Unknown;
            _snapshot.ButtonLastError = $"No se pudo leer el estado inicial del pulsador: {ex.Message}";
        }
    }

    private SerialPort BuildSerialPort()
    {
        var parity = Enum.Parse<Parity>(_scaleSettings.Parity, ignoreCase: true);
        var stopBits = Enum.Parse<StopBits>(_scaleSettings.StopBits, ignoreCase: true);

        return new SerialPort(_scaleSettings.PortName, _scaleSettings.BaudRate, parity, _scaleSettings.DataBits, stopBits)
        {
            Encoding = Encoding.ASCII,
            Handshake = Handshake.None,
            NewLine = NormalizeNewLine(_scaleSettings.NewLine),
            DtrEnable = false,
            RtsEnable = false
        };
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
                RegisterPortError($"Error de lectura serial: {ex.Message}");
            }
        }
    }

    private void OnPinChanged(object sender, SerialPinChangedEventArgs e)
    {
        lock (_sync)
        {
            if (_serialPort is null || _buttonInputLine is null || !IsRelevantPinChange(e.EventType, _buttonInputLine.Value))
            {
                return;
            }

            try
            {
                bool currentState = ReadButtonState();
                bool wasReleased = _lastButtonSignalState == false;

                if (currentState && wasReleased)
                {
                    _snapshot.LastButtonPressedAt = DateTime.Now;
                }

                _snapshot.ButtonState = currentState ? ServiceButtonState.Pressed : ServiceButtonState.Released;
                _snapshot.ButtonLastError = null;
                _snapshot.UpdatedAt = DateTime.Now;
                _lastButtonSignalState = currentState;
            }
            catch (Exception ex)
            {
                _snapshot.ButtonState = ServiceButtonState.Unknown;
                _snapshot.ButtonLastError = $"Error leyendo el pulsador: {ex.Message}";
                _snapshot.UpdatedAt = DateTime.Now;
                _lastButtonSignalState = null;
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

    private void RegisterPortError(string errorMessage)
    {
        _snapshot.IsConnected = false;
        _snapshot.LastError = errorMessage;
        _snapshot.ButtonState = ServiceButtonState.Unknown;
        _snapshot.ButtonLastError = errorMessage;
        _snapshot.UpdatedAt = DateTime.Now;
        _lastButtonSignalState = null;
    }

    private bool ReadButtonState()
    {
        if (_serialPort is null || _buttonInputLine is null)
        {
            throw new InvalidOperationException("El monitoreo del pulsador no está inicializado.");
        }

        return _buttonInputLine.Value switch
        {
            ButtonInputLine.Cts => _serialPort.CtsHolding,
            ButtonInputLine.Dsr => _serialPort.DsrHolding,
            _ => throw new InvalidOperationException("La línea del pulsador no es compatible.")
        };
    }

    private static bool IsRelevantPinChange(SerialPinChange eventType, ButtonInputLine inputLine)
    {
        return inputLine switch
        {
            ButtonInputLine.Cts => eventType == SerialPinChange.CtsChanged,
            ButtonInputLine.Dsr => eventType == SerialPinChange.DsrChanged,
            _ => false
        };
    }

    private static bool TryResolveButtonInputLine(string? configuredValue, out ButtonInputLine inputLine)
    {
        if (Enum.TryParse<ButtonInputLine>(configuredValue, ignoreCase: true, out inputLine))
        {
            return inputLine is ButtonInputLine.Cts or ButtonInputLine.Dsr;
        }

        inputLine = default;
        return false;
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
