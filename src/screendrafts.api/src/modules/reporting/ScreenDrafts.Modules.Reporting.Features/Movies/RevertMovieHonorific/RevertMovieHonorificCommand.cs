namespace ScreenDrafts.Modules.Reporting.Features.Movies.RevertMovieHonorific;

internal sealed record RevertMovieHonorificCommand : ICommand
{
  public required string MoviePublicId { get; init; }
  public required string MovieTitle { get; init; }
  public required string DraftPartPublicId { get; init; }
}
