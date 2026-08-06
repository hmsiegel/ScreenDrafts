namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.GetSubDraftGameplay;

internal sealed record GetSubDraftGameplayQuery : IQuery<GetSubDraftGameplayResponse>
{
  public required string DraftPartPublicId { get; init; }
  public required string SubDraftPublicId { get; init; }
}
