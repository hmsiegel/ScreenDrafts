namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed record CreateDraftHostInput
{
  public required string HostPublicId { get; init; }
  public required int HostRole { get; init; } // 0 = Primary, 1 = CoHost
}
