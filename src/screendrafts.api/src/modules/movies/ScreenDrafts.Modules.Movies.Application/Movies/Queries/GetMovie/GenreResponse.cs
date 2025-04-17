namespace ScreenDrafts.Modules.Movies.Application.Movies.Queries.GetMovie;

public sealed record GenreResponse(Guid Id, string Name)
{
  public GenreResponse()
      : this(Guid.Empty, string.Empty)
  {
  }
}
