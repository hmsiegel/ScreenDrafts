namespace ScreenDrafts.Modules.Movies.Application.Movies.Queries.GetMovie;

public sealed record WriterResponse(Guid Id, string ImdbId, string Name)
{
  public WriterResponse()
      : this(Guid.Empty, string.Empty, string.Empty)
  {
  }
}
