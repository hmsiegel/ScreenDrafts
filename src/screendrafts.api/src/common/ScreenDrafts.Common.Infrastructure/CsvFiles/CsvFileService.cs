namespace ScreenDrafts.Common.Infrastructure.CsvFiles;

public sealed class CsvFileService : ICsvFileService
{
  public IEnumerable<T> ReadCsvFile<T>(string filePath)
  {
    using var reader = new StreamReader(filePath);
    using var csv = new CsvReader(
      reader,
      new CsvConfiguration(CultureInfo.InvariantCulture));
    return [.. csv.GetRecords<T>()];
  }
}
