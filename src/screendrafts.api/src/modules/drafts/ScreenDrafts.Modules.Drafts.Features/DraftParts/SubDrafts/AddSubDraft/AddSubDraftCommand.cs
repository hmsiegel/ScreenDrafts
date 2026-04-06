namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AddSubDraft;

internal sealed record AddSubDraftCommand : ICommand<string>
{
  public required string DraftPartPublicId { get; init; }
  public required int Index { get; init; }
}
