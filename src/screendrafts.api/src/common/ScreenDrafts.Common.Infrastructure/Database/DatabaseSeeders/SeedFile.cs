namespace ScreenDrafts.Common.Infrastructure.Database.DatabaseSeeders;

public sealed class SeedFile(string fileName, SeedFileType fileType)
{
  public string FileName { get; } = fileName;
  public SeedFileType FileType { get; } = fileType;
}
