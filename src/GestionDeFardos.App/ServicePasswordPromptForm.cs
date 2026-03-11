namespace GestionDeFardos.App;

public sealed class ServicePasswordPromptForm : Form
{
    private readonly TextBox _passwordTextBox;

    public string EnteredPassword => _passwordTextBox.Text;

    public ServicePasswordPromptForm()
    {
        Text = "Acceso a Modo Service";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Width = 390;
        Height = 170;

        var instructionLabel = new Label
        {
            Text = "Ingrese contraseña de Service",
            AutoSize = true,
            Location = new Point(20, 20)
        };

        _passwordTextBox = new TextBox
        {
            Location = new Point(20, 45),
            Width = 330,
            PasswordChar = '●'
        };

        var acceptButton = new Button
        {
            Text = "Aceptar",
            DialogResult = DialogResult.OK,
            Location = new Point(194, 82),
            Width = 75
        };

        var cancelButton = new Button
        {
            Text = "Cancelar",
            DialogResult = DialogResult.Cancel,
            Location = new Point(275, 82),
            Width = 75
        };

        AcceptButton = acceptButton;
        CancelButton = cancelButton;

        Controls.Add(instructionLabel);
        Controls.Add(_passwordTextBox);
        Controls.Add(acceptButton);
        Controls.Add(cancelButton);
    }
}
