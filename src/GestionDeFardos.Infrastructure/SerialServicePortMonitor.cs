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
    private readonly IAppLogger _logger;
    private readonly object _sync = new();
    private readonly StringBuilder _buffer = new();

    private ServicePortSnapshot _snapshot = new();
    private SerialPort? _serialPort;
    private ButtonInputLine? _buttonInputLine;
    private bool? _lastButtonSignalState;

    public SerialServicePortMonitor(ScaleSettings scaleSettings, ButtonSettings buttonSettings, IAppLogger logger)
    {
        _scaleSettings = scaleSettings;
        _buttonSettings = buttonSettings;
        _logger = logger;
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
                SerialPort serialPort = BuildSerialPort();
                serialPort.DataReceived += OnDataReceived;
                serialPort.PinChanged += OnPinChanged;
                serialPort.Open();

                _serialPort = serialPort;
                _buffer.Clear();
                _snapshot = CreateConnectedSnapshot();

                RefreshControlLineStates();
                InitializeButtonMonitoring();

                _logger.Log(
                    AppLogLevel.Info,
                    "SERVICE",
                    $"Puerto {_scaleSettings.PortName} abierto. Linea configurada={_snapshot.ConfiguredButtonLine}, CTS={DescribeRawState(_snapshot.CtsState)}, DSR={DescribeRawState(_snapshot.DsrState)}, Pulsador={DescribeButtonState(_snapshot.ButtonState)}.");
            }
            catch (Exception ex)
            {
                string errorMessage = $"No se pudo abrir el puerto {_scaleSettings.PortName}: {ex.Message}";
                _snapshot = CreateDisconnectedSnapshot(errorMessage);
                _buttonInputLine = null;
                _lastButtonSignalState = null;
                _logger.Log(AppLogLevel.Error, "SERVICE", errorMessage);
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

            string portName = _scaleSettings.PortName;

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
                _snapshot.CtsState = null;
                _snapshot.DsrState = null;
                _snapshot.ButtonState = ServiceButtonState.Unknown;
                _snapshot.UpdatedAt = DateTime.Now;

                _logger.Log(AppLogLevel.Info, "SERVICE", $"Puerto {portName} cerrado.");
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
                ConfiguredButtonLine = _snapshot.ConfiguredButtonLine,
                CtsState = _snapshot.CtsState,
                DsrState = _snapshot.DsrState,
                ButtonState = _snapshot.ButtonState,
                LastButtonPressedAt = _snapshot.LastButtonPressedAt,
                ButtonLastError = _snapshot.ButtonLastError
            };
        }
    }

    private ServicePortSnapshot CreateConnectedSnapshot()
    {
        return new ServicePortSnapshot
        {
            IsConnected = true,
            UpdatedAt = DateTime.Now,
            LastError = null,
            ConfiguredButtonLine = FormatConfiguredButtonLine(_buttonSettings.InputLine),
            CtsState = null,
            DsrState = null,
            ButtonState = ServiceButtonState.Unknown,
            LastButtonPressedAt = null,
            ButtonLastError = null
        };
    }

    private ServicePortSnapshot CreateDisconnectedSnapshot(string errorMessage)
    {
        return new ServicePortSnapshot
        {
            IsConnected = false,
            UpdatedAt = DateTime.Now,
            LastError = errorMessage,
            ConfiguredButtonLine = FormatConfiguredButtonLine(_buttonSettings.InputLine),
            CtsState = null,
            DsrState = null,
            ButtonState = ServiceButtonState.Unknown,
            LastButtonPressedAt = null,
            ButtonLastError = errorMessage
        };
    }

    private void InitializeButtonMonitoring()
    {
        _lastButtonSignalState = null;

        if (!TryResolveButtonInputLine(_buttonSettings.InputLine, out ButtonInputLine inputLine))
        {
            _buttonInputLine = null;
            _snapshot.ButtonState = ServiceButtonState.Unknown;
            _snapshot.ButtonLastError =
                $"Button.InputLine '{_buttonSettings.InputLine}' no es valido. Valores soportados: Cts, Dsr.";
            _logger.Log(AppLogLevel.Warning, "BUTTON", _snapshot.ButtonLastError);
            return;
        }

        _buttonInputLine = inputLine;

        try
        {
            bool currentState = ReadButtonState(inputLine);
            _lastButtonSignalState = currentState;
            _snapshot.ButtonState = currentState ? ServiceButtonState.Pressed : ServiceButtonState.Released;
            _snapshot.ButtonLastError = null;
        }
        catch (Exception ex)
        {
            _buttonInputLine = null;
            _snapshot.ButtonState = ServiceButtonState.Unknown;
            _snapshot.ButtonLastError = $"No se pudo leer el estado inicial del pulsador: {ex.Message}";
            _logger.Log(AppLogLevel.Error, "BUTTON", _snapshot.ButtonLastError);
        }
    }

    private SerialPort BuildSerialPort()
    {
        Parity parity = Enum.Parse<Parity>(_scaleSettings.Parity, ignoreCase: true);
        StopBits stopBits = Enum.Parse<StopBits>(_scaleSettings.StopBits, ignoreCase: true);

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
                string chunk = _serialPort.ReadExisting();
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
            if (_serialPort is null || !IsTrackedPinChange(e.EventType))
            {
                return;
            }

            try
            {
                RefreshControlLineStates();
                _snapshot.UpdatedAt = DateTime.Now;

                _logger.Log(
                    AppLogLevel.Info,
                    "BUTTON",
                    $"Cambio de lineas detectado. Evento={e.EventType}, CTS={DescribeRawState(_snapshot.CtsState)}, DSR={DescribeRawState(_snapshot.DsrState)}.");

                if (_buttonInputLine is null || !IsRelevantPinChange(e.EventType, _buttonInputLine.Value))
                {
                    return;
                }

                bool currentState = ReadButtonState(_buttonInputLine.Value);
                bool stateChanged = _lastButtonSignalState != currentState;
                bool isNewPress = currentState && _lastButtonSignalState == false;

                _snapshot.ButtonState = currentState ? ServiceButtonState.Pressed : ServiceButtonState.Released;
                _snapshot.ButtonLastError = null;
                _lastButtonSignalState = currentState;

                if (isNewPress)
                {
                    _snapshot.LastButtonPressedAt = DateTime.Now;
                    _logger.Log(
                        AppLogLevel.Info,
                        "BUTTON",
                        $"Opresion detectada. Linea={DescribeInputLine(_buttonInputLine.Value)}, CTS={DescribeRawState(_snapshot.CtsState)}, DSR={DescribeRawState(_snapshot.DsrState)}.");
                }
                else if (stateChanged)
                {
                    _logger.Log(
                        AppLogLevel.Info,
                        "BUTTON",
                        $"Estado logico del pulsador={DescribeButtonState(_snapshot.ButtonState)}. Linea={DescribeInputLine(_buttonInputLine.Value)}.");
                }
            }
            catch (Exception ex)
            {
                _snapshot.ButtonState = ServiceButtonState.Unknown;
                _snapshot.ButtonLastError = $"Error leyendo el pulsador: {ex.Message}";
                _snapshot.UpdatedAt = DateTime.Now;
                _lastButtonSignalState = null;
                _logger.Log(AppLogLevel.Error, "BUTTON", _snapshot.ButtonLastError);
            }
        }
    }

    private void ProcessBufferLines()
    {
        while (TryReadLine(_buffer, out string line))
        {
            int? grams = TryExtractFirstInteger(line);
            decimal? kilograms = grams.HasValue ? WeightConversionHelper.GramsToKg(grams.Value) : null;

            _snapshot.RawFrame = line;
            _snapshot.RawGrams = grams;
            _snapshot.WeightKg = kilograms;
            _snapshot.UpdatedAt = DateTime.Now;
            _snapshot.IsConnected = true;
            _snapshot.LastError = grams.HasValue ? null : "No se detecto valor numerico en la trama.";

            if (grams.HasValue && kilograms.HasValue)
            {
                _logger.Log(
                    AppLogLevel.Info,
                    "SCALE",
                    $"Trama=\"{SanitizeFrameForLog(line)}\", Gramos={grams.Value}, Kg={kilograms.Value:F3}.");
            }
            else
            {
                _logger.Log(
                    AppLogLevel.Warning,
                    "SCALE",
                    $"Trama=\"{SanitizeFrameForLog(line)}\" sin valor numerico valido.");
            }
        }
    }

    private void RegisterPortError(string errorMessage)
    {
        _snapshot.IsConnected = false;
        _snapshot.LastError = errorMessage;
        _snapshot.CtsState = null;
        _snapshot.DsrState = null;
        _snapshot.ButtonState = ServiceButtonState.Unknown;
        _snapshot.ButtonLastError = errorMessage;
        _snapshot.UpdatedAt = DateTime.Now;
        _lastButtonSignalState = null;

        _logger.Log(AppLogLevel.Error, "SERVICE", errorMessage);
    }

    private void RefreshControlLineStates()
    {
        if (_serialPort is null || !_serialPort.IsOpen)
        {
            _snapshot.CtsState = null;
            _snapshot.DsrState = null;
            return;
        }

        _snapshot.CtsState = _serialPort.CtsHolding;
        _snapshot.DsrState = _serialPort.DsrHolding;
    }

    private bool ReadButtonState(ButtonInputLine inputLine)
    {
        return inputLine switch
        {
            ButtonInputLine.Cts => _snapshot.CtsState
                ?? throw new InvalidOperationException("No se pudo leer CTS."),
            ButtonInputLine.Dsr => _snapshot.DsrState
                ?? throw new InvalidOperationException("No se pudo leer DSR."),
            _ => throw new InvalidOperationException("La linea del pulsador no es compatible.")
        };
    }

    private static bool IsTrackedPinChange(SerialPinChange eventType)
    {
        return eventType is SerialPinChange.CtsChanged or SerialPinChange.DsrChanged;
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
        if (Enum.TryParse(configuredValue, ignoreCase: true, out inputLine))
        {
            return inputLine is ButtonInputLine.Cts or ButtonInputLine.Dsr;
        }

        inputLine = default;
        return false;
    }

    private static string FormatConfiguredButtonLine(string? configuredValue)
    {
        return string.IsNullOrWhiteSpace(configuredValue)
            ? "--"
            : configuredValue.Trim().ToUpperInvariant();
    }

    private static string DescribeInputLine(ButtonInputLine inputLine)
    {
        return inputLine switch
        {
            ButtonInputLine.Cts => "CTS",
            ButtonInputLine.Dsr => "DSR",
            _ => inputLine.ToString().ToUpperInvariant()
        };
    }

    private static string DescribeRawState(bool? state)
    {
        return state switch
        {
            true => "Activa",
            false => "Inactiva",
            null => "Sin lectura"
        };
    }

    private static string DescribeButtonState(ServiceButtonState buttonState)
    {
        return buttonState switch
        {
            ServiceButtonState.Pressed => "Presionado",
            ServiceButtonState.Released => "No presionado",
            _ => "Sin lectura"
        };
    }

    private static string SanitizeFrameForLog(string frame)
    {
        return frame
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);
    }

    private static bool TryReadLine(StringBuilder buffer, out string line)
    {
        string content = buffer.ToString();
        int lineBreakIndex = content.IndexOf('\n');
        if (lineBreakIndex < 0)
        {
            line = string.Empty;
            return false;
        }

        string rawLine = content[..lineBreakIndex].TrimEnd('\r');
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

        foreach (string token in frame.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                return value;
            }
        }

        return null;
    }
}
