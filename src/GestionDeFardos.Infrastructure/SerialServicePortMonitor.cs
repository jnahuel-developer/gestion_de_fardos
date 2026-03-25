using System.IO.Ports;
using System.Text;
using GestionDeFardos.Core.Config;
using GestionDeFardos.Core.Interfaces;
using GestionDeFardos.Core.Models;
using GestionDeFardos.Core.Utils;

namespace GestionDeFardos.Infrastructure;

public sealed class SerialServicePortMonitor : IServicePortMonitor
{
    private static readonly byte[] ButtonRequestBytes = Encoding.ASCII.GetBytes("$P1!");
    private static readonly byte[] ButtonResponseBytes = Encoding.ASCII.GetBytes("$B1!");

    private readonly ScaleSettings _scaleSettings;
    private readonly ButtonSettings _buttonSettings;
    private readonly IAppLogger _logger;
    private readonly object _sync = new();
    private readonly List<byte> _scaleBuffer = [];
    private readonly List<byte> _buttonBuffer = [];
    private readonly string _configuredScaleProtocol;
    private readonly IScaleProtocol? _scaleProtocol;

    private ServicePortSnapshot _snapshot = new();
    private SerialPort? _scalePort;
    private SerialPort? _buttonPort;
    private bool _started;

    public SerialServicePortMonitor(ScaleSettings scaleSettings, ButtonSettings buttonSettings, IAppLogger logger)
    {
        _scaleSettings = scaleSettings;
        _buttonSettings = buttonSettings;
        _logger = logger;
        _configuredScaleProtocol = string.IsNullOrWhiteSpace(scaleSettings.Protocol)
            ? string.Empty
            : scaleSettings.Protocol.Trim();

        if (ScaleProtocolCatalog.TryResolve(_configuredScaleProtocol, out IScaleProtocol? scaleProtocol))
        {
            _scaleProtocol = scaleProtocol;
        }
    }

    public void Start()
    {
        lock (_sync)
        {
            if (_started)
            {
                return;
            }

            _started = true;
            _scaleBuffer.Clear();
            _buttonBuffer.Clear();
            _snapshot = CreateInitialSnapshot();

            _logger.Log(
                AppLogLevel.Info,
                "SERVICE",
                $"Iniciando Service. ProtocoloBalanza={DisplayScaleProtocol()}, PerfilBalanza={_snapshot.ScalePortProfile}, PerfilPulsador={_snapshot.ButtonPortProfile}.");

            OpenScalePort();
            OpenButtonPort();
        }
    }

    public void Stop()
    {
        lock (_sync)
        {
            if (!_started && _scalePort is null && _buttonPort is null)
            {
                return;
            }

            CloseScalePort();
            CloseButtonPort();

            _started = false;
            _scaleBuffer.Clear();
            _buttonBuffer.Clear();
            _snapshot.IsConnected = false;
            _snapshot.ButtonIsConnected = false;
            _snapshot.ButtonStatus = _snapshot.ButtonIsConfigured
                ? ServiceButtonState.Disconnected
                : ServiceButtonState.Disabled;
            _snapshot.UpdatedAt = DateTime.Now;
        }
    }

    public ServicePortSnapshot GetSnapshot()
    {
        lock (_sync)
        {
            return new ServicePortSnapshot
            {
                ScaleProtocol = _snapshot.ScaleProtocol,
                ScalePortProfile = _snapshot.ScalePortProfile,
                ScaleRawChunk = _snapshot.ScaleRawChunk,
                ScaleLastDecodedFrame = _snapshot.ScaleLastDecodedFrame,
                RawGrams = _snapshot.RawGrams,
                RawTareGrams = _snapshot.RawTareGrams,
                WeightKg = _snapshot.WeightKg,
                UpdatedAt = _snapshot.UpdatedAt,
                IsConnected = _snapshot.IsConnected,
                LastError = _snapshot.LastError,
                ButtonPortProfile = _snapshot.ButtonPortProfile,
                ButtonIsConfigured = _snapshot.ButtonIsConfigured,
                ButtonIsConnected = _snapshot.ButtonIsConnected,
                ButtonStatus = _snapshot.ButtonStatus,
                LastButtonRawChunk = _snapshot.LastButtonRawChunk,
                LastButtonFrame = _snapshot.LastButtonFrame,
                LastButtonResponse = _snapshot.LastButtonResponse,
                LastButtonPressedAt = _snapshot.LastButtonPressedAt,
                ButtonLastError = _snapshot.ButtonLastError
            };
        }
    }

