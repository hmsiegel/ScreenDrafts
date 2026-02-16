namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

public sealed class RequiredMovieVersion
{
  public RequiredMovieVersion(Guid movieId, string versionName)
  {
    MovieId = movieId;
    VersionName = versionName;
  }

  private RequiredMovieVersion()
  {
  }

  public Guid MovieId { get; private set; }
  public string VersionName { get; private set; } = string.Empty;
}
