namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.AddMovie;

internal sealed record AddMovieToDraftPoolRequest
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }

  public int TmdbId { get; init; }
}