    private ServicePortSnapshot CreateInitialSnapshot()
    {
        bool buttonConfigured = !string.IsNullOrWhiteSpace(_buttonSettings.PortName);
        string? scaleProtocolError = BuildUnknownProtocolMessage();

        return new ServicePortSnapshot
        {
            ScaleProtocol = DisplayScaleProtocol(),
            ScalePortProfile = SerialSettingsHelper.DescribeScaleProfile(_scaleSettings),
            ScaleRawChunk = string.Empty,
            ScaleLastDecodedFrame = string.Empty,
            RawGrams = null,
            RawTareGrams = null,
            WeightKg = null,
            UpdatedAt = DateTime.Now,
            IsConnected = false,
            LastError = scaleProtocolError,
            ButtonPortProfile = SerialSettingsHelper.DescribeButtonProfile(_buttonSettings),
            ButtonIsConfigured = buttonConfigured,
            ButtonIsConnected = false,
            ButtonStatus = buttonConfigured ? ServiceButtonState.Disconnected : ServiceButtonState.Disabled,
            LastButtonRawChunk = string.Empty,
            LastButtonFrame = string.Empty,
            LastButtonResponse = string.Empty,
            LastButtonPressedAt = null,
            ButtonLastError = buttonConfigured ? null : "Button.PortName esta vacio. Service abrira sin pulsador."
        };
    }

    private void OpenScalePort()
    {
        if (string.IsNullOrWhiteSpace(_scaleSettings.PortName))
        {
            RegisterScalePortError("Scale.PortName esta vacio.");
            return;
        }

        try
        {
            SerialPort serialPort = BuildScaleSerialPort();
            serialPort.DataReceived += OnScaleDataReceived;
            serialPort.Open();

            _scalePort = serialPort;
            _snapshot.IsConnected = true;
            _snapshot.UpdatedAt = DateTime.Now;

            if (_scaleProtocol is not null)
            {
                _snapshot.LastError = null;
            }

            _logger.Log(
                AppLogLevel.Info,
                "SCALE",
                $"Puerto {_scaleSettings.PortName} abierto. Protocolo={DisplayScaleProtocol()}, Perfil={_snapshot.ScalePortProfile}.");

            if (_scaleProtocol is null)
            {
                string warning = BuildUnknownProtocolMessage()!;
                _snapshot.LastError = warning;
                _logger.Log(AppLogLevel.Warning, "SCALE", warning);
            }
        }
        catch (Exception ex)
        {
            RegisterScalePortError($"No se pudo abrir el puerto {_scaleSettings.PortName}: {ex.Message}");
        }
    }

    private void OpenButtonPort()
    {
        if (!_snapshot.ButtonIsConfigured)
        {
            _logger.Log(AppLogLevel.Info, "BUTTON", _snapshot.ButtonLastError ?? "Pulsador deshabilitado.");
            return;
        }

        if (string.Equals(_buttonSettings.PortName, _scaleSettings.PortName, StringComparison.OrdinalIgnoreCase))
        {
            RegisterButtonError("Button.PortName no puede coincidir con Scale.PortName.");
            return;
        }

        try
        {
            SerialPort serialPort = BuildButtonSerialPort();
            serialPort.DataReceived += OnButtonDataReceived;
            serialPort.Open();

            _buttonPort = serialPort;
            _snapshot.ButtonIsConnected = true;
            _snapshot.ButtonStatus = ServiceButtonState.Listening;
            _snapshot.ButtonLastError = null;
            _snapshot.UpdatedAt = DateTime.Now;

            _logger.Log(
                AppLogLevel.Info,
                "BUTTON",
                $"Puerto {_buttonSettings.PortName} abierto. Perfil={_snapshot.ButtonPortProfile}. Esperando $P1!.");
        }
        catch (Exception ex)
        {
            RegisterButtonError($"No se pudo abrir el puerto {_buttonSettings.PortName}: {ex.Message}");
        }
    }

