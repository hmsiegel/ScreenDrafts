namespace ScreenDrafts.Common.Features.Abstractions.CsvFiles;

public interface ICsvFileService
{
  IEnumerable<T> ReadCsvFile<T>(string filePath);
}
