namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.ApplyCommissionerOverride;

internal sealed record ApplyCommissionerOverrideCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required int PlayOrder { get; init; }
}


