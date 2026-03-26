using ClosedXML.Excel;
using GestionDeFardos.Core.Interfaces;
using GestionDeFardos.Core.Models;

namespace GestionDeFardos.Infrastructure;

public sealed class ClosedXmlReportExporter : IReportExporter
{
    private readonly IAppLogger _logger;

    public ClosedXmlReportExporter(IAppLogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task ExportAsync(IEnumerable<WeighingRecord> records, string outputPath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        List<WeighingRecord> orderedRecords = records
            .OrderBy(record => record.Timestamp)
            .ThenBy(record => record.Id)
            .ToList();

        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            string? directoryPath = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Pesadas");

            worksheet.Cell(1, 1).Value = "Nro de Fardo";
            worksheet.Cell(1, 2).Value = "Dia";
            worksheet.Cell(1, 3).Value = "Hora";
            worksheet.Cell(1, 4).Value = "Kg";

            var headerRange = worksheet.Range(1, 1, 1, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F1EC");

            int rowIndex = 2;
            foreach (WeighingRecord record in orderedRecords)
            {
                worksheet.Cell(rowIndex, 1).Value = record.Id;
                worksheet.Cell(rowIndex, 2).Value = record.Timestamp.ToString("dd/MM/yyyy");
                worksheet.Cell(rowIndex, 3).Value = record.Timestamp.ToString("HH:mm:ss");
                worksheet.Cell(rowIndex, 4).Value = record.WeightKg;
                rowIndex++;
            }

            worksheet.Column(4).Style.NumberFormat.Format = "0.00";
            worksheet.Columns(1, 4).AdjustToContents();
            workbook.SaveAs(outputPath);
        }, cancellationToken);

        _logger.Log(AppLogLevel.Info, "EXPORT", $"Archivo Excel generado. Ruta={outputPath}, Registros={orderedRecords.Count}.");
    }
}
