namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

internal sealed record GetTitleHonorificsRequest
{
  /// <summary>marquee-of-fame, hat-trick, grand-slam or high-five.</summary>
  [FromRoute(Name = "level")]
  public string Level { get; init; } = string.Empty;

  /// <summary>Title contains this text (case-insensitive).</summary>
  [FromQuery(Name = "search")]
  public string? Search { get; init; }

  /// <summary>newest (default), oldest, alphabetical or appearances.</summary>
  [FromQuery(Name = "sort")]
  public string? Sort { get; init; }

  [FromQuery(Name = "page")]
  public int? Page { get; init; }

  /// <summary>1 to 100. Defaults to 50.</summary>
  [FromQuery(Name = "pageSize")]
  public int? PageSize { get; init; }

  /// <summary>Include Patreon and Speed drafts. Only honored for Patreon members.</summary>
  [FromQuery(Name = "includeAll")]
  public bool IncludeAll { get; init; }
}
