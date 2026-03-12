namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.AddMovie;

internal sealed record AddMovieToDraftPoolCommand : ICommand
{
  public required string PublicId { get; init; }

  public int TmdbId { get; init; }
}
