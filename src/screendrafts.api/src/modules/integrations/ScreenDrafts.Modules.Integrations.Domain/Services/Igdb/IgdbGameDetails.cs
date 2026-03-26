namespace ScreenDrafts.Modules.Integrations.Domain.Services.Igdb;

public sealed record IgdbGameDetails
{
  public int Id { get; init; }
  public string Name { get; init; } = default!;
  public string? Summary { get; init; }
  public Uri? CoverUrl { get; init; }

  public long? FirstReleaseDate { get; init; }

  public IReadOnlyList<string> Genres { get; init; } = [];
}
