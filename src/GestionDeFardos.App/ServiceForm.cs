using GestionDeFardos.Core.Interfaces;
using GestionDeFardos.Core.Models;
using GestionDeFardos.Core.Utils;

namespace GestionDeFardos.App;

public sealed class ServiceForm : Form
{
    private readonly IWeighingRuntime _weighingRuntime;
    private readonly IWeighingRepository? _weighingRepository;
    private readonly IAppLogger _logger;
    private readonly Label _scaleProtocolLabel;
    private readonly Label _scaleProfileLabel;
    private readonly Label _scaleConnectionLabel;
    private readonly Label _scaleErrorLabel;
    private readonly Label _weightLabel;
    private readonly Label _tareLabel;
    private readonly TextBox _scaleRawTextBox;
    private readonly TextBox _scaleDecodedTextBox;
    private readonly Label _buttonProfileLabel;
    private readonly Label _buttonConnectionLabel;
    private readonly Label _buttonStateLabel;
    private readonly Label _buttonErrorLabel;
    private readonly TextBox _buttonRawTextBox;
    private readonly Label _buttonFrameLabel;
    private readonly Label _buttonResponseLabel;
    private readonly Label _buttonActivityLabel;
    private readonly DateTimePicker _deleteUpToDatePicker;
    private readonly Button _deleteRecordsButton;
    private readonly Label _administrationStatusLabel;
    private readonly System.Windows.Forms.Timer _refreshTimer;
    private bool _deleteInProgress;

    public ServiceForm(IWeighingRuntime weighingRuntime, IWeighingRepository? weighingRepository, IAppLogger logger)
    {
        _weighingRuntime = weighingRuntime;
        _weighingRepository = weighingRepository;
        _logger = logger;

        Text = "Modo Service";
        StartPosition = FormStartPosition.CenterParent;
        Width = 780;
        Height = 790;

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
            Size = new Size(720, 310)
        };

        _scaleProtocolLabel = new Label
        {
            Text = "Protocolo configurado: --",
            AutoSize = true,
            Location = new Point(16, 30)
        };

        _scaleProfileLabel = new Label
        {
            Text = "Puerto y perfil: --",
            AutoSize = true,
            Location = new Point(16, 55)
        };

        _scaleConnectionLabel = new Label
        {
            Text = "Conexion: Desconectada",
            AutoSize = true,
            Location = new Point(16, 80)
        };

        _scaleErrorLabel = new Label
        {
            Text = "Error / diagnostico: --",
            AutoSize = true,
            Location = new Point(16, 105)
        };

        _weightLabel = new Label
        {
            Text = "Peso interpretado: -- kg",
            AutoSize = true,
            Location = new Point(16, 130)
        };

        _tareLabel = new Label
        {
            Text = "Tara interpretada: -- kg",
            AutoSize = true,
            Location = new Point(16, 155)
        };

        var scaleRawCaption = new Label
        {
            Text = "Ultimo chunk crudo recibido:",
            AutoSize = true,
            Location = new Point(16, 185)
        };

        _scaleRawTextBox = BuildReadOnlyTextBox(new Point(16, 205), new Size(680, 38));

        var scaleDecodedCaption = new Label
        {
            Text = "Ultima trama interpretada correctamente:",
            AutoSize = true,
            Location = new Point(16, 252)
        };

        _scaleDecodedTextBox = BuildReadOnlyTextBox(new Point(16, 272), new Size(680, 24));

        balanzaGroup.Controls.Add(_scaleProtocolLabel);
        balanzaGroup.Controls.Add(_scaleProfileLabel);
        balanzaGroup.Controls.Add(_scaleConnectionLabel);
        balanzaGroup.Controls.Add(_scaleErrorLabel);
        balanzaGroup.Controls.Add(_weightLabel);
        balanzaGroup.Controls.Add(_tareLabel);
        balanzaGroup.Controls.Add(scaleRawCaption);
        balanzaGroup.Controls.Add(_scaleRawTextBox);
        balanzaGroup.Controls.Add(scaleDecodedCaption);
        balanzaGroup.Controls.Add(_scaleDecodedTextBox);

        var pulsadorGroup = new GroupBox
        {
            Text = "Pulsador",
            Location = new Point(20, 385),
            Size = new Size(720, 215)
        };

        _buttonProfileLabel = new Label
        {
            Text = "Puerto y perfil: --",
            AutoSize = true,
            Location = new Point(16, 30)
        };

        _buttonConnectionLabel = new Label
        {
            Text = "Conexion: --",
            AutoSize = true,
            Location = new Point(16, 55)
        };

        _buttonStateLabel = new Label
        {
            Text = "Estado del pulsador: --",
            AutoSize = true,
            Location = new Point(16, 80)
        };

        _buttonErrorLabel = new Label
        {
            Text = "Error / diagnostico: --",
            AutoSize = true,
            Location = new Point(16, 105)
        };

        var buttonRawCaption = new Label
        {
            Text = "Ultimo chunk crudo recibido:",
            AutoSize = true,
            Location = new Point(16, 130)
        };

        _buttonRawTextBox = BuildReadOnlyTextBox(new Point(16, 150), new Size(680, 24));

