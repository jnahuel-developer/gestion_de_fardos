namespace GestionDeFardos.App;

public sealed class MainForm : Form
{
    public MainForm()
    {
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
    }
}
