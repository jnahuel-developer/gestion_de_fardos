using GestionDeFardos.Core.Interfaces;

namespace GestionDeFardos.App;

public sealed class ServiceForm : Form
{
    private readonly IScaleReader _scaleReader;
    private readonly Label _pesoActualLabel;
    private readonly Label _tramaLabel;
    private readonly Label _conexionLabel;
    private readonly Label _errorLabel;
    private readonly System.Windows.Forms.Timer _refreshTimer;

    public ServiceForm(IScaleReader scaleReader)
    {
        _scaleReader = scaleReader;

        Text = "Modo Service";
        StartPosition = FormStartPosition.CenterParent;
        Width = 700;
        Height = 450;

        var descriptionLabel = new Label
        {
            Text = "Módulo Service - Lectura de balanza serial",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Location = new Point(20, 20)
        };

        var balanzaGroup = new GroupBox
        {
            Text = "Balanza",
            Location = new Point(20, 60),
            Size = new Size(640, 130)
        };

        _pesoActualLabel = new Label
        {
            Text = "Peso actual: -- kg",
            AutoSize = true,
            Location = new Point(16, 30)
        };

        _tramaLabel = new Label
        {
            Text = "Trama ASCII: --",
            AutoSize = true,
            Location = new Point(16, 55)
        };

        _conexionLabel = new Label
        {
            Text = "Conexión: Desconectada",
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
            Location = new Point(20, 200),
            Size = new Size(640, 90)
        };

        var estadoLabel = new Label
        {
            Text = "Estado: (pendiente)",
            AutoSize = true,
            Location = new Point(16, 30)
        };

        var ultimoEventoLabel = new Label
        {
            Text = "Último evento: (pendiente)",
            AutoSize = true,
            Location = new Point(16, 55)
        };

        pulsadorGroup.Controls.Add(estadoLabel);
        pulsadorGroup.Controls.Add(ultimoEventoLabel);

        var administracionGroup = new GroupBox
        {
            Text = "Administración",
            Location = new Point(20, 300),
            Size = new Size(640, 90)
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

        _refreshTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _refreshTimer.Tick += (_, _) => RefreshScaleData();

        Shown += OnServiceFormShown;
        FormClosed += OnServiceFormClosed;
    }

    private void OnServiceFormShown(object? sender, EventArgs e)
    {
        _scaleReader.Start();
        RefreshScaleData();
        _refreshTimer.Start();
    }

    private void OnServiceFormClosed(object? sender, FormClosedEventArgs e)
    {
        _refreshTimer.Stop();
        _scaleReader.Stop();
    }

    private void RefreshScaleData()
    {
        var snapshot = _scaleReader.GetSnapshot();

        _pesoActualLabel.Text = snapshot.WeightKg.HasValue
            ? $"Peso actual: {snapshot.WeightKg.Value:F3} kg"
            : "Peso actual: -- kg";

        _tramaLabel.Text = string.IsNullOrWhiteSpace(snapshot.RawFrame)
            ? "Trama ASCII: --"
            : $"Trama ASCII: {snapshot.RawFrame}";

        _conexionLabel.Text = snapshot.IsConnected
            ? "Conexión: Conectada"
            : "Conexión: Desconectada";

        _errorLabel.Text = string.IsNullOrWhiteSpace(snapshot.LastError)
            ? "Error: --"
            : $"Error: {snapshot.LastError}";
    }
}
