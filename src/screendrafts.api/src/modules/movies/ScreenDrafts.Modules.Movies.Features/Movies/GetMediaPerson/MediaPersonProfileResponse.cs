namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaPerson;

internal sealed record MediaPersonProfileResponse
{
  /// <summary>
  /// The ID to use in the page URL: the TMDb ID when the person has one, otherwise the IMDb ID.
  /// </summary>
  public string Id { get; init; } = default!;
  public string Name { get; init; } = default!;
  public int TmdbId { get; init; }
  public string? ImdbId { get; init; }
  public string? PhotoUrl { get; init; }

  /// <summary>Distinct titles in the Vault the person is credited on, across all roles.</summary>
  public int TitleCount { get; init; }

  /// <summary>One group per role, the role with the most credits first.</summary>
  public IReadOnlyList<MediaPersonCreditGroupResponse> Groups { get; init; } = [];
}

internal sealed record MediaPersonCreditGroupResponse
{
  /// <summary>actor, director, writer or producer.</summary>
  public string Role { get; init; } = default!;
  public IReadOnlyList<MediaPersonCreditResponse> Credits { get; init; } = [];
}

internal sealed record MediaPersonCreditResponse
{
  public string MediaPublicId { get; init; } = default!;
  public string Title { get; init; } = default!;
  public string? Year { get; init; }
  public string? PosterUrl { get; init; }
  public MediaType MediaType { get; init; } = default!;

  /// <summary>Position in TMDb's cast list; null for non-actor roles and unbilled actors.</summary>
  public int? BillingOrder { get; init; }
}
