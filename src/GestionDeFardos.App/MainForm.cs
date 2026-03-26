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

    private static readonly Color PageBackgroundColor = Color.FromArgb(242, 244, 247);
    private static readonly Color CardColor = Color.White;
    private static readonly Color AccentColor = Color.FromArgb(0, 102, 68);
    private static readonly Color WarningColor = Color.FromArgb(176, 74, 0);
    private static readonly Color ErrorColor = Color.FromArgb(166, 31, 45);
    private static readonly Color MutedColor = Color.FromArgb(94, 102, 112);

    private readonly IAppLogger _logger;
    private readonly AppSettings _settings;
    private readonly string _configPath;
    private readonly bool _configFileExists;
    private readonly bool _configLoadedSuccessfully;
    private readonly IWeighingRepository? _weighingRepository;
    private readonly IWeighingRuntime _weighingRuntime;
    private readonly Label _scaleConnectionLabel;
    private readonly Label _weightUpdatedAtLabel;
    private readonly Label _currentWeightValueLabel;
    private readonly Label _captureStatusLabel;
    private readonly Label _captureMessageLabel;
    private readonly Label _lastCaptureAtLabel;
    private readonly Label _lastRecordLabel;
    private readonly Button _editRecordButton;
    private readonly Button _exportButton;
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
        MinimumSize = new Size(940, 600);
        Size = new Size(940, 600);
        BackColor = PageBackgroundColor;

        var titleLabel = new Label
        {
            Text = "Gestion de Fardos",
            AutoSize = true,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = Color.FromArgb(24, 31, 38),
            Location = new Point(28, 24)
        };

        var subtitleLabel = new Label
        {
            Text = "Operacion en linea con captura automatica al pulsador",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular),
            ForeColor = MutedColor,
            Location = new Point(31, 62)
        };

        Panel weightPanel = BuildCard(new Point(28, 100), new Size(392, 188));
        Panel capturePanel = BuildCard(new Point(444, 100), new Size(452, 188));
        Panel recordPanel = BuildCard(new Point(28, 308), new Size(392, 192));
        Panel actionsPanel = BuildCard(new Point(444, 308), new Size(452, 192));

        var weightTitleLabel = BuildSectionTitle("Peso actual", new Point(22, 20));
        _scaleConnectionLabel = new Label
        {
            Text = "Balanza: --",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = MutedColor,
            Location = new Point(22, 56)
        };

        _weightUpdatedAtLabel = new Label
        {
            Text = "Ultima actualizacion: --",
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            ForeColor = MutedColor,
            Location = new Point(22, 82)
        };

        _currentWeightValueLabel = new Label
        {
            Text = "-- kg",
            AutoSize = true,
            Font = new Font("Segoe UI", 34F, FontStyle.Bold),
            ForeColor = AccentColor,
            Location = new Point(22, 112)
        };

        weightPanel.Controls.Add(weightTitleLabel);
        weightPanel.Controls.Add(_scaleConnectionLabel);
        weightPanel.Controls.Add(_weightUpdatedAtLabel);
        weightPanel.Controls.Add(_currentWeightValueLabel);

        var captureTitleLabel = BuildSectionTitle("Ultima opresion", new Point(22, 20));
        _captureStatusLabel = new Label
        {
            Text = "Esperando pulsador",
            AutoSize = true,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = MutedColor,
            Location = new Point(22, 58)
        };

        _captureMessageLabel = new Label
        {
            Text = "--",
            AutoSize = false,
            Size = new Size(408, 52),
            Font = new Font("Segoe UI", 10F, FontStyle.Regular),
            ForeColor = Color.FromArgb(34, 41, 47),
            Location = new Point(22, 94)
        };

        _lastCaptureAtLabel = new Label
        {
            Text = "Momento de captura: --",
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            ForeColor = MutedColor,
            Location = new Point(22, 154)
        };

        capturePanel.Controls.Add(captureTitleLabel);
        capturePanel.Controls.Add(_captureStatusLabel);
        capturePanel.Controls.Add(_captureMessageLabel);
        capturePanel.Controls.Add(_lastCaptureAtLabel);

        var recordTitleLabel = BuildSectionTitle("Ultimo registro guardado", new Point(22, 20));
        _lastRecordLabel = new Label
        {
            Text = "--",
            AutoSize = false,
            Size = new Size(348, 108),
            Font = new Font("Segoe UI", 10F, FontStyle.Regular),
            ForeColor = Color.FromArgb(34, 41, 47),
            Location = new Point(22, 58)
        };

        recordPanel.Controls.Add(recordTitleLabel);
        recordPanel.Controls.Add(_lastRecordLabel);

        var actionsTitleLabel = BuildSectionTitle("Acciones", new Point(22, 20));
        var actionsDescriptionLabel = new Label
        {
            Text = "Los accesos ya quedan visibles en la pantalla principal para completar los flujos en las siguientes mods.",
            AutoSize = false,
            Size = new Size(408, 48),
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            ForeColor = MutedColor,
            Location = new Point(22, 56)
        };

        _editRecordButton = new Button
        {
            Text = "Editar registro",
            Size = new Size(188, 44),
            Location = new Point(22, 118),
            BackColor = AccentColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            UseVisualStyleBackColor = false
        };
        _editRecordButton.FlatAppearance.BorderSize = 0;
        _editRecordButton.Click += (_, _) => ShowFeaturePendingMessage(
            "Editar registro",
            "La edicion de registros se implementara en mod0008.");

        _exportButton = new Button
        {
            Text = "Exportar a Excel",
            Size = new Size(188, 44),
            Location = new Point(242, 118),
            BackColor = Color.FromArgb(233, 239, 244),
            ForeColor = Color.FromArgb(34, 41, 47),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            UseVisualStyleBackColor = false
        };
        _exportButton.FlatAppearance.BorderColor = Color.FromArgb(209, 216, 223);
        _exportButton.FlatAppearance.BorderSize = 1;
        _exportButton.Click += (_, _) => ShowFeaturePendingMessage(
            "Exportar a Excel",
            "La exportacion a Excel se implementara en mod0009.");

        actionsPanel.Controls.Add(actionsTitleLabel);
        actionsPanel.Controls.Add(actionsDescriptionLabel);
        actionsPanel.Controls.Add(_editRecordButton);
        actionsPanel.Controls.Add(_exportButton);

        Controls.Add(titleLabel);
        Controls.Add(subtitleLabel);
        Controls.Add(weightPanel);
        Controls.Add(capturePanel);
        Controls.Add(recordPanel);
        Controls.Add(actionsPanel);

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

        _currentWeightValueLabel.Text = snapshot.CurrentWeightKg.HasValue
            ? $"{snapshot.CurrentWeightKg.Value:F2} kg"
            : "-- kg";
        _currentWeightValueLabel.ForeColor = snapshot.ScaleIsConnected && snapshot.CurrentWeightKg.HasValue
            ? AccentColor
            : MutedColor;

        _scaleConnectionLabel.Text = snapshot.ScaleIsConnected
            ? "Balanza conectada"
            : "Balanza desconectada";
        _scaleConnectionLabel.ForeColor = snapshot.ScaleIsConnected ? AccentColor : ErrorColor;

        _weightUpdatedAtLabel.Text = snapshot.UpdatedAt == DateTime.MinValue
            ? "Ultima actualizacion: --"
            : $"Ultima actualizacion: {snapshot.UpdatedAt:dd/MM/yyyy HH:mm:ss.fff}";

        _captureStatusLabel.Text = FormatCaptureStatus(snapshot.LastCaptureStatus);
        _captureStatusLabel.ForeColor = ResolveCaptureStatusColor(snapshot.LastCaptureStatus);
        _captureMessageLabel.Text = FormatValue(snapshot.LastCaptureMessage);
        _lastCaptureAtLabel.Text = snapshot.LastCaptureAt.HasValue
            ? $"Momento de captura: {snapshot.LastCaptureAt.Value:dd/MM/yyyy HH:mm:ss.fff}"
            : "Momento de captura: --";

        _lastRecordLabel.Text = snapshot.LastSavedRecord is null
            ? "Aun no hay pesadas guardadas en la base local."
            : $"Numero: {snapshot.LastSavedRecord.Id}\nFecha: {snapshot.LastSavedRecord.Timestamp:dd/MM/yyyy}\nHora: {snapshot.LastSavedRecord.Timestamp:HH:mm:ss}\nPeso: {snapshot.LastSavedRecord.WeightKg:F2} kg";
    }

    private void ShowFeaturePendingMessage(string featureName, string message)
    {
        _logger.Log(AppLogLevel.Info, "UI", $"{featureName} seleccionado en pantalla principal. Pendiente de implementacion.");

        MessageBox.Show(
            message,
            featureName,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
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

    private static Panel BuildCard(Point location, Size size)
    {
        return new Panel
        {
            Location = location,
            Size = size,
            BackColor = CardColor,
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    private static Label BuildSectionTitle(string text, Point location)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(24, 31, 38),
            Location = location
        };
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

    private static Color ResolveCaptureStatusColor(WeighingCaptureStatus status)
    {
        return status switch
        {
            WeighingCaptureStatus.Saved => AccentColor,
            WeighingCaptureStatus.RejectedNoWeight => WarningColor,
            WeighingCaptureStatus.RejectedOutOfRange => WarningColor,
            WeighingCaptureStatus.SaveError => ErrorColor,
            _ => MutedColor
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
