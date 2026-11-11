namespace ScreenDrafts.Modules.Movies.Features.Movies.ListMediaPeople;

internal sealed record ListMediaPeopleResponse
{
  public required PagedResult<MediaPersonListItemResponse> Result { get; init; }
}

internal sealed record MediaPersonListItemResponse
{
  /// <summary>The ID for /cast-crew/{id}: the TMDb ID when the person has one, otherwise the IMDb ID.</summary>
  public required string Id { get; init; }
  public required string Name { get; init; }

  /// <summary>Distinct titles in the Vault the person is credited on, within the requested role.</summary>
  public required int TitleCount { get; init; }

  /// <summary>Roles the person holds in the Vault (within the requested role): actor, director, writer, producer.</summary>
  public required IReadOnlyList<string> Roles { get; init; }
}
