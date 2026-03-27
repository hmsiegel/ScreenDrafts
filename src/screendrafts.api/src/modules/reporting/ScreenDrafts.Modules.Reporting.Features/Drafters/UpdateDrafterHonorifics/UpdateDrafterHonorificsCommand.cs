namespace ScreenDrafts.Modules.Reporting.Features.Drafters.UpdateDrafterHonorifics;

internal sealed record UpdateDrafterHonorificsCommand : ICommand
{
  public required Guid DrafterIdValue { get; init; }
  public required string DraftPartPublicId { get; init; }
  public required int CanonicalPolicyValue { get; init; }
  public required bool HasMainFeedRelease { get; init; }
}
