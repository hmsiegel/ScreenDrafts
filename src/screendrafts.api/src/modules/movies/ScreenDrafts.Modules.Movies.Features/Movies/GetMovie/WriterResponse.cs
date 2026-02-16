namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMovie;

public sealed record WriterResponse(Guid Id, string ImdbId, string Name)
{
  public WriterResponse()
      : this(Guid.Empty, string.Empty, string.Empty)
  {
  }
}
