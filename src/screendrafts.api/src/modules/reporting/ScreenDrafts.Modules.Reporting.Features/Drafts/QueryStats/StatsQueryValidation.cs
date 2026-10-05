namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

internal sealed record StatsQueryValidation
{
  public StatsQuerySpec? Spec { get; private init; }
  public SDError? Error { get; private init; }

  public static StatsQueryValidation Ok(StatsQuerySpec spec) => new() { Spec = spec };

  public static StatsQueryValidation Fail(SDError error) => new() { Error = error };
}