    private void CloseScalePort()
    {
        if (_scalePort is null)
        {
            return;
        }

        string portName = _scaleSettings.PortName;

        try
        {
            _scalePort.DataReceived -= OnScaleDataReceived;

            if (_scalePort.IsOpen)
            {
                _scalePort.Close();
            }
        }
        finally
        {
            _scalePort.Dispose();
            _scalePort = null;
            _logger.Log(AppLogLevel.Info, "SCALE", $"Puerto {portName} cerrado.");
        }
    }

    private void CloseButtonPort()
    {
        if (_buttonPort is null)
        {
            return;
        }

        string portName = _buttonSettings.PortName;

        try
        {
            _buttonPort.DataReceived -= OnButtonDataReceived;

            if (_buttonPort.IsOpen)
            {
                _buttonPort.Close();
            }
        }
        finally
        {
            _buttonPort.Dispose();
            _buttonPort = null;
            _logger.Log(AppLogLevel.Info, "BUTTON", $"Puerto {portName} cerrado.");
        }
    }

    private SerialPort BuildScaleSerialPort()
    {
        Parity parity = SerialSettingsHelper.ParseParity(_scaleSettings.Parity, "Scale.Parity");
        StopBits stopBits = SerialSettingsHelper.ParseStopBits(_scaleSettings.StopBits, "Scale.StopBits");
        Handshake handshake = SerialSettingsHelper.ParseHandshake(_scaleSettings.Handshake, "Scale.Handshake");

        return new SerialPort(_scaleSettings.PortName, _scaleSettings.BaudRate, parity, _scaleSettings.DataBits, stopBits)
        {
            Encoding = Encoding.ASCII,
            Handshake = handshake,
            NewLine = SerialSettingsHelper.NormalizeNewLine(_scaleSettings.NewLine),
            DtrEnable = false,
            RtsEnable = false
        };
    }

    private SerialPort BuildButtonSerialPort()
    {
        Parity parity = SerialSettingsHelper.ParseParity(_buttonSettings.Parity, "Button.Parity");
        StopBits stopBits = SerialSettingsHelper.ParseStopBits(_buttonSettings.StopBits, "Button.StopBits");
        Handshake handshake = SerialSettingsHelper.ParseHandshake(_buttonSettings.Handshake, "Button.Handshake");

        return new SerialPort(_buttonSettings.PortName, _buttonSettings.BaudRate, parity, _buttonSettings.DataBits, stopBits)
        {
            Encoding = Encoding.ASCII,
            Handshake = handshake,
            DtrEnable = false,
            RtsEnable = false
        };
    }

    private void OnScaleDataReceived(object? sender, SerialDataReceivedEventArgs e)
    {
        lock (_sync)
        {
            if (_scalePort is null)
            {
                return;
            }

            try
            {
                int bytesToRead = _scalePort.BytesToRead;
                if (bytesToRead <= 0)
                {
                    return;
                }

                byte[] chunk = new byte[bytesToRead];
                int bytesRead = _scalePort.Read(chunk, 0, chunk.Length);
                if (bytesRead <= 0)
                {
                    return;
                }

                string rawChunk = ByteFrameFormatter.FormatChunk(chunk, bytesRead);
                _snapshot.ScaleRawChunk = rawChunk;
                _snapshot.UpdatedAt = DateTime.Now;
                _snapshot.IsConnected = true;

                _logger.Log(
                    AppLogLevel.Info,
                    "SCALE",
                    $"Chunk RX. Bytes={bytesRead}, Protocolo={DisplayScaleProtocol()}, {rawChunk}");

                if (_scaleProtocol is null)
                {
                    _snapshot.LastError = BuildUnknownProtocolMessage();
                    return;
                }

                AppendBytes(_scaleBuffer, chunk, bytesRead);
                ProcessScaleFrames();
            }
            catch (Exception ex)
            {
                RegisterScalePortError($"Error de lectura serial en balanza: {ex.Message}");
            }
        }
    }

