namespace ScreenDrafts.Modules.Reporting.Features.Drafts.RecordPartStats;

internal sealed record RecordPartStatsCommand : ICommand
{
  public required DraftPartStatsRecordedIntegrationEvent Stats { get; init; }
}
