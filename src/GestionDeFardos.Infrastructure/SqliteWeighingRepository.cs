using System.Globalization;
using GestionDeFardos.Core.Config;
using GestionDeFardos.Core.Interfaces;
using GestionDeFardos.Core.Models;
using Microsoft.Data.Sqlite;

namespace GestionDeFardos.Infrastructure;

public sealed class SqliteWeighingRepository : IWeighingRepository
{
    private const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";

    private readonly string _databasePath;
    private readonly string _connectionString;
    private readonly IAppLogger _logger;

    public SqliteWeighingRepository(string baseDirectory, DatabaseSettings settings, IAppLogger logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(logger);

        _databasePath = ResolveDatabasePath(baseDirectory, settings.FilePath);
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        string? directoryPath = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS WeighingRecords (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Timestamp TEXT NOT NULL,
                WeightKg TEXT NOT NULL,
                RawGrams INTEGER NULL,
                RawFrame TEXT NULL,
                IsEditedToZero INTEGER NOT NULL DEFAULT 0,
                EditedAt TEXT NULL
            );

            CREATE INDEX IF NOT EXISTS IX_WeighingRecords_Timestamp
                ON WeighingRecords (Timestamp);
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.Log(AppLogLevel.Info, "DB", $"Base local lista en {_databasePath}.");
    }

    public async Task<long> SaveAsync(WeighingRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO WeighingRecords
                (Timestamp, WeightKg, RawGrams, RawFrame, IsEditedToZero, EditedAt)
            VALUES
                (@timestamp, @weightKg, @rawGrams, @rawFrame, @isEditedToZero, @editedAt);

            SELECT last_insert_rowid();
            """;

        command.Parameters.AddWithValue("@timestamp", FormatDateTime(record.Timestamp));
        command.Parameters.AddWithValue("@weightKg", record.WeightKg.ToString(CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("@rawGrams", (object?)record.RawGrams ?? DBNull.Value);
        command.Parameters.AddWithValue("@rawFrame", (object?)record.RawFrame ?? DBNull.Value);
        command.Parameters.AddWithValue("@isEditedToZero", record.IsEditedToZero ? 1 : 0);
        command.Parameters.AddWithValue("@editedAt", record.EditedAt is null ? DBNull.Value : FormatDateTime(record.EditedAt.Value));

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        long id = Convert.ToInt64(result, CultureInfo.InvariantCulture);

        _logger.Log(AppLogLevel.Info, "DB", $"Pesada insertada. Id={id}, Timestamp={FormatDateTime(record.Timestamp)}, Kg={record.WeightKg.ToString(CultureInfo.InvariantCulture)}.");
        return id;
    }

    public async Task<WeighingRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "El Id debe ser mayor que cero.");
        }

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, Timestamp, WeightKg, RawGrams, RawFrame, IsEditedToZero, EditedAt
            FROM WeighingRecords
            WHERE Id = @id;
            """;
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? MapRecord(reader)
            : null;
    }

    public async Task<WeighingRecord?> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, Timestamp, WeightKg, RawGrams, RawFrame, IsEditedToZero, EditedAt
            FROM WeighingRecords
            ORDER BY Id DESC
            LIMIT 1;
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? MapRecord(reader)
            : null;
    }

    public async Task<IReadOnlyList<WeighingRecord>> ListByDateRangeAsync(
        DateTime fromInclusive,
        DateTime toInclusive,
        CancellationToken cancellationToken = default)
    {
        if (toInclusive < fromInclusive)
        {
            throw new ArgumentException("La fecha final no puede ser menor a la inicial.");
        }

        var records = new List<WeighingRecord>();

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, Timestamp, WeightKg, RawGrams, RawFrame, IsEditedToZero, EditedAt
            FROM WeighingRecords
            WHERE Timestamp >= @fromInclusive AND Timestamp <= @toInclusive
            ORDER BY Timestamp ASC, Id ASC;
            """;
        command.Parameters.AddWithValue("@fromInclusive", FormatDateTime(fromInclusive));
        command.Parameters.AddWithValue("@toInclusive", FormatDateTime(toInclusive));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            records.Add(MapRecord(reader));
        }

        return records;
    }

    public async Task<bool> SetWeightToZeroAsync(long id, DateTime editedAt, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "El Id debe ser mayor que cero.");
        }

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE WeighingRecords
            SET WeightKg = @weightKg,
                IsEditedToZero = 1,
                EditedAt = @editedAt
            WHERE Id = @id;
            """;

        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@weightKg", 0m.ToString(CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("@editedAt", FormatDateTime(editedAt));

        int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affectedRows > 0)
        {
            _logger.Log(AppLogLevel.Info, "DB", $"Pesada editada a cero. Id={id}, EditedAt={FormatDateTime(editedAt)}.");
        }

        return affectedRows > 0;
    }

    public async Task<int> DeleteUpToAsync(DateTime toInclusive, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            DELETE FROM WeighingRecords
            WHERE Timestamp <= @toInclusive;
            """;
        command.Parameters.AddWithValue("@toInclusive", FormatDateTime(toInclusive));

        int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.Log(AppLogLevel.Info, "DB", $"Borrado historico ejecutado. FechaHasta={FormatDateTime(toInclusive)}, Registros={affectedRows}.");
        return affectedRows;
    }

    public string GetDatabasePath()
    {
        return _databasePath;
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static string ResolveDatabasePath(string baseDirectory, string? configuredPath)
    {
        string candidatePath = string.IsNullOrWhiteSpace(configuredPath)
            ? "data\\gestion-de-fardos.db"
            : configuredPath.Trim();

        return Path.IsPathRooted(candidatePath)
            ? Path.GetFullPath(candidatePath)
            : Path.GetFullPath(Path.Combine(baseDirectory, candidatePath));
    }

    private static string FormatDateTime(DateTime value)
    {
        return value.ToString(TimestampFormat, CultureInfo.InvariantCulture);
    }

    private static DateTime ParseDateTime(string value)
    {
        return DateTime.ParseExact(
            value,
            TimestampFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None);
    }

    private static WeighingRecord MapRecord(SqliteDataReader reader)
    {
        return new WeighingRecord
        {
            Id = reader.GetInt64(0),
            Timestamp = ParseDateTime(reader.GetString(1)),
            WeightKg = decimal.Parse(reader.GetString(2), CultureInfo.InvariantCulture),
            RawGrams = reader.IsDBNull(3) ? null : reader.GetInt32(3),
            RawFrame = reader.IsDBNull(4) ? null : reader.GetString(4),
            IsEditedToZero = reader.GetInt64(5) == 1,
            EditedAt = reader.IsDBNull(6) ? null : ParseDateTime(reader.GetString(6))
        };
    }
}