    private void OnButtonDataReceived(object? sender, SerialDataReceivedEventArgs e)
    {
        lock (_sync)
        {
            if (_buttonPort is null)
            {
                return;
            }

            try
            {
                int bytesToRead = _buttonPort.BytesToRead;
                if (bytesToRead <= 0)
                {
                    return;
                }

                byte[] chunk = new byte[bytesToRead];
                int bytesRead = _buttonPort.Read(chunk, 0, chunk.Length);
                if (bytesRead <= 0)
                {
                    return;
                }

                string rawChunk = ByteFrameFormatter.FormatChunk(chunk, bytesRead);
                _snapshot.LastButtonRawChunk = rawChunk;
                _snapshot.UpdatedAt = DateTime.Now;
                _snapshot.ButtonIsConnected = true;

                _logger.Log(
                    AppLogLevel.Info,
                    "BUTTON",
                    $"Chunk RX. Bytes={bytesRead}, {rawChunk}");

                AppendBytes(_buttonBuffer, chunk, bytesRead);
                ProcessButtonFrames();
            }
            catch (Exception ex)
            {
                RegisterButtonError($"Error de lectura serial en pulsador: {ex.Message}");
            }
        }
    }

    private void ProcessScaleFrames()
    {
        if (_scaleProtocol is null)
        {
            return;
        }

        while (true)
        {
            ScaleFrameReadResult result = _scaleProtocol.TryReadFrame(_scaleBuffer, _scaleSettings);

            switch (result.Status)
            {
                case ScaleFrameReadStatus.NeedMoreData:
                    return;

                case ScaleFrameReadStatus.InvalidFrame:
                    RegisterScaleFrameWarning(result.ErrorMessage ?? "Se recibio una trama invalida.");
                    continue;

                case ScaleFrameReadStatus.FrameDecoded:
                    ApplyScaleFrame(result.Frame!);
                    continue;

                default:
                    return;
            }
        }
    }

    private void ProcessButtonFrames()
    {
        while (true)
        {
            int frameIndex = ByteSequenceHelper.FindSequence(_buttonBuffer, ButtonRequestBytes);
            if (frameIndex < 0)
            {
                ByteSequenceHelper.TrimBufferToPartialMatch(_buttonBuffer, ButtonRequestBytes);
                return;
            }

            if (frameIndex > 0)
            {
                _buttonBuffer.RemoveRange(0, frameIndex);
                _logger.Log(AppLogLevel.Warning, "BUTTON", $"Se descartaron {frameIndex} bytes previos a $P1!.");
            }

            if (_buttonBuffer.Count < ButtonRequestBytes.Length)
            {
                return;
            }

            _buttonBuffer.RemoveRange(0, ButtonRequestBytes.Length);
            HandleButtonRequestReceived();
        }
    }

    private void HandleButtonRequestReceived()
    {
        _snapshot.ButtonStatus = ServiceButtonState.Listening;
        _snapshot.LastButtonFrame = "$P1!";
        _snapshot.LastButtonPressedAt = DateTime.Now;
        _snapshot.ButtonLastError = null;
        _snapshot.UpdatedAt = DateTime.Now;

        _logger.Log(AppLogLevel.Info, "BUTTON", "BUTTON RX $P1!.");

        try
        {
            if (_buttonPort is null || !_buttonPort.IsOpen)
            {
                throw new InvalidOperationException("El puerto del pulsador no esta abierto.");
            }

            _buttonPort.Write(ButtonResponseBytes, 0, ButtonResponseBytes.Length);
            _snapshot.LastButtonResponse = "$B1!";
            _snapshot.ButtonStatus = ServiceButtonState.Listening;
            _snapshot.ButtonLastError = null;
            _snapshot.UpdatedAt = DateTime.Now;

            _logger.Log(AppLogLevel.Info, "BUTTON", "BUTTON TX $B1!.");
        }
        catch (Exception ex)
        {
            RegisterButtonError($"Error enviando $B1! al pulsador: {ex.Message}");
        }
    }

