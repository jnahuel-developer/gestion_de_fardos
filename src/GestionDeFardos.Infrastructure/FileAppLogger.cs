using System.Text;
using GestionDeFardos.Core.Interfaces;

namespace GestionDeFardos.Infrastructure;

public sealed class FileAppLogger : IAppLogger
{
    private readonly string _logsDirectory;
    private readonly object _sync = new();

    public FileAppLogger(string baseDirectory)
    {
        _logsDirectory = Path.Combine(baseDirectory, "logs");
    }

    public void Log(AppLogLevel level, string category, string message)
    {
        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string fileName = $"gestion-de-fardos-{DateTime.Now:yyyyMMdd}.log";
            string filePath = Path.Combine(_logsDirectory, fileName);
            string normalizedCategory = string.IsNullOrWhiteSpace(category)
                ? "APP"
                : category.Trim().ToUpperInvariant();
            string normalizedMessage = (message ?? string.Empty)
                .Replace(Environment.NewLine, " ", StringComparison.Ordinal)
                .Trim();
            string line = $"{timestamp} [{level.ToString().ToUpperInvariant()}] [{normalizedCategory}] {normalizedMessage}{Environment.NewLine}";

            lock (_sync)
            {
                Directory.CreateDirectory(_logsDirectory);
                File.AppendAllText(filePath, line, Encoding.UTF8);
            }
        }
        catch
        {
            // El logger nunca debe romper la lectura serial ni la UI.
        }
    }
}
