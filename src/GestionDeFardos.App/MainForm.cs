using System.Runtime.InteropServices;
using GestionDeFardos.Core.Config;
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

    private readonly AppSettings _settings;
    private readonly string _configPath;
    private readonly bool _configFileExists;
    private ServiceForm? _serviceForm;

    public MainForm()
    {
        _configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
        _configFileExists = File.Exists(_configPath);
        _settings = LoadAppSettings();

        Text = "Gestión de Fardos";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 800;
        Height = 450;

        var titleLabel = new Label
        {
            Text = "Sistema de Gestión de Fardos",
            AutoSize = true,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            Location = new Point(20, 20)
        };

        var skeletonLabel = new Label
        {
            Text = "Skeleton inicial - sin funcionalidad",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular),
            Location = new Point(20, 70)
        };

        Controls.Add(titleLabel);
        Controls.Add(skeletonLabel);

        Load += OnMainFormLoad;
        FormClosed += OnMainFormClosed;
    }

    private AppSettings LoadAppSettings()
    {
        var configLoader = new ConfigLoader();

        try
        {
            return configLoader.Load(AppContext.BaseDirectory);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "No se pudo cargar config.json.\n" +
                $"Detalle: {ex.Message}",
                "Error de configuración",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return new AppSettings();
        }
    }

    private void OnMainFormLoad(object? sender, EventArgs e)
    {
        RegisterServiceHotkeys();
    }

    private void OnMainFormClosed(object? sender, FormClosedEventArgs e)
    {
        UnregisterHotKey(Handle, HotkeyIdCtrlShiftS);
        UnregisterHotKey(Handle, HotkeyIdCtrlAltShiftS);
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
        var wasRegistered = RegisterHotKey(Handle, hotkeyId, modifiers, virtualKey);
        if (wasRegistered)
        {
            return;
        }

        MessageBox.Show(
            $"No se pudo registrar la hotkey {displayShortcut}.\n" +
            "Es posible que esté en uso por otra aplicación.",
            "Hotkey no registrada",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotKey)
        {
            var hotkeyId = m.WParam.ToInt32();
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
            return;
        }

        using var prompt = new ServicePasswordPromptForm();
        var dialogResult = prompt.ShowDialog(this);
        if (dialogResult != DialogResult.OK)
        {
            return;
        }

        if (!string.Equals(prompt.EnteredPassword, _settings.Passwords.Service, StringComparison.Ordinal))
        {
            MessageBox.Show(
                "La contraseña de Service es incorrecta.",
                "Acceso denegado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var servicePortMonitor = new SerialServicePortMonitor(_settings.Scale, _settings.Button);
        _serviceForm = new ServiceForm(servicePortMonitor);
        _serviceForm.FormClosed += (_, _) => _serviceForm = null;
        _serviceForm.Show(this);
        _serviceForm.Activate();
    }

    private bool CanOpenServiceMode()
    {
        if (!_configFileExists)
        {
            MessageBox.Show(
                $"No se encontró config.json en la ruta esperada:\n{_configPath}\n\n" +
                "Copie samples/config.example.json junto al ejecutable y complete Passwords.Service.",
                "Acceso a Service bloqueado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_settings.Passwords.Service))
        {
            MessageBox.Show(
                "El campo Passwords.Service está vacío en config.json.\n" +
                "Configure una contraseña de Service para habilitar el acceso.",
                "Acceso a Service bloqueado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        return true;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, int vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
