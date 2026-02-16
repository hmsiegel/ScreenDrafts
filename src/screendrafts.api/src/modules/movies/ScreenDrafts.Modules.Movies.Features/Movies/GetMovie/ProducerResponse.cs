namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMovie;

public sealed record ProducerResponse(Guid Id, string ImdbId, string Name)
{
  public ProducerResponse()
      : this(Guid.Empty, string.Empty, string.Empty)
  {
  }
}
