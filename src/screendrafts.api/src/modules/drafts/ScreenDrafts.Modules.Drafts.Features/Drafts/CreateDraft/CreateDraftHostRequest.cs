namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed record CreateDraftHostRequest
{
  public required string HostPublicId { get; init; }
  public required int HostRole { get; init; }
}
