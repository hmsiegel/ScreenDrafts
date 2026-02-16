namespace ScreenDrafts.Modules.Drafts.Features.Movies.Add;

internal sealed record AddMovieCommand : ICommand<string>
{
  public Guid Id { get; init; }
  public string ImdbId { get; init; } = default!;
  public string Title { get; init; } = default!;
}

