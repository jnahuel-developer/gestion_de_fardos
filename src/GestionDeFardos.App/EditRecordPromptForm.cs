namespace GestionDeFardos.App;

public sealed class EditRecordPromptForm : Form
{
    private readonly NumericUpDown _recordIdInput;
    private readonly TextBox _passwordTextBox;

    public long SelectedRecordId => decimal.ToInt64(_recordIdInput.Value);
    public string EnteredPassword => _passwordTextBox.Text;

    public EditRecordPromptForm()
    {
        Text = "Editar registro";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Width = 410;
        Height = 220;

        var recordIdLabel = new Label
        {
            Text = "Numero de pesada",
            AutoSize = true,
            Location = new Point(20, 20)
        };

        _recordIdInput = new NumericUpDown
        {
            Location = new Point(20, 44),
            Width = 350,
            Minimum = 1,
            Maximum = decimal.MaxValue,
            DecimalPlaces = 0,
            ThousandsSeparator = false
        };

        var passwordLabel = new Label
        {
            Text = "Contrasena de edicion",
            AutoSize = true,
            Location = new Point(20, 84)
        };

        _passwordTextBox = new TextBox
        {
            Location = new Point(20, 108),
            Width = 350,
            PasswordChar = '*'
        };

        var acceptButton = new Button
        {
            Text = "Aceptar",
            DialogResult = DialogResult.OK,
            Location = new Point(214, 146),
            Width = 75
        };

        var cancelButton = new Button
        {
            Text = "Cancelar",
            DialogResult = DialogResult.Cancel,
            Location = new Point(295, 146),
            Width = 75
        };

        AcceptButton = acceptButton;
        CancelButton = cancelButton;

        Controls.Add(recordIdLabel);
        Controls.Add(_recordIdInput);
        Controls.Add(passwordLabel);
        Controls.Add(_passwordTextBox);
        Controls.Add(acceptButton);
        Controls.Add(cancelButton);
    }
}