        _buttonFrameLabel = new Label
        {
            Text = "Ultima trama $P1! recibida: --",
            AutoSize = true,
            Location = new Point(16, 180)
        };

        _buttonResponseLabel = new Label
        {
            Text = "Ultima respuesta $B1! enviada: --",
            AutoSize = true,
            Location = new Point(250, 180)
        };

        _buttonActivityLabel = new Label
        {
            Text = "Ultima opresion / diagnostico: --",
            AutoSize = true,
            Location = new Point(16, 200)
        };

        pulsadorGroup.Controls.Add(_buttonProfileLabel);
        pulsadorGroup.Controls.Add(_buttonConnectionLabel);
        pulsadorGroup.Controls.Add(_buttonStateLabel);
        pulsadorGroup.Controls.Add(_buttonErrorLabel);
        pulsadorGroup.Controls.Add(buttonRawCaption);
        pulsadorGroup.Controls.Add(_buttonRawTextBox);
        pulsadorGroup.Controls.Add(_buttonFrameLabel);
        pulsadorGroup.Controls.Add(_buttonResponseLabel);
        pulsadorGroup.Controls.Add(_buttonActivityLabel);

        var administracionGroup = new GroupBox
        {
            Text = "Administracion",
            Location = new Point(20, 615),
            Size = new Size(720, 120)
        };

        var deleteDescriptionLabel = new Label
        {
            Text = "Borrar todos los registros desde la fecha seleccionada hacia atras.",
            AutoSize = true,
            Location = new Point(16, 30)
        };

