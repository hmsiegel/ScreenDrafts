namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraftStatus;

internal sealed record Query : IQuery<Response>
{
  public required string DraftPublicId { get; init; }
}
