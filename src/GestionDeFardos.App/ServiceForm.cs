namespace GestionDeFardos.App;

public sealed class ServiceForm : Form
{
    public ServiceForm()
    {
        Text = "Modo Service";
        StartPosition = FormStartPosition.CenterParent;
        Width = 700;
        Height = 420;

        var descriptionLabel = new Label
        {
            Text = "Módulo Service - Sin integración de hardware en esta versión",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Location = new Point(20, 20)
        };

        var balanzaGroup = new GroupBox
        {
            Text = "Balanza",
            Location = new Point(20, 60),
            Size = new Size(640, 90)
        };

        var pesoActualLabel = new Label
        {
            Text = "Peso actual: (pendiente)",
            AutoSize = true,
            Location = new Point(16, 30)
        };

        var tramaLabel = new Label
        {
            Text = "Trama ASCII: (pendiente)",
            AutoSize = true,
            Location = new Point(16, 55)
        };

        balanzaGroup.Controls.Add(pesoActualLabel);
        balanzaGroup.Controls.Add(tramaLabel);

        var pulsadorGroup = new GroupBox
        {
            Text = "Pulsador",
            Location = new Point(20, 160),
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
            Location = new Point(20, 260),
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
    }
}