    private void ApplyScaleFrame(ScaleDecodedFrame frame)
    {
        decimal? kilograms = frame.WeightGrams.HasValue
            ? WeightConversionHelper.GramsToKg(frame.WeightGrams.Value)
            : null;

        _snapshot.ScaleLastDecodedFrame = frame.DisplayFrame;
        _snapshot.RawGrams = frame.WeightGrams;
        _snapshot.RawTareGrams = frame.TareGrams;
        _snapshot.WeightKg = kilograms;
        _snapshot.IsConnected = true;
        _snapshot.LastError = null;
        _snapshot.UpdatedAt = DateTime.Now;

        string tareSegment = frame.TareGrams.HasValue ? $", TaraGramos={frame.TareGrams.Value}" : string.Empty;
        string weightSegment = frame.WeightGrams.HasValue && kilograms.HasValue
            ? $"Gramos={frame.WeightGrams.Value}, Kg={kilograms.Value:F3}"
            : "Sin peso numerico";

        _logger.Log(
            AppLogLevel.Info,
            "SCALE",
            $"Trama interpretada. Protocolo={DisplayScaleProtocol()}, {weightSegment}{tareSegment}, Decodificada=\"{frame.DisplayFrame}\".");
    }

    private void RegisterScaleFrameWarning(string message)
    {
        _snapshot.IsConnected = true;
        _snapshot.LastError = $"Protocolo {DisplayScaleProtocol()}: {message}";
        _snapshot.UpdatedAt = DateTime.Now;
        _logger.Log(AppLogLevel.Warning, "SCALE", _snapshot.LastError);
    }

    private void RegisterScalePortError(string errorMessage)
    {
        _snapshot.IsConnected = false;
        _snapshot.LastError = errorMessage;
        _snapshot.UpdatedAt = DateTime.Now;
        _scaleBuffer.Clear();

        _logger.Log(AppLogLevel.Error, "SCALE", errorMessage);
    }

    private void RegisterButtonError(string errorMessage)
    {
        _snapshot.ButtonIsConnected = false;
        _snapshot.ButtonStatus = _snapshot.ButtonIsConfigured ? ServiceButtonState.Error : ServiceButtonState.Disabled;
        _snapshot.ButtonLastError = errorMessage;
        _snapshot.UpdatedAt = DateTime.Now;
        _buttonBuffer.Clear();

        _logger.Log(AppLogLevel.Error, "BUTTON", errorMessage);
    }

    private string DisplayScaleProtocol()
    {
        return string.IsNullOrWhiteSpace(_configuredScaleProtocol) ? "--" : _configuredScaleProtocol;
    }

    private string? BuildUnknownProtocolMessage()
    {
        if (_scaleProtocol is not null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(_configuredScaleProtocol))
        {
            return $"Scale.Protocol esta vacio. Se mostrara solo recepcion cruda. Protocolos soportados: {ScaleProtocolCatalog.DescribeSupportedValues()}.";
        }

        return $"Scale.Protocol '{_configuredScaleProtocol}' no es soportado. Se mostrara solo recepcion cruda. Protocolos soportados: {ScaleProtocolCatalog.DescribeSupportedValues()}.";
    }

    private static void AppendBytes(List<byte> buffer, byte[] chunk, int count)
    {
        for (int index = 0; index < count; index++)
        {
            buffer.Add(chunk[index]);
        }

        const int maxBufferedBytes = 4096;
        if (buffer.Count > maxBufferedBytes)
        {
            buffer.RemoveRange(0, buffer.Count - maxBufferedBytes);
        }
    }
}
