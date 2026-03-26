namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

public sealed record GenreResponse(Guid Id, string Name)
{
  public GenreResponse()
      : this(Guid.Empty, string.Empty)
  {
  }
}
