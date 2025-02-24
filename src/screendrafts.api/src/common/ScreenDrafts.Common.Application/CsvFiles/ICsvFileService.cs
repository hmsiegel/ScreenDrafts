namespace ScreenDrafts.Common.Application.CsvFiles;

public interface ICsvFileService
{
  IEnumerable<T> ReadCsvFile<T>(string filePath);
}
