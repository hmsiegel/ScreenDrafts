namespace ScreenDrafts.Modules.Reporting.Features.Movies.UpdateMovieHonorific;

internal sealed record UpdateMovieHonorificCommand : ICommand
{
  public required string MoviePublicId { get; init; }
  public required string MovieTitle { get; init; }
  public required string DraftPartPublicId { get; init; }
  public required int BoardPosition { get; init; }
}
