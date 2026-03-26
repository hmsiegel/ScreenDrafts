namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

internal sealed record GetMediaQuery : IQuery<MediaResponse>
{
  public required string PublicId { get; init; }
}
