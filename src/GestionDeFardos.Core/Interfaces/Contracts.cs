using GestionDeFardos.Core.Config;
using GestionDeFardos.Core.Models;

namespace GestionDeFardos.Core.Interfaces;

public interface IScaleReader
{
    void Start();
    void Stop();
    ScaleSnapshot GetSnapshot();
}

public interface IButtonReader
{
    Task<bool> ReadStateAsync(CancellationToken cancellationToken = default);
}

public interface IWeighingRepository
{
    Task SaveAsync(WeighingRecord record, CancellationToken cancellationToken = default);
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
