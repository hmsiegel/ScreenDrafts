namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaPublicIdByImdbId;

internal sealed record GetMediaPublicIdByImdbIdQuery : IQuery<GetMediaPublicIdByImdbIdResponse>
{
  public required string ImdbId { get; init; }
}
