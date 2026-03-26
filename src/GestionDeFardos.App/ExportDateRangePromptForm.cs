namespace GestionDeFardos.App;

public sealed class ExportDateRangePromptForm : Form
{
    private readonly DateTimePicker _fromDatePicker;
    private readonly DateTimePicker _toDatePicker;

    public DateTime FromDate => _fromDatePicker.Value.Date;
    public DateTime ToDate => _toDatePicker.Value.Date;

    public ExportDateRangePromptForm(string exportDirectory)
    {
        Text = "Exportar a Excel";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Width = 440;
        Height = 255;

        var fromLabel = new Label
        {
            Text = "Desde",
            AutoSize = true,
            Location = new Point(20, 20)
        };

        _fromDatePicker = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Location = new Point(20, 44),
            Width = 160,
            Value = DateTime.Today
        };

        var toLabel = new Label
        {
            Text = "Hasta",
            AutoSize = true,
            Location = new Point(210, 20)
        };

        _toDatePicker = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Location = new Point(210, 44),
            Width = 160,
            Value = DateTime.Today
        };

        var destinationLabel = new Label
        {
            Text = "Carpeta de salida",
            AutoSize = true,
            Location = new Point(20, 86)
        };

        var destinationValueLabel = new Label
        {
            Text = exportDirectory,
            AutoSize = false,
            Size = new Size(380, 44),
            Location = new Point(20, 110)
        };

        var acceptButton = new Button
        {
            Text = "Exportar",
            DialogResult = DialogResult.OK,
            Location = new Point(244, 170),
            Width = 75
        };

        var cancelButton = new Button
        {
            Text = "Cancelar",
            DialogResult = DialogResult.Cancel,
            Location = new Point(325, 170),
            Width = 75
        };

        AcceptButton = acceptButton;
        CancelButton = cancelButton;

        Controls.Add(fromLabel);
        Controls.Add(_fromDatePicker);
        Controls.Add(toLabel);
        Controls.Add(_toDatePicker);
        Controls.Add(destinationLabel);
        Controls.Add(destinationValueLabel);
        Controls.Add(acceptButton);
        Controls.Add(cancelButton);
    }
}
