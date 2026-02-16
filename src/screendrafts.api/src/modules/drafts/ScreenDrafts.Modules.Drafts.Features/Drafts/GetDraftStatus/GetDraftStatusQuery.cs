namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraftStatus;

internal sealed record GetDraftStatusQuery : IQuery<Response>
{
  public required string DraftPublicId { get; init; }
}

