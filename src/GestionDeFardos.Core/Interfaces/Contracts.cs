using GestionDeFardos.Core.Config;
using GestionDeFardos.Core.Models;

namespace GestionDeFardos.Core.Interfaces;

public interface IServicePortMonitor
{
    void Start();
    void Stop();
    ServicePortSnapshot GetSnapshot();
}

public interface IAppLogger
{
    void Log(AppLogLevel level, string category, string message);
}

public enum AppLogLevel
{
    Info,
    Warning,
    Error
}

public interface IWeighingRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<long> SaveAsync(WeighingRecord record, CancellationToken cancellationToken = default);
    Task<WeighingRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<WeighingRecord?> GetLatestAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WeighingRecord>> ListByDateRangeAsync(
        DateTime fromInclusive,
        DateTime toInclusive,
        CancellationToken cancellationToken = default);
    Task<bool> SetWeightToZeroAsync(long id, DateTime editedAt, CancellationToken cancellationToken = default);
    Task<int> DeleteUpToAsync(DateTime toInclusive, CancellationToken cancellationToken = default);
}

public interface IReportExporter
{
    Task ExportAsync(IEnumerable<WeighingRecord> records, string outputPath, CancellationToken cancellationToken = default);
}

public interface IClock
{
    DateTime UtcNow { get; }
}

public interface IConfigLoader
{
    AppSettings Load(string baseDirectory);
}
