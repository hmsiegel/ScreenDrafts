namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraftStatus;

internal sealed record GetDraftStatusQuery : IQuery<GetDraftStatusResponse>
{
  public required string DraftPublicId { get; init; }
}
