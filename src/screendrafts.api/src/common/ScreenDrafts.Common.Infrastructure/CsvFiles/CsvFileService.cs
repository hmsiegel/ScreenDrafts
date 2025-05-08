namespace ScreenDrafts.Common.Infrastructure.CsvFiles;

internal sealed class CsvFileService : ICsvFileService
{
  public IEnumerable<T> ReadCsvFile<T>(string filePath)
  {
    using var reader = new StreamReader(filePath);
    using var csv = new CsvReader(
      reader,
      new CsvConfiguration(CultureInfo.InvariantCulture));
    var records = csv.GetRecords<T>();
    return [.. records];
  }
}
