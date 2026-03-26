using System.Runtime.InteropServices;
using GestionDeFardos.Core.Config;
using GestionDeFardos.Core.Interfaces;
using GestionDeFardos.Core.Models;
using GestionDeFardos.Infrastructure;
using GestionDeFardos.Infrastructure.Config;

namespace GestionDeFardos.App;

public sealed class MainForm : Form
{
    private const int WmHotKey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const int VkS = 0x53;

    private const int HotkeyIdCtrlShiftS = 1001;
    private const int HotkeyIdCtrlAltShiftS = 1002;

    private readonly IAppLogger _logger;
    private readonly AppSettings _settings;
    private readonly string _configPath;
    private readonly bool _configFileExists;
    private readonly bool _configLoadedSuccessfully;
    private readonly IWeighingRepository? _weighingRepository;
    private readonly IWeighingRuntime _weighingRuntime;
    private readonly Label _scaleConnectionLabel;
    private readonly Label _currentWeightLabel;
    private readonly Label _captureStatusLabel;
    private readonly Label _captureMessageLabel;
    private readonly Label _lastCaptureAtLabel;
    private readonly Label _lastRecordLabel;
    private readonly System.Windows.Forms.Timer _refreshTimer;
    private ServiceForm? _serviceForm;

    public MainForm(IAppLogger logger)
    {
        _logger = logger;
        _configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
        _configFileExists = File.Exists(_configPath);
        (_settings, _configLoadedSuccessfully) = LoadAppSettings();
        _weighingRepository = EnsureDatabaseReady();
        _weighingRuntime = new SerialServicePortMonitor(_settings.Scale, _settings.Button, _settings.Thresholds, _weighingRepository, _logger);

        Text = "Gestion de Fardos";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 860;
        Height = 480;

        var titleLabel = new Label
        {
            Text = "Gestion de Fardos",
            AutoSize = true,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            Location = new Point(24, 22)
        };

        var operationGroup = new GroupBox
        {
            Text = "Operacion",
            Location = new Point(24, 64),
            Size = new Size(792, 332)
        };

        _currentWeightLabel = new Label
        {
            Text = "Peso actual: -- kg",
            AutoSize = true,
            Font = new Font("Segoe UI", 24F, FontStyle.Bold),
            Location = new Point(24, 40)
        };

        _scaleConnectionLabel = new Label
        {
            Text = "Estado de balanza: --",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular),
            Location = new Point(28, 96)
        };

        _captureStatusLabel = new Label
        {
            Text = "Ultima opresion: Esperando",
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Location = new Point(28, 140)
        };

        _captureMessageLabel = new Label
        {
            Text = "Resultado: --",
            AutoSize = false,
            Size = new Size(736, 44),
            Font = new Font("Segoe UI", 10F, FontStyle.Regular),
            Location = new Point(28, 170)
        };

