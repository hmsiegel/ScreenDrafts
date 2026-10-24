namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaPublicIdByImdbId;

internal sealed record GetMediaPublicIdByImdbIdResponse
{
  public string PublicId { get; init; } = default!;
}
