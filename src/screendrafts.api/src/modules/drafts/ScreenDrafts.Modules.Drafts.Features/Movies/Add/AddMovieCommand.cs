namespace ScreenDrafts.Modules.Drafts.Features.Movies.Add;

internal sealed record AddMovieCommand : ICommand<string>
{
  public Guid Id { get; init; }
  public string PublicId { get; init; } = default!;
  public string Title { get; init; } = default!;
  public string? ImdbId { get; init; } = default!;
  public int? TmdbId { get; init; }
  public int? IgdbId { get; init; }
  public MediaType MediaType { get; init; } = default!;
}