        _lastCaptureAtLabel = new Label
        {
            Text = "Momento de la ultima opresion: --",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular),
            Location = new Point(28, 226)
        };

        _lastRecordLabel = new Label
        {
            Text = "Ultimo registro guardado: --",
            AutoSize = false,
            Size = new Size(736, 52),
            Font = new Font("Segoe UI", 10F, FontStyle.Regular),
            Location = new Point(28, 258)
        };

        operationGroup.Controls.Add(_currentWeightLabel);
        operationGroup.Controls.Add(_scaleConnectionLabel);
        operationGroup.Controls.Add(_captureStatusLabel);
        operationGroup.Controls.Add(_captureMessageLabel);
        operationGroup.Controls.Add(_lastCaptureAtLabel);
        operationGroup.Controls.Add(_lastRecordLabel);

        Controls.Add(titleLabel);
        Controls.Add(operationGroup);

        _refreshTimer = new System.Windows.Forms.Timer { Interval = 200 };
        _refreshTimer.Tick += (_, _) => RefreshOperationData();

        Load += OnMainFormLoad;
        FormClosed += OnMainFormClosed;
    }

    private (AppSettings Settings, bool WasLoadedSuccessfully) LoadAppSettings()
    {
        var configLoader = new ConfigLoader();

        try
        {
            AppSettings settings = configLoader.Load(AppContext.BaseDirectory);

            if (_configFileExists)
            {
                _logger.Log(AppLogLevel.Info, "CONFIG", $"Configuracion cargada desde {_configPath}.");
            }
            else
            {
                _logger.Log(AppLogLevel.Warning, "CONFIG", $"No se encontro config.json en {_configPath}. La app usara valores por defecto.");
            }

            return (settings, true);
        }
        catch (Exception ex)
        {
            _logger.Log(AppLogLevel.Error, "CONFIG", $"No se pudo cargar {_configPath}: {ex.Message}");

            MessageBox.Show(
                "No se pudo cargar config.json.\n" +
                $"Detalle: {ex.Message}",
                "Error de configuracion",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return (new AppSettings(), false);
        }
    }

    private IWeighingRepository? EnsureDatabaseReady()
    {
        if (!_configFileExists || !_configLoadedSuccessfully)
        {
            _logger.Log(AppLogLevel.Warning, "DB", "La base local no se inicializa porque config.json no esta disponible o no se pudo cargar correctamente.");
            return null;
        }

        try
        {
            var repository = new SqliteWeighingRepository(AppContext.BaseDirectory, _settings.Database, _logger);
            repository.InitializeAsync().GetAwaiter().GetResult();
            _logger.Log(AppLogLevel.Info, "DB", $"Repositorio local preparado en {repository.GetDatabasePath()}.");
            return repository;
        }
        catch (Exception ex)
        {
            _logger.Log(AppLogLevel.Error, "DB", $"No se pudo inicializar la base local: {ex.Message}");

            MessageBox.Show(
                "No se pudo inicializar la base local de pesadas.\n" +
                $"Detalle: {ex.Message}\n\n" +
                "La aplicacion continuara abierta, pero la persistencia local queda pendiente de revision.",
                "Advertencia de base local",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return null;
        }
    }

    private void OnMainFormLoad(object? sender, EventArgs e)
    {
        RegisterServiceHotkeys();
        _weighingRuntime.Start();
        RefreshOperationData();
        _refreshTimer.Start();
    }

    private void OnMainFormClosed(object? sender, FormClosedEventArgs e)
    {
        _refreshTimer.Stop();
        _weighingRuntime.Stop();
        UnregisterHotKey(Handle, HotkeyIdCtrlShiftS);
        UnregisterHotKey(Handle, HotkeyIdCtrlAltShiftS);
    }

    private void RefreshOperationData()
    {
        WeighingRuntimeSnapshot snapshot = _weighingRuntime.GetOperationSnapshot();

        _currentWeightLabel.Text = snapshot.CurrentWeightKg.HasValue
            ? $"Peso actual: {snapshot.CurrentWeightKg.Value:F2} kg"
            : "Peso actual: -- kg";

        _scaleConnectionLabel.Text = snapshot.ScaleIsConnected
            ? "Estado de balanza: Conectada"
            : "Estado de balanza: Desconectada";

        _captureStatusLabel.Text = $"Ultima opresion: {FormatCaptureStatus(snapshot.LastCaptureStatus)}";
        _captureMessageLabel.Text = $"Resultado: {FormatValue(snapshot.LastCaptureMessage)}";
        _lastCaptureAtLabel.Text = snapshot.LastCaptureAt.HasValue
            ? $"Momento de la ultima opresion: {snapshot.LastCaptureAt.Value:dd/MM/yyyy HH:mm:ss.fff}"
            : "Momento de la ultima opresion: --";
        _lastRecordLabel.Text = snapshot.LastSavedRecord is null
            ? "Ultimo registro guardado: --"
            : $"Ultimo registro guardado: Nro {snapshot.LastSavedRecord.Id} - {snapshot.LastSavedRecord.Timestamp:dd/MM/yyyy HH:mm:ss} - {snapshot.LastSavedRecord.WeightKg:F2} kg";
    }

    private void RegisterServiceHotkeys()
    {
        RegisterHotkeyWithWarning(
            HotkeyIdCtrlShiftS,
            ModControl | ModShift,
            VkS,
            "Ctrl+Shift+S");

        RegisterHotkeyWithWarning(
            HotkeyIdCtrlAltShiftS,
            ModControl | ModAlt | ModShift,
            VkS,
            "Ctrl+Alt+Shift+S");
    }

    private void RegisterHotkeyWithWarning(int hotkeyId, uint modifiers, int virtualKey, string displayShortcut)
    {
        bool wasRegistered = RegisterHotKey(Handle, hotkeyId, modifiers, virtualKey);
        if (wasRegistered)
        {
            _logger.Log(AppLogLevel.Info, "SERVICE", $"Hotkey registrada: {displayShortcut}.");
            return;
        }

        _logger.Log(AppLogLevel.Warning, "SERVICE", $"No se pudo registrar la hotkey {displayShortcut}.");

        MessageBox.Show(
            $"No se pudo registrar la hotkey {displayShortcut}.\n" +
            "Es posible que este en uso por otra aplicacion.",
            "Hotkey no registrada",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotKey)
        {
            int hotkeyId = m.WParam.ToInt32();
            if (hotkeyId == HotkeyIdCtrlShiftS || hotkeyId == HotkeyIdCtrlAltShiftS)
            {
                HandleServiceHotkey();
                return;
            }
        }

        base.WndProc(ref m);
    }

    private void HandleServiceHotkey()
    {
        if (!CanOpenServiceMode())
        {
            return;
        }

        if (_serviceForm is not null && !_serviceForm.IsDisposed)
        {
            if (_serviceForm.WindowState == FormWindowState.Minimized)
            {
                _serviceForm.WindowState = FormWindowState.Normal;
            }

            _serviceForm.Activate();
            _logger.Log(AppLogLevel.Info, "SERVICE", "Service ya estaba abierto; se trae al frente.");
            return;
        }

        using var prompt = new ServicePasswordPromptForm();
        DialogResult dialogResult = prompt.ShowDialog(this);
        if (dialogResult != DialogResult.OK)
        {
            return;
        }

        if (!string.Equals(prompt.EnteredPassword, _settings.Passwords.Service, StringComparison.Ordinal))
        {
            _logger.Log(AppLogLevel.Warning, "SERVICE", "Intento de acceso con contrasena incorrecta.");

            MessageBox.Show(
                "La contrasena de Service es incorrecta.",
                "Acceso denegado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        _serviceForm = new ServiceForm(_weighingRuntime);
        _serviceForm.FormClosed += (_, _) =>
        {
            _logger.Log(AppLogLevel.Info, "SERVICE", "Modo Service cerrado.");
            _serviceForm = null;
        };

        _logger.Log(AppLogLevel.Info, "SERVICE", "Modo Service abierto.");
        _serviceForm.Show(this);
        _serviceForm.Activate();
    }

    private bool CanOpenServiceMode()
    {
        if (!_configFileExists)
        {
            _logger.Log(AppLogLevel.Warning, "CONFIG", $"No se encontro config.json en {_configPath}.");

            MessageBox.Show(
                $"No se encontro config.json en la ruta esperada:\n{_configPath}\n\n" +
                "Copie samples/config.example.json junto al ejecutable y complete Passwords.Service.",
                "Acceso a Service bloqueado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_settings.Passwords.Service))
        {
            _logger.Log(AppLogLevel.Warning, "CONFIG", "Passwords.Service esta vacio en config.json.");

            MessageBox.Show(
                "El campo Passwords.Service esta vacio en config.json.\n" +
                "Configure una contrasena de Service para habilitar el acceso.",
                "Acceso a Service bloqueado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        return true;
    }

    private static string FormatCaptureStatus(WeighingCaptureStatus status)
    {
        return status switch
        {
            WeighingCaptureStatus.Idle => "Esperando pulsador",
            WeighingCaptureStatus.Saved => "Pesada guardada",
            WeighingCaptureStatus.RejectedNoWeight => "Rechazada sin peso valido",
            WeighingCaptureStatus.RejectedOutOfRange => "Rechazada fuera de rango",
            WeighingCaptureStatus.SaveError => "Error de guardado",
            _ => "--"
        };
    }

    private static string FormatValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "--" : value;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, int vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
