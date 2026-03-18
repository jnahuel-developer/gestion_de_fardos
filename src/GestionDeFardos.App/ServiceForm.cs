using GestionDeFardos.Core.Interfaces;
using GestionDeFardos.Core.Models;

namespace GestionDeFardos.App;

public sealed class ServiceForm : Form
{
    private readonly IServicePortMonitor _servicePortMonitor;
    private readonly Label _pesoActualLabel;
    private readonly Label _tramaLabel;
    private readonly Label _conexionLabel;
    private readonly Label _errorLabel;
    private readonly Label _buttonLineLabel;
    private readonly Label _ctsLabel;
    private readonly Label _dsrLabel;
    private readonly Label _buttonStateLabel;
    private readonly Label _buttonActivityLabel;
    private readonly System.Windows.Forms.Timer _refreshTimer;

    public ServiceForm(IServicePortMonitor servicePortMonitor)
    {
        _servicePortMonitor = servicePortMonitor;

        Text = "Modo Service";
        StartPosition = FormStartPosition.CenterParent;
        Width = 760;
        Height = 560;

        var descriptionLabel = new Label
        {
            Text = "Modulo Service - Diagnostico serial de balanza y pulsador",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Location = new Point(20, 20)
        };

        var balanzaGroup = new GroupBox
        {
            Text = "Balanza",
            Location = new Point(20, 60),
            Size = new Size(700, 130)
        };

        _pesoActualLabel = new Label
        {
            Text = "Peso convertido: -- kg",
            AutoSize = true,
            Location = new Point(16, 30)
        };

        _tramaLabel = new Label
        {
            Text = "Ultima trama ASCII: --",
            AutoSize = true,
            Location = new Point(16, 55)
        };

        _conexionLabel = new Label
        {
            Text = "Conexion: Desconectada",
            AutoSize = true,
            Location = new Point(16, 80)
        };

        _errorLabel = new Label
        {
            Text = "Error: --",
            AutoSize = true,
            Location = new Point(16, 105)
        };

        balanzaGroup.Controls.Add(_pesoActualLabel);
        balanzaGroup.Controls.Add(_tramaLabel);
        balanzaGroup.Controls.Add(_conexionLabel);
        balanzaGroup.Controls.Add(_errorLabel);

        var pulsadorGroup = new GroupBox
        {
            Text = "Pulsador",
            Location = new Point(20, 205),
            Size = new Size(700, 165)
        };

        _buttonLineLabel = new Label
        {
            Text = "Linea configurada: --",
            AutoSize = true,
            Location = new Point(16, 30)
        };

        _ctsLabel = new Label
        {
            Text = "CTS: --",
            AutoSize = true,
            Location = new Point(16, 55)
        };

        _dsrLabel = new Label
        {
            Text = "DSR: --",
            AutoSize = true,
            Location = new Point(16, 80)
        };

        _buttonStateLabel = new Label
        {
            Text = "Estado del pulsador: Sin lectura",
            AutoSize = true,
            Location = new Point(16, 105)
        };

        _buttonActivityLabel = new Label
        {
            Text = "Ultima opresion / diagnostico: --",
            AutoSize = true,
            Location = new Point(16, 130)
        };

        pulsadorGroup.Controls.Add(_buttonLineLabel);
        pulsadorGroup.Controls.Add(_ctsLabel);
        pulsadorGroup.Controls.Add(_dsrLabel);
        pulsadorGroup.Controls.Add(_buttonStateLabel);
        pulsadorGroup.Controls.Add(_buttonActivityLabel);

        var administracionGroup = new GroupBox
        {
            Text = "Administracion",
            Location = new Point(20, 385),
            Size = new Size(700, 90)
        };

        var borradoButton = new Button
        {
            Text = "Borrado de fardos (pendiente)",
            Enabled = false,
            AutoSize = true,
            Location = new Point(16, 35)
        };

        administracionGroup.Controls.Add(borradoButton);

        Controls.Add(descriptionLabel);
        Controls.Add(balanzaGroup);
        Controls.Add(pulsadorGroup);
        Controls.Add(administracionGroup);

        _refreshTimer = new System.Windows.Forms.Timer { Interval = 200 };
        _refreshTimer.Tick += (_, _) => RefreshServiceData();

        Shown += OnServiceFormShown;
        FormClosed += OnServiceFormClosed;
    }

    private void OnServiceFormShown(object? sender, EventArgs e)
    {
        _servicePortMonitor.Start();
        RefreshServiceData();
        _refreshTimer.Start();
    }

    private void OnServiceFormClosed(object? sender, FormClosedEventArgs e)
    {
        _refreshTimer.Stop();
        _servicePortMonitor.Stop();
    }

    private void RefreshServiceData()
    {
        ServicePortSnapshot snapshot = _servicePortMonitor.GetSnapshot();

        _pesoActualLabel.Text = snapshot.WeightKg.HasValue
            ? $"Peso convertido: {snapshot.WeightKg.Value:F3} kg"
            : "Peso convertido: -- kg";

        _tramaLabel.Text = string.IsNullOrWhiteSpace(snapshot.RawFrame)
            ? "Ultima trama ASCII: --"
            : $"Ultima trama ASCII: {snapshot.RawFrame}";

        _conexionLabel.Text = snapshot.IsConnected
            ? "Conexion: Conectada"
            : "Conexion: Desconectada";

        _errorLabel.Text = string.IsNullOrWhiteSpace(snapshot.LastError)
            ? "Error: --"
            : $"Error: {snapshot.LastError}";

        _buttonLineLabel.Text = $"Linea configurada: {FormatConfiguredLine(snapshot.ConfiguredButtonLine)}";
        _ctsLabel.Text = $"CTS: {FormatControlLineState(snapshot.CtsState)}";
        _dsrLabel.Text = $"DSR: {FormatControlLineState(snapshot.DsrState)}";
        _buttonStateLabel.Text = $"Estado del pulsador: {FormatButtonState(snapshot.ButtonState)}";
        _buttonActivityLabel.Text = FormatButtonActivity(snapshot);
    }

    private static string FormatConfiguredLine(string configuredLine)
    {
        return string.IsNullOrWhiteSpace(configuredLine) ? "--" : configuredLine;
    }

    private static string FormatControlLineState(bool? state)
    {
        return state switch
        {
            true => "Activa",
            false => "Inactiva",
            null => "Sin lectura"
        };
    }

    private static string FormatButtonState(ServiceButtonState buttonState)
    {
        return buttonState switch
        {
            ServiceButtonState.Pressed => "Presionado",
            ServiceButtonState.Released => "No presionado",
            _ => "Sin lectura"
        };
    }

    private static string FormatButtonActivity(ServicePortSnapshot snapshot)
    {
        if (snapshot.LastButtonPressedAt.HasValue)
        {
            return $"Ultima opresion: {snapshot.LastButtonPressedAt.Value:dd/MM/yyyy HH:mm:ss.fff}";
        }

        if (!string.IsNullOrWhiteSpace(snapshot.ButtonLastError))
        {
            return $"Diagnostico: {snapshot.ButtonLastError}";
        }

        return "Ultima opresion / diagnostico: --";
    }
}
