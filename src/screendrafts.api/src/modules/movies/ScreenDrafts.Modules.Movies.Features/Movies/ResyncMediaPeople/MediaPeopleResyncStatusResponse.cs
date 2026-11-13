namespace ScreenDrafts.Modules.Movies.Features.Movies.ResyncMediaPeople;

internal sealed record MediaPeopleResyncStatusResponse
{
  /// <summary>Idle, Running, Completed, Cancelled, Aborted or Failed.</summary>
  public string State { get; init; } = default!;

  /// <summary>Titles selected for the current or last run.</summary>
  public int Total { get; init; }
  public int Succeeded { get; init; }
  public int Failed { get; init; }
  public double? PerMinute { get; init; }

  /// <summary>Estimated time left, for example "2h 41m". Only while running.</summary>
  public string? Eta { get; init; }
  public DateTimeOffset? StartedAt { get; init; }
  public DateTimeOffset? FinishedAt { get; init; }

  /// <summary>Why the last run stopped early, or null.</summary>
  public string? StopReason { get; init; }

  /// <summary>Titles a new run would pick up. Only when no run is in progress.</summary>
  public int? Pending { get; init; }
}
