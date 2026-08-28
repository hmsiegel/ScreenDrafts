namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

public sealed record WriterResponse(Guid Id, string ImdbId, int TmdbId, string Name)
{
  public WriterResponse()
    : this(Guid.Empty, string.Empty, 0, string.Empty) { }
}