        _deleteUpToDatePicker = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Location = new Point(16, 56),
            Width = 140,
            Value = DateTime.Today
        };

        _deleteRecordsButton = new Button
        {
            Text = "Borrar hasta fecha",
            AutoSize = true,
            Location = new Point(176, 54),
            Enabled = _weighingRepository is not null
        };
        _deleteRecordsButton.Click += async (_, _) => await HandleDeleteRecordsRequestedAsync();

        _administrationStatusLabel = new Label
        {
            Text = _weighingRepository is null
                ? "Estado: la base local no esta disponible para ejecutar borrados."
                : "Estado: listo para borrar registros por fecha.",
            AutoSize = true,
            Location = new Point(16, 88)
        };

        administracionGroup.Controls.Add(deleteDescriptionLabel);
        administracionGroup.Controls.Add(_deleteUpToDatePicker);
        administracionGroup.Controls.Add(_deleteRecordsButton);
        administracionGroup.Controls.Add(_administrationStatusLabel);

        Controls.Add(descriptionLabel);
        Controls.Add(balanzaGroup);
        Controls.Add(pulsadorGroup);
        Controls.Add(administracionGroup);

        _refreshTimer = new System.Windows.Forms.Timer { Interval = 200 };
        _refreshTimer.Tick += (_, _) => RefreshServiceData();

        Shown += OnServiceFormShown;
        FormClosed += OnServiceFormClosed;
    }

    private static TextBox BuildReadOnlyTextBox(Point location, Size size)
    {
        return new TextBox
        {
            Location = location,
            Size = size,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            WordWrap = false,
            Font = new Font("Consolas", 9F, FontStyle.Regular)
        };
    }

    private void OnServiceFormShown(object? sender, EventArgs e)
    {
        RefreshServiceData();
        _refreshTimer.Start();
    }

    private void OnServiceFormClosed(object? sender, FormClosedEventArgs e)
    {
        _refreshTimer.Stop();
    }

    private void RefreshServiceData()
    {
        ServicePortSnapshot snapshot = _weighingRuntime.GetSnapshot();

        _scaleProtocolLabel.Text = $"Protocolo configurado: {FormatValue(snapshot.ScaleProtocol)}";
        _scaleProfileLabel.Text = $"Puerto y perfil: {FormatValue(snapshot.ScalePortProfile)}";
        _scaleConnectionLabel.Text = snapshot.IsConnected
            ? "Conexion: Conectada"
            : "Conexion: Desconectada";
        _scaleErrorLabel.Text = string.IsNullOrWhiteSpace(snapshot.LastError)
            ? "Error / diagnostico: --"
            : $"Error / diagnostico: {snapshot.LastError}";
        _weightLabel.Text = snapshot.RawGrams.HasValue
            ? $"Peso interpretado: {WeightConversionHelper.GramsToKg(snapshot.RawGrams.Value):F2} kg"
            : "Peso interpretado: -- kg";
        _tareLabel.Text = snapshot.RawTareGrams.HasValue
            ? $"Tara interpretada: {WeightConversionHelper.GramsToKg(snapshot.RawTareGrams.Value):F2} kg"
            : "Tara interpretada: -- kg";
        _scaleRawTextBox.Text = string.IsNullOrWhiteSpace(snapshot.ScaleRawChunk) ? "--" : snapshot.ScaleRawChunk;
        _scaleDecodedTextBox.Text = string.IsNullOrWhiteSpace(snapshot.ScaleLastDecodedFrame) ? "--" : snapshot.ScaleLastDecodedFrame;

        _buttonProfileLabel.Text = $"Puerto y perfil: {FormatValue(snapshot.ButtonPortProfile)}";
        _buttonConnectionLabel.Text = $"Conexion: {FormatButtonConnection(snapshot)}";
        _buttonStateLabel.Text = $"Estado del pulsador: {FormatButtonState(snapshot.ButtonStatus)}";
        _buttonErrorLabel.Text = string.IsNullOrWhiteSpace(snapshot.ButtonLastError)
            ? "Error / diagnostico: --"
            : $"Error / diagnostico: {snapshot.ButtonLastError}";
        _buttonRawTextBox.Text = string.IsNullOrWhiteSpace(snapshot.LastButtonRawChunk) ? "--" : snapshot.LastButtonRawChunk;
        _buttonFrameLabel.Text = string.IsNullOrWhiteSpace(snapshot.LastButtonFrame)
            ? "Ultima trama $P1! recibida: --"
            : $"Ultima trama $P1! recibida: {snapshot.LastButtonFrame}";
        _buttonResponseLabel.Text = string.IsNullOrWhiteSpace(snapshot.LastButtonResponse)
            ? "Ultima respuesta $B1! enviada: --"
            : $"Ultima respuesta $B1! enviada: {snapshot.LastButtonResponse}";
        _buttonActivityLabel.Text = FormatButtonActivity(snapshot);
    }

    private async Task HandleDeleteRecordsRequestedAsync()
    {
        if (_deleteInProgress)
        {
            return;
        }

        if (_weighingRepository is null)
        {
            _administrationStatusLabel.Text = "Estado: la base local no esta disponible para ejecutar borrados.";
            return;
        }

        DateTime selectedDate = _deleteUpToDatePicker.Value.Date;
        DateTime toInclusive = selectedDate.AddDays(1).AddTicks(-1);
        string displayDate = selectedDate.ToString("dd/MM/yyyy");

        DialogResult firstConfirmation = MessageBox.Show(
            $"Se borraran todos los registros con fecha hasta {displayDate} inclusive.\n\nEsta accion no se puede deshacer.\n\nDesea continuar?",
            "Confirmar borrado historico",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (firstConfirmation != DialogResult.Yes)
        {
            _administrationStatusLabel.Text = $"Estado: borrado cancelado para la fecha {displayDate}.";
            return;
        }

        DialogResult secondConfirmation = MessageBox.Show(
            $"Confirmacion final.\n\nSe eliminaran definitivamente todos los registros hasta {displayDate} inclusive.\n\nDesea ejecutar el borrado ahora?",
            "Confirmacion final de borrado",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (secondConfirmation != DialogResult.Yes)
        {
            _administrationStatusLabel.Text = $"Estado: borrado cancelado en la confirmacion final para la fecha {displayDate}.";
            return;
        }

        try
        {
            SetDeleteUiState(true);
            int deletedCount = await _weighingRepository.DeleteUpToAsync(toInclusive);
            _weighingRuntime.RefreshLastSavedRecord();
            _administrationStatusLabel.Text = $"Estado: se borraron {deletedCount} registros hasta {displayDate} inclusive.";

            _logger.Log(AppLogLevel.Info, "SERVICE", $"Borrado historico ejecutado desde Service. FechaHasta={displayDate}, Registros={deletedCount}.");

            MessageBox.Show(
                $"Borrado historico completado.\n\nFecha hasta: {displayDate}\nRegistros eliminados: {deletedCount}",
                "Borrado completado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _administrationStatusLabel.Text = $"Estado: error al borrar hasta {displayDate}.";
            _logger.Log(AppLogLevel.Error, "SERVICE", $"No se pudo ejecutar el borrado historico hasta {displayDate}: {ex.Message}");

            MessageBox.Show(
                "No se pudo ejecutar el borrado historico solicitado.\n" +
                $"Detalle: {ex.Message}",
                "Error de borrado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            SetDeleteUiState(false);
        }
    }

    private static string FormatValue(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "--" : value;
    }

    private static string FormatButtonConnection(ServicePortSnapshot snapshot)
    {
        if (!snapshot.ButtonIsConfigured)
        {
            return "Deshabilitado";
        }

        return snapshot.ButtonIsConnected ? "Conectada" : "Desconectada";
    }

    private static string FormatButtonState(ServiceButtonState buttonState)
    {
        return buttonState switch
        {
            ServiceButtonState.Disabled => "Deshabilitado",
            ServiceButtonState.Disconnected => "Desconectado",
            ServiceButtonState.Listening => "Escuchando $P1!",
            ServiceButtonState.Error => "Error",
            _ => "--"
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
            return $"Ultima opresion / diagnostico: {snapshot.ButtonLastError}";
        }

        return "Ultima opresion / diagnostico: --";
    }

    private void SetDeleteUiState(bool isDeleting)
    {
        _deleteInProgress = isDeleting;
        _deleteRecordsButton.Enabled = !isDeleting && _weighingRepository is not null;
        _deleteUpToDatePicker.Enabled = !isDeleting;
        UseWaitCursor = isDeleting;
        Cursor.Current = isDeleting ? Cursors.WaitCursor : Cursors.Default;
    }
}
