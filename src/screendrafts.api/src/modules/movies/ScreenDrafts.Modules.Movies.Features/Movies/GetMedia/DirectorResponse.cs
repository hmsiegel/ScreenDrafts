namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

public sealed record DirectorResponse(
  Guid Id,
  string ImdbId,
  string Name)
{
  public DirectorResponse()
    : this(
        Guid.Empty,
        string.Empty,
        string.Empty)
  {
  }
}
