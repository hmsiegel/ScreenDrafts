namespace ScreenDrafts.Modules.Movies.Application.Movies.Queries.GetMovie;

public sealed record ProducerResponse(Guid Id, string ImdbId, string Name)
{
  public ProducerResponse()
      : this(Guid.Empty, string.Empty, string.Empty)
  {
  }
}
