namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

public sealed record ProducerResponse(Guid Id, string ImdbId, string Name)
{
  public ProducerResponse()
      : this(Guid.Empty, string.Empty, string.Empty)
  {
  }
}
